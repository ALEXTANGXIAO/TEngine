using ProtoBuf;

namespace TEngine
{
    [ProtoContract]
    public abstract class AProto
    {
        public virtual void AfterDeserialization() => EndInit();
        protected virtual void EndInit() { }
    }
}