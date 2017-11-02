using System;
using System.Collections.Generic;
using System.Linq;
using Amqp.Net.Client.Entities;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Frames
{
    internal class FrameParser : IFrameParser
    {
        private static readonly IDictionary<FrameType, Func<FrameHeader, IByteBuffer, IFrame>> Map =
            new Dictionary<FrameType, Func<FrameHeader, IByteBuffer, IFrame>>
                {
                    { FrameType.METHOD, (header, buffer) => MethodFrameDescriptor.Parse(buffer).BuildFrame(header, buffer) },
                    { FrameType.HEADER, HeaderFrame.Parse },
                    { FrameType.BODY, BodyFrame.Parse }
                };

        public IFrame Parse(IByteBuffer buffer)
        {
            var pairs = new List<Tuple<FrameHeader, IByteBuffer>>();

            while (buffer.ReadableBytes > 0)
            {
                var header = FrameHeader.Parse(buffer);
                var data = buffer.ReadBytes(buffer.ReadInt());
                buffer.ReadByte();
                pairs.Add(new Tuple<FrameHeader, IByteBuffer>(header, data));
            }

            var frames = pairs.Select(_ =>
                                      {
                                          if (!Map.ContainsKey(_.Item1.Type))
                                              throw new NotSupportedException($"frame of type {_.Item1.Type} is not supported");

                                          return Map[_.Item1.Type](_.Item1, _.Item2);
                                      })
                              .ToList();

            return frames.FirstOrDefault();
        }
    }
}