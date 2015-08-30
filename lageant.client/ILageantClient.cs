using lageant.client.Models;

namespace lageant.client
{
    public interface ILageantClient
    {
        Keystore Keystore { get; }
        bool Connect();
        void Dispose();
    }
}