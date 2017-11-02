using System;
using System.Collections.Generic;
using System.Linq;
using Amqp.Net.Client.Entities;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Frames
{
    internal class FrameParser : IFrameParser
    {
        private static readonly IDictionary<FrameType, Func<FrameHeader, IByteBuffer, IEnumerable<IFrame>, IFrame>> Map =
            new Dictionary<FrameType, Func<FrameHeader, IByteBuffer, IEnumerable<IFrame>, IFrame>>
                {
                    { FrameType.METHOD, (header, buffer, children) => MethodFrameDescriptor.Parse(buffer).BuildFrame(header, buffer, children) },
                    { FrameType.HEADER, (header, buffer, children) => HeaderFrame.Parse(header, buffer) },
                    { FrameType.BODY, (header, buffer, children) => BodyFrame.Parse(header, buffer) }
                };

        public IFrame Parse(IByteBuffer buffer)
        {
            var tuple = ReadTuple(buffer);
            var children = new List<Tuple<FrameHeader, IByteBuffer>>();

            while (buffer.ReadableBytes > 0)
                children.Add(ReadTuple(buffer));

            return FrameMap(tuple, children.Select(_ => FrameMap(_)).ToList());
        }

        private static IFrame FrameMap(Tuple<FrameHeader, IByteBuffer> tuple,
                                       IEnumerable<IFrame> children = null)
        {
            if (!Map.ContainsKey(tuple.Item1.Type))
                throw new NotSupportedException($"frame of type {tuple.Item1.Type} is not supported");

            return Map[tuple.Item1.Type](tuple.Item1, tuple.Item2, children ?? new List<IFrame>());
        }

        private static Tuple<FrameHeader, IByteBuffer> ReadTuple(IByteBuffer buffer)
        {
            var header = FrameHeader.Parse(buffer);
            var data = buffer.ReadBytes(buffer.ReadInt());
            buffer.ReadByte();

            return new Tuple<FrameHeader, IByteBuffer>(header, data);
        }
    }
}