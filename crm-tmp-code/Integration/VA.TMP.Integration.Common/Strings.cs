using System;

namespace VA.TMP.Integration.Common
{
    public static class Strings
    {
        /// <summary>
        /// Trims the given string from the begining.
        /// </summary>
        /// <param name="value">String to trim.</param>
        /// <param name="toTrim">Value to trim.</param>
        /// <returns></returns>
        public static string TrimStart(this string value, string toTrim)
        {
            var result = value;
            while (TrimStart(result, toTrim, out result)) { }
            return result;
        }

        /// <summary>
        /// Trims the given string from the begining.
        /// </summary>
        /// <param name="value">String to trim.</param>
        /// <param name="toTrim">Value to trim.</param>
        /// <param name="result">Result of the trim start.</param>
        /// <returns></returns>
        private static bool TrimStart(this string value, string toTrim, out string result)
        {
            result = value;
            if (!value.StartsWith(toTrim)) return false;

            var startIndex = toTrim.Length;
            result = value.Substring(startIndex);
            return true;
        }

        /// <summary>
        /// Builds an error message from an exception recursively.
        /// </summary>
        /// <param name="ex">Exception.</param>
        /// <returns>Exception message.</returns>
        public static string BuildErrorMessage(Exception ex)
        {
            var errorMessage = ex.Message;

            if (ex.InnerException == null) return errorMessage;

            errorMessage += string.Format("\n\n{0}\n", ex.InnerException.Message);
            errorMessage += BuildErrorMessage(ex.InnerException);

            return errorMessage;
        }
    }
}