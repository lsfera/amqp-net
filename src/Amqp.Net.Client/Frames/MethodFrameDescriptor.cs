using System;
using System.Collections.Generic;
using System.Linq;
using Amqp.Net.Client.Decoding;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Frames
{
    internal struct MethodFrameDescriptor
    {
        internal readonly Int16 ClassId;
        internal readonly Int16 MethodId;

        private static readonly IDictionary<MethodFrameDescriptor, Func<FrameHeader, IByteBuffer, IFrame>> map =
            new Dictionary<MethodFrameDescriptor, Func<FrameHeader, IByteBuffer, IFrame>>
                {
                    // connection class
                    { Payloads.ConnectionStart.StaticDescriptor, (header, buffer) => new ConnectionStartFrame(header.ChannelIndex,
                                                                                                              Payloads.ConnectionStart.Parse(buffer)) },
                    { Payloads.ConnectionStartOk.StaticDescriptor, (header, buffer) => new ConnectionStartOkFrame(header.ChannelIndex,
                                                                                                                  Payloads.ConnectionStartOk.Parse(buffer)) },
                    { Payloads.ConnectionTune.StaticDescriptor, (header, buffer) => new ConnectionTuneFrame(header.ChannelIndex,
                                                                                                            Payloads.ConnectionTune.Parse(buffer)) },
                    { Payloads.ConnectionTuneOk.StaticDescriptor, (header, buffer) => new ConnectionTuneOkFrame(header.ChannelIndex,
                                                                                                                Payloads.ConnectionTuneOk.Parse(buffer)) },
                    { Payloads.ConnectionOpen.StaticDescriptor, (header, buffer) => new ConnectionOpenFrame(header.ChannelIndex,
                                                                                                            Payloads.ConnectionOpen.Parse(buffer)) },
                    { Payloads.ConnectionOpenOk.StaticDescriptor, (header, buffer) => new ConnectionOpenOkFrame(header.ChannelIndex,
                                                                                                                Payloads.ConnectionOpenOk.Parse(buffer)) },
                    { Payloads.ConnectionClose.StaticDescriptor, (header, buffer) => new ConnectionCloseFrame(header.ChannelIndex,
                                                                                                              Payloads.ConnectionClose.Parse(buffer)) },
                    { Payloads.ConnectionCloseOk.StaticDescriptor, (header, buffer) => new ConnectionCloseOkFrame(header.ChannelIndex,
                                                                                                                  Payloads.ConnectionCloseOk.Parse(buffer)) },
                    // channel class
                    { Payloads.ChannelOpen.StaticDescriptor, (header, buffer) => new ChannelOpenFrame(header.ChannelIndex,
                                                                                                      Payloads.ChannelOpen.Parse(buffer)) },
                    { Payloads.ChannelOpenOk.StaticDescriptor, (header, buffer) => new ChannelOpenOkFrame(header.ChannelIndex,
                                                                                                          Payloads.ChannelOpenOk.Parse(buffer)) },
                    { Payloads.ChannelClose.StaticDescriptor, (header, buffer) => new ChannelCloseFrame(header.ChannelIndex,
                                                                                                        Payloads.ChannelClose.Parse(buffer)) },
                    { Payloads.ChannelCloseOk.StaticDescriptor, (header, buffer) => new ChannelCloseOkFrame(header.ChannelIndex,
                                                                                                            Payloads.ChannelCloseOk.Parse(buffer)) },
                    // exchange class
                    { Payloads.ExchangeDeclare.StaticDescriptor, (header, buffer) => new ExchangeDeclareFrame(header.ChannelIndex,
                                                                                                              Payloads.ExchangeDeclare.Parse(buffer)) },
                    { Payloads.ExchangeDeclareOk.StaticDescriptor, (header, buffer) => new ExchangeDeclareOkFrame(header.ChannelIndex,
                                                                                                                  Payloads.ExchangeDeclareOk.Parse(buffer)) },
                    { Payloads.ExchangeBind.StaticDescriptor, (header, buffer) => new ExchangeBindFrame(header.ChannelIndex,
                                                                                                        Payloads.ExchangeBind.Parse(buffer)) },
                    { Payloads.ExchangeBindOk.StaticDescriptor, (header, buffer) => new ExchangeBindOkFrame(header.ChannelIndex,
                                                                                                            Payloads.ExchangeBindOk.Parse(buffer)) },
                    { Payloads.ExchangeUnbind.StaticDescriptor, (header, buffer) => new ExchangeUnbindFrame(header.ChannelIndex,
                                                                                                            Payloads.ExchangeUnbind.Parse(buffer)) },
                    { Payloads.ExchangeUnbindOk.StaticDescriptor, (header, buffer) => new ExchangeUnbindOkFrame(header.ChannelIndex,
                                                                                                                Payloads.ExchangeUnbindOk.Parse(buffer)) },
                    { Payloads.ExchangeDelete.StaticDescriptor, (header, buffer) => new ExchangeDeleteFrame(header.ChannelIndex,
                                                                                                            Payloads.ExchangeDelete.Parse(buffer)) },
                    { Payloads.ExchangeDeleteOk.StaticDescriptor, (header, buffer) => new ExchangeDeleteOkFrame(header.ChannelIndex,
                                                                                                                Payloads.ExchangeDeleteOk.Parse(buffer)) },
                    // queue class
                    { Payloads.QueueDeclare.StaticDescriptor, (header, buffer) => new QueueDeclareFrame(header.ChannelIndex,
                                                                                                        Payloads.QueueDeclare.Parse(buffer)) },
                    { Payloads.QueueDeclareOk.StaticDescriptor, (header, buffer) => new QueueDeclareOkFrame(header.ChannelIndex,
                                                                                                            Payloads.QueueDeclareOk.Parse(buffer)) },
                    { Payloads.QueueBind.StaticDescriptor, (header, buffer) => new QueueBindFrame(header.ChannelIndex,
                                                                                                  Payloads.QueueBind.Parse(buffer)) },
                    { Payloads.QueueBindOk.StaticDescriptor, (header, buffer) => new QueueBindOkFrame(header.ChannelIndex,
                                                                                                      Payloads.QueueBindOk.Parse(buffer)) },
                    { Payloads.QueueUnbind.StaticDescriptor, (header, buffer) => new QueueUnbindFrame(header.ChannelIndex,
                                                                                                      Payloads.QueueUnbind.Parse(buffer)) },
                    { Payloads.QueueUnbindOk.StaticDescriptor, (header, buffer) => new QueueUnbindOkFrame(header.ChannelIndex,
                                                                                                          Payloads.QueueUnbindOk.Parse(buffer)) },
                    { Payloads.QueueDelete.StaticDescriptor, (header, buffer) => new QueueDeleteFrame(header.ChannelIndex,
                                                                                                      Payloads.QueueDelete.Parse(buffer)) },
                    { Payloads.QueueDeleteOk.StaticDescriptor, (header, buffer) => new QueueDeleteOkFrame(header.ChannelIndex,
                                                                                                          Payloads.QueueDeleteOk.Parse(buffer)) },
                    // basic class
                    { Payloads.BasicQos.StaticDescriptor, (header, buffer) => new BasicQosFrame(header.ChannelIndex,
                                                                                                Payloads.BasicQos.Parse(buffer)) },
                    { Payloads.BasicQosOk.StaticDescriptor, (header, buffer) => new BasicQosOkFrame(header.ChannelIndex,
                                                                                                    Payloads.BasicQosOk.Parse(buffer)) },
                    { Payloads.BasicConsume.StaticDescriptor, (header, buffer) => new BasicConsumeFrame(header.ChannelIndex,
                                                                                                        Payloads.BasicConsume.Parse(buffer)) },
                    { Payloads.BasicConsumeOk.StaticDescriptor, (header, buffer) => new BasicConsumeOkFrame(header.ChannelIndex,
                                                                                                            Payloads.BasicConsumeOk.Parse(buffer)) },
                    { Payloads.BasicDeliver.StaticDescriptor, (header, buffer) => new BasicDeliverFrame(header.ChannelIndex,
                                                                                                        Payloads.BasicDeliver.Parse(buffer)) }
                };

        // TODO: be sure it won't be evaluated every time
        internal static readonly IEnumerable<MethodFrameDescriptor> AvailableDescriptors = map.Select(_ => _.Key);

        internal IFrame BuildFrame(FrameHeader header, IByteBuffer buffer)
        {
            return map[this](header, buffer);
        }

        internal static MethodFrameDescriptor Parse(IByteBuffer buffer)
        {
            return new MethodFrameDescriptor(buffer.ReadShort(), buffer.ReadShort());
        }

        internal MethodFrameDescriptor(Int16 classId, Int16 methodId)
        {
            ClassId = classId;
            MethodId = methodId;
        }

        internal void Write(IByteBuffer buffer)
        {
            Int16FieldValueCodec.Instance.Encode(ClassId, buffer);
            Int16FieldValueCodec.Instance.Encode(MethodId, buffer);
        }

        public Boolean Equals(MethodFrameDescriptor other)
        {
            return ClassId == other.ClassId && MethodId == other.MethodId;
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is MethodFrameDescriptor descriptor && Equals(descriptor);
        }

        public override Int32 GetHashCode()
        {
            unchecked
            {
                return (ClassId.GetHashCode() * 397) ^ MethodId.GetHashCode();
            }
        }

        public static Boolean operator ==(MethodFrameDescriptor left, MethodFrameDescriptor right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(MethodFrameDescriptor left, MethodFrameDescriptor right)
        {
            return !left.Equals(right);
        }

        public override String ToString()
        {
            return $"{{\"class_id\":{ClassId},\"method_id\":{MethodId}}}";
        }
    }
}