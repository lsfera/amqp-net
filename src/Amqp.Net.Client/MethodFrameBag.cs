using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Amqp.Net.Client.Extensions;
using Amqp.Net.Client.Frames;

namespace Amqp.Net.Client
{
    internal interface IFrameHandlerResult
    {
        void Handle(IFrame frame);

        Task<IFrame> Result { get; }
    }

    internal class RpcFrameHandlerResult<TFrame> : IFrameHandlerResult
        where TFrame : class, IFrame
    {
        private readonly TaskCompletionSource<TFrame> tcs;

        public RpcFrameHandlerResult(TaskCompletionSource<TFrame> tcs)
        {
            this.tcs = tcs;
        }

        public void Handle(IFrame frame)
        {
            tcs.SetResult((TFrame)frame);
        }

        public Task<IFrame> Result => tcs.Task.Then<TFrame, IFrame>(_ => _);
    }

    internal class ConsumeFrameHandlerResult<TFrame> : IFrameHandlerResult
        where TFrame : class, IFrame
    {
        private readonly IFrame sourceFrame;
        private readonly Action<TFrame> action;

        public ConsumeFrameHandlerResult(IFrame sourceFrame, Action<TFrame> action)
        {
            this.sourceFrame = sourceFrame;
            this.action = action;
        }

        public void Handle(IFrame frame)
        {
            action((TFrame)frame);
        }

        public Task<IFrame> Result => Task.FromResult(sourceFrame);
    }

    internal class MethodFrameBag : IMethodFrameBag
    {
        private static readonly ConcurrentDictionary<MethodFrameDescriptor, IFrameHandler<RpcContext>> RpcDescriptorsMap =
            new ConcurrentDictionary<MethodFrameDescriptor, IFrameHandler<RpcContext>>();

        private static readonly ConcurrentDictionary<MethodFrameDescriptor, IFrameHandler<ConsumeContext>> ConsumeDescriptorsMap =
            new ConcurrentDictionary<MethodFrameDescriptor, IFrameHandler<ConsumeContext>>();

        internal interface IFrameHandler<in TContext>
            where TContext : IFrameContext
        {
            IFrameHandlerResult Pop(TContext context);

            Task<TFrame> Register<TFrame>(TContext context, Func<IFrameHandlerResult> func)
                where TFrame : class, IFrame;
        }

        internal class RpcFrameHandler : IFrameHandler<RpcContext>
        {
            private readonly ConcurrentDictionary<Int16, BlockingCollection<IFrameHandlerResult>> ChannelsMap =
                new ConcurrentDictionary<Int16, BlockingCollection<IFrameHandlerResult>>();

            private BlockingCollection<IFrameHandlerResult> AtChannelIndex(Int16 channelIndex)
            {
                return ChannelsMap.GetOrAdd(channelIndex,
                                            new BlockingCollection<IFrameHandlerResult>(new ConcurrentQueue<IFrameHandlerResult>()));
            }

            public IFrameHandlerResult Pop(RpcContext context)
            {
                return AtChannelIndex(context.ChannelIndex).Take();
            }

            public Task<TFrame> Register<TFrame>(RpcContext context, Func<IFrameHandlerResult> func)
                where TFrame : class, IFrame
            {
                var result = func();
                AtChannelIndex(context.ChannelIndex).Add(result);
                
                return result.Result.Cast<TFrame>();
            }
        }

        internal class ConsumeFrameHandler : IFrameHandler<ConsumeContext>
        {
            private readonly ConcurrentDictionary<Int16, ConcurrentDictionary<String, BlockingCollection<IFrameHandlerResult>>> ChannelsMap =
                new ConcurrentDictionary<Int16, ConcurrentDictionary<String, BlockingCollection<IFrameHandlerResult>>>();

            public IFrameHandlerResult Pop(ConsumeContext context)
            {
                var consumers = WithConsumer(AtChannelIndex(context.ChannelIndex),
                                             context.ConsumerTag);

                return consumers.Take();
            }

            public Task<TFrame> Register<TFrame>(ConsumeContext context, Func<IFrameHandlerResult> func)
                where TFrame : class, IFrame
            {
                var consumers = WithConsumer(AtChannelIndex(context.ChannelIndex),
                                             context.ConsumerTag);

                var result = func();
                consumers.Add(result);

                return result.Result.Cast<TFrame>();
            }

            private static BlockingCollection<IFrameHandlerResult> WithConsumer(ConcurrentDictionary<String, BlockingCollection<IFrameHandlerResult>> channels,
                                                                                String consumerTag)
            {
                return channels.GetOrAdd(consumerTag,
                                         new BlockingCollection<IFrameHandlerResult>(new ConcurrentQueue<IFrameHandlerResult>()));
            }

            private ConcurrentDictionary<String, BlockingCollection<IFrameHandlerResult>> AtChannelIndex(Int16 channelIndex)
            {
                return ChannelsMap.GetOrAdd(channelIndex,
                                            new ConcurrentDictionary<String, BlockingCollection<IFrameHandlerResult>>());
            }
        }

        public IFrameHandler<RpcContext> OnRpc(MethodFrameDescriptor descriptor)
        {
            return RpcDescriptorsMap.GetOrAdd(descriptor, new RpcFrameHandler());
        }

        public IFrameHandler<ConsumeContext> OnConsume(MethodFrameDescriptor descriptor)
        {
            return ConsumeDescriptorsMap.GetOrAdd(descriptor, new ConsumeFrameHandler());
        }
    }

    internal static class FrameHandlerExtensions
    {
        internal static Task<TFrame> Register<TFrame>(this MethodFrameBag.IFrameHandler<RpcContext> handler,
                                                      RpcContext context)
            where TFrame : class, IFrame
        {
            IFrameHandlerResult Func() => new RpcFrameHandlerResult<TFrame>(new TaskCompletionSource<TFrame>());

            return handler.Register<TFrame>(context, Func);
        }

        internal static Task<TFrame> Register<TFrame>(this MethodFrameBag.IFrameHandler<ConsumeContext> handler,
                                                      ConsumeContext context,
                                                      IFrame sourceFrame,
                                                      Action<TFrame> action)
            where TFrame : class, IFrame
        {
            IFrameHandlerResult Func() => new ConsumeFrameHandlerResult<TFrame>(sourceFrame, action);

            return handler.Register<TFrame>(context, Func);
        }
    }
}