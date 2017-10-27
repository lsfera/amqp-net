using Amqp.Net.Client.Frames;

namespace Amqp.Net.Client
{
    internal interface IMethodFrameBag
    {
        MethodFrameBag.IMethodFrameDictionary For(MethodFrameDescriptor descriptor);
    }
}