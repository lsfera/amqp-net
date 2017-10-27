using System;
using System.Collections.Generic;
using Amqp.Net.Client.Entities;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Frames
{
    internal class FrameParser : IFrameParser
    {
        private static readonly IDictionary<FrameType, Func<FrameHeader, IByteBuffer, IFrame>> Map =
            new Dictionary<FrameType, Func<FrameHeader, IByteBuffer, IFrame>>
                {
                    { FrameType.METHOD, (header, buffer) => MethodFrameDescriptor.Parse(buffer).BuildFrame(header, buffer) }
                };

        public IFrame Parse(IByteBuffer buffer)
        {
            var header = FrameHeader.Parse(buffer);
            var data = buffer.ReadBytes(buffer.ReadInt());
            buffer.ReadByte();

            return !Map.ContainsKey(header.Type)
                       ? throw new NotSupportedException($"frame of type {header.Type} is not supported")
                       : Map[header.Type](header, data);
        }
    }
}