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
            var key = client.Keystore.GetKeyById(StringToByteArray("4022a87de0ff0724"));

            Assert.AreEqual(StringToByteArray("279025e793dea73c0af88cef0e2f0a729b0846087029f1a27f4a63f10b8e2c51"),
                key.PublicKey);
        }
    }
}