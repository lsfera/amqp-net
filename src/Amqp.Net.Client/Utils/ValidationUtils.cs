using System;
using System.Text.RegularExpressions;

namespace Amqp.Net.Client.Utils
{
    internal static class ValidationUtils
    {
        internal static void ValidateQueueName(String name)
        {
            ValidateInternal(name, "queue");
        }

        internal static void ValidateExchangeName(String name)
        {
            ValidateInternal(name, "exchange");
        }

        private static void ValidateInternal(String name, String key)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var match = Regex.Match(name, "^(?!amq\\.)([a-zA-Z0-9_.\\-,])*$");

            if (!match.Success)
                throw new Exception($"string '{name}' is not allowed; the {key} name consists of a non-empty sequence of these characters: letters, digits, hyphen, underscore, period, or colon; exchange names starting with \"amq.\" are reserved");
        }
    }
}