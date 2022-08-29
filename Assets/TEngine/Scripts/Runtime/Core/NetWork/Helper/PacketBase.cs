using ProtoBuf;

namespace TEngine.Runtime
{
    public abstract class PacketBase : Packet, IExtensible
    {
        private IExtension m_ExtensionObject;

        public PacketBase()
        {
            m_ExtensionObject = null;
        }

        public abstract PacketType PacketType
        {
            get;
        }

        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref m_ExtensionObject, createIfMissing);
        }
    }
}
