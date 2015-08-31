using System;
using System.Linq;
using NUnit.Framework;

namespace lageant.client.tests
{
    [TestFixture]
    public class ClientTests
    {
        /// <summary>
        ///     Convert hex into byte array.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x%2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        [Test]
        public void ManualTest()
        {
            var client = new LageantClient();
            if (!client.Connect()) return;
            var key = client.Keystore.GetKeyById(StringToByteArray("817618db3eafb76b"));

            Assert.AreEqual(StringToByteArray("cb9e100f267e13817df835793148ee4fe98df78a620929b800ea8fdbea80990f"),
                key.PublicKey);
        }
    }
}