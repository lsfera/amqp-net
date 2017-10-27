﻿using System;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Payloads;

namespace Amqp.Net.Client.Frames
{
    internal class ConnectionOpenOkFrame : MethodFrame<ConnectionOpenOk>
    {
        internal ConnectionOpenOkFrame(Int16 channel, ConnectionOpenOk payload)
            : base(new FrameHeader(FrameType.METHOD, channel), payload)
        {
        }
    }
}