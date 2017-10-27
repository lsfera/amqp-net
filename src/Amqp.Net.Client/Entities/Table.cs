using System;
using System.Collections.Generic;
using System.Linq;

namespace Amqp.Net.Client.Entities
{
    internal class Table : IEquatable<Table>
    {
        internal readonly IDictionary<String, Object> Fields;

        internal Table(IDictionary<String, Object> fields)
        {
            Fields = fields;
        }

        internal T Field<T>(String key)
        {
            return Fields.ContainsKey(key) ? (T)Fields[key] : default(T);
        }

        internal Boolean IsEmpty => !Fields.Any();

        public override String ToString()
        {
            return $"{{{String.Join(",", Fields.Select(_ => $"\"{_.Key}\":{FormatValue(_.Value)}"))}}}";
        }

        private static String FormatValue(Object obj)
        {
            if (obj is String)
                return $"\"{obj.ToString()}\"";

            if (obj is Boolean)
                return $"{obj.ToString().ToLowerInvariant()}";

            return $"{obj.ToString()}";
        }

        public Boolean Equals(Table other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Equals(Fields, other.Fields);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj is Table other && FieldsEquality(other);
        }

        private Boolean FieldsEquality(Table other)
        {
            if (Fields.Count != other.Fields.Count)
                return false;

            foreach (var field in Fields)
            {
                if (!other.Fields.ContainsKey(field.Key))
                    return false;

                var v1 = field.Value;
                var v2 = other.Fields[field.Key];

                if (!Equals(v1, v2))
                    return false;
            }

            return true;
        }

        public override Int32 GetHashCode()
        {
            if (Fields.Count == 0)
                return 0;

            var result = 0;

            foreach (var field in Fields.OrderBy(_ => _.Key))
            {
                var key = field.Key;
                var value = field.Value;

                result = (result * 397) ^ key.GetHashCode();
                result = (result * 397) ^ (value?.GetHashCode() ?? 0);
            }

            return result;
        }
    }
}