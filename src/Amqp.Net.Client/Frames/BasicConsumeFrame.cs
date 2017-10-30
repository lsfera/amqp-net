﻿using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class BasicConsumeFrame : MethodFrame<BasicConsume, RpcContext>
    {
        internal BasicConsumeFrame(Int16 channelIndex, BasicConsume payload)
            : base(new FrameHeader(FrameType.METHOD, channelIndex), payload)
        {
        }

        public override RpcContext Context => new RpcContext(this);
    }
}