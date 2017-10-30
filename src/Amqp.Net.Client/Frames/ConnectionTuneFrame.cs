using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ConnectionTuneFrame : MethodFrame<ConnectionTune, RpcContext>
    {
        internal ConnectionTuneFrame(Int16 channel, ConnectionTune payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);

        // TODO: make an extension method?
        internal ConnectionTuneOkFrame ToConnectionTuneOkFrame()
        {
            return new ConnectionTuneOkFrame(Header.ChannelIndex,
                                             new ConnectionTuneOk(Payload.ChannelMax,
                                                                  Payload.FrameMax,
                                                                  Payload.Heartbeat));
        }
    }
}