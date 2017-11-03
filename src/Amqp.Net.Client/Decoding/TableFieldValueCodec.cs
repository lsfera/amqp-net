using System;
using System.Collections.Generic;
using Amqp.Net.Client.Entities;
using Amqp.Net.Client.Extensions;
using DotNetty.Buffers;

namespace Amqp.Net.Client.Decoding
{
    internal class TableFieldValueCodec : FieldValueCodec<Table>
    {
        internal static readonly FieldValueCodec<Table> Instance = new TableFieldValueCodec();

        private static readonly IDictionary<Byte, IFieldValueCodec> DecodeMap =
            new Dictionary<Byte, IFieldValueCodec>
                {
                    { 0x74, BooleanFieldValueCodec.Instance },      // 't' - boolean (OCTET)                      -> Boolean
                    { 0x42, ByteFieldValueCodec.Instance },         // 'B' - short-short-uint (OCTET)             -> Byte
                    { 0x55, Int16FieldValueCodec.Instance },        // 'U' - short-int (2*OCTET)                  -> Int16
                    { 0x75, UInt16FieldValueCodec.Instance },       // 'u' - short-uint (2*OCTET)                 -> UInt16
                    { 0x49, Int32FieldValueCodec.Instance },        // 'I' - long-int (4*OCTET)                   -> Int32
                    { 0x69, UInt32FieldValueCodec.Instance },       // 'i' - long-uint (4*OCTET)                  -> UInt32
                    { 0x4C, Int64FieldValueCodec.Instance },        // 'L' - long-long-int (8*OCTET)              -> Int64
                    { 0x66, SingleFieldValueCodec.Instance },       // 'f' - float (4*OCTET)                      -> Single
                    { 0x64, DoubleFieldValueCodec.Instance },       // 'd' - double (8*OCTET)                     -> Double
                    { 0x73, ShortStringFieldValueCodec.Instance },  // 's' - short-string (OCTET *string-char)    -> String
                    { 0x53, LongStringFieldValueCodec.Instance },   // 'S' - long-string (long-uint *OCTET)       -> String
                    { 0x46, Instance }                              // 'F' - field-table                          -> Table
                };

        private static readonly IDictionary<Type, Func<Object, IFieldValueCodec>> EncodeMap =
            new Dictionary<Type, Func<Object, IFieldValueCodec>>
                {
                    { typeof(Boolean), _ => BooleanFieldValueCodec.Instance },
                    { typeof(Byte), _ => ByteFieldValueCodec.Instance },
                    { typeof(Int16), _ => Int16FieldValueCodec.Instance },
                    { typeof(UInt16), _ => UInt16FieldValueCodec.Instance },
                    { typeof(Int32), _ => Int32FieldValueCodec.Instance },
                    { typeof(UInt32), _ => UInt32FieldValueCodec.Instance },
                    { typeof(Int64), _ => Int64FieldValueCodec.Instance },
                    { typeof(Single), _ => SingleFieldValueCodec.Instance },
                    { typeof(Double), _ => DoubleFieldValueCodec.Instance },
                    { typeof(String), _ => ((String)_).Length > sizeof(Byte)
                                               ? LongStringFieldValueCodec.Instance
                                               : ShortStringFieldValueCodec.Instance },
                    { typeof(Table), _ => Instance },
                    { typeof(ClientCapabilities), _ => Instance } // TODO: redundant
                };

        public override Byte Type => 0x46;

        internal IFieldValueCodec For(Byte type)
        {
            if (DecodeMap.ContainsKey(type))
                return DecodeMap[type];

            throw new NotSupportedException($"type '{type} ({Convert.ToChar(type)})' is not supported");
        }

        internal IFieldValueCodec For(Object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var type = obj.GetType();

            if (EncodeMap.ContainsKey(type))
                return EncodeMap[type](obj);

            throw new NotSupportedException($"type '{type}' is not supported");
        }

        internal override Table Decode(IByteBuffer buffer)
        {
            var length = buffer.ReadInt();
            var start = buffer.ReaderIndex;
            var fields = new Dictionary<String, Object>();

            while (buffer.ReaderIndex < start + length - 1)
            {
                var name = buffer.DecodeFieldName();
                var type = buffer.ReadByte();
                var value = For(type).Decode(buffer);
                fields.Add(name, value);
            }

            return new Table(fields);
        }

        internal override void Encode(Table source, IByteBuffer buffer)
        {
            var b = Unpooled.Buffer();

            foreach (var field in source.Fields)
            {
                if (field.Value == null)
                    continue;

                var name = field.Key;
                name.EncodeFieldName(b);

                if (!EncodeMap.ContainsKey(field.Value.GetType()))
                    throw new NotSupportedException($"type '{field.Value.GetType()}' is not supported");

                var codec = For(field.Value);
                b.WriteByte(codec.Type);
                codec.Encode(field.Value, b);
            }

            buffer.WriteInt(b.WriterIndex);
            buffer.WriteBytes(b);
        }
    }
}