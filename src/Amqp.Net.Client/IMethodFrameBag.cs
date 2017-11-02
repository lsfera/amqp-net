using Amqp.Net.Client.Frames;

namespace Amqp.Net.Client
{
    internal interface IMethodFrameBag
    {
        MethodFrameBag.IFrameHandler<RpcContext> OnRpc(MethodFrameDescriptor descriptor);

        MethodFrameBag.IFrameHandler<ConsumeContext> OnConsume(MethodFrameDescriptor descriptor);
    }
}