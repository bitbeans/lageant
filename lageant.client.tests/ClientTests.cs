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
        /// <param name="hex">A valid hex string.</param>
        /// <returns>A byte array.</returns>
        private static byte[] ConvertToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        [Test]
        public void ManualTest()
        {
            var client = new LageantClient();
            if (!client.Connect()) return;
            var key = client.Keystore.GetKeyById("5b6d0ce1bf4ed9c5");

            var keyStore = client.Keystore;
            Console.WriteLine(keyStore.Keys.Count);

            Assert.AreEqual(ConvertToByteArray("c3e6725d3cdb2de6c71f6b437980c0db1125bd8d9bc249ccb7cdb9901c6d7c6a"),
                key.PublicKey);
        }
    }
}