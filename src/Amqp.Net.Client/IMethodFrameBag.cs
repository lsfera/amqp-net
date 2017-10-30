using Amqp.Net.Client.Frames;

namespace Amqp.Net.Client
{
    internal interface IMethodFrameBag
    {
        MethodFrameBag.IFrameHandler<RpcContext> Rpc(MethodFrameDescriptor descriptor);

        MethodFrameBag.IFrameHandler<AsyncContext> Async(MethodFrameDescriptor descriptor);
    }
}