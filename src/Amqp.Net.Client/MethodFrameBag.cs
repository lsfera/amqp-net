using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amqp.Net.Client.Extensions;
using Amqp.Net.Client.Frames;

namespace Amqp.Net.Client
{
    internal class MethodFrameBag : IMethodFrameBag
    {
        // TODO: be sure it won't be evaluated all the times
        private static readonly IDictionary<MethodFrameDescriptor, MethodFrameDictionary> DescriptorsMap =
            MethodFrameDescriptor.AvailableDescriptors
                                 .Select(_ => new KeyValuePair<MethodFrameDescriptor, MethodFrameDictionary>(_, new MethodFrameDictionary()))
                                 .ToDictionary(_ => _.Key, _ => _.Value);

        internal interface IMethodFrameDictionary
        {
            TaskCompletionSource<IFrame> Pop(Int16 channelIndex);

            Task<T> WaitForAsync<T>(Int16 channelIndex)
                where T : IFrame;
        }

        internal class MethodFrameDictionary : IMethodFrameDictionary
        {
            private readonly ConcurrentDictionary<Int16, BlockingCollection<TaskCompletionSource<IFrame>>> ChannelsMap =
                new ConcurrentDictionary<Int16, BlockingCollection<TaskCompletionSource<IFrame>>>();

            private BlockingCollection<TaskCompletionSource<IFrame>> AtChannelIndex(Int16 channelIndex)
            {
                return ChannelsMap.GetOrAdd(channelIndex,
                                            new BlockingCollection<TaskCompletionSource<IFrame>>(new ConcurrentQueue<TaskCompletionSource<IFrame>>()));
            }

            public TaskCompletionSource<IFrame> Pop(Int16 channelIndex)
            {
                return AtChannelIndex(channelIndex).Take();
            }

            public Task<T> WaitForAsync<T>(Int16 channelIndex)
                where T : IFrame
            {
                var tcs = new TaskCompletionSource<IFrame>();
                AtChannelIndex(channelIndex).Add(tcs);
                
                return tcs.Task.Then(_ => (T)_);
            }
        }

        public IMethodFrameDictionary For(MethodFrameDescriptor descriptor)
        {
            return DescriptorsMap.ContainsKey(descriptor)
                       ? DescriptorsMap[descriptor]
                       : throw new NotSupportedException($"class-id '{descriptor.ClassId}' and method-id '{descriptor.MethodId}' are not supported");
        }
    }
}