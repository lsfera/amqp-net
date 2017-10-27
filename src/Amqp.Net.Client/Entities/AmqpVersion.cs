using System;

namespace Amqp.Net.Client.Entities
{
    public struct AmqpVersion
    {
        public readonly Byte Major;
        public readonly Byte Minor;

        /// <remarks>
        /// HACK: converts major=8 and minor=0 into major=0 and minor=8. Please see the class comment.
        /// </remarks>
        public static AmqpVersion New(Byte major, Byte minor)
        {
            return new AmqpVersion(major == 8 && minor == 0 ? (Byte)0 : major,
                                   major == 8 && minor == 0 ? (Byte)8 : minor);
        }

        private AmqpVersion(Byte major, Byte minor)
        {
            Major = major;
            Minor = minor;
        }

        public override Boolean Equals(Object obj)
        {
            if (!(obj is AmqpVersion))
                return false;

            var version = (AmqpVersion)obj;

            return Major == version.Major &&
                   Minor == version.Minor;
        }

        public override Int32 GetHashCode()
        {
            var hashCode = 317314336;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Major.GetHashCode();
            hashCode = hashCode * -1521134295 + Minor.GetHashCode();

            return hashCode;
        }

        public override String ToString()
        {
            return $"{Major}.{Minor}";
        }

        public static Boolean operator ==(AmqpVersion left, AmqpVersion right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(AmqpVersion left, AmqpVersion right)
        {
            return !(left == right);
        }
    }
}