using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace VA.TMP.Integration.Common
{
    /// <summary>
    /// Class to generate random numbers as strings.
    /// </summary>
    public static class RandomDigits
    {
        /// <summary>
        /// Gets a random number of specified length.
        /// </summary>
        /// <param name="numberOfDigits">The number of digits for the random number.</param>
        /// <returns>Random number string of specified length.</returns>
        public static string GetRandomDigitString(int numberOfDigits)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < numberOfDigits; ++i) sb.Append((char)('0' + NextRandomDigit()));

            return sb.ToString();
        }

        /// <summary>
        /// Generates a random integer.
        /// </summary>
        /// <returns>Random integer.</returns>
        private static int NextRandomDigit()
        {
            var bytes = new byte[8];

            using (var provider = new RNGCryptoServiceProvider())
            {
                while (true)
                {
                    provider.GetBytes(bytes);

                    if (bytes[0] >= 250) continue;

                    return bytes[0] % 10;
                }
            }
        }

        public static string SHA256HexHashString(string StringIn)
        {
            if (String.IsNullOrEmpty(StringIn))
                StringIn = "";
            byte[] bytes = Encoding.Default.GetBytes(StringIn);
            StringIn = Encoding.UTF8.GetString(bytes);
            using (HashAlgorithm algorithm = SHA256.Create())
            {
                var hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(StringIn));
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                //Console.WriteLine($"Hex: {hashString}");
                var intHash = BigInteger.Parse("0" + hashString, System.Globalization.NumberStyles.AllowHexSpecifier);
                //Console.WriteLine($"Int: {intHash}");
                return intHash.ToString();
            }
        }







    }
}
