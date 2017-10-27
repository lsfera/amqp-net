﻿using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ChannelOpenFrame : MethodFrame<ChannelOpen>
    {
        internal ChannelOpenFrame(Int16 channel, ChannelOpen payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }
    }
}