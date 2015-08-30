using lageant.core.Models;

namespace lageant.core
{
    public interface ILageantCore
    {
        Keystore Keystore { get; set; }
        bool CreateServer();
        void AddKey(Key key);
        void RemoveKey(Key key);
        bool OpenClient();
        void Dispose();
    }
}