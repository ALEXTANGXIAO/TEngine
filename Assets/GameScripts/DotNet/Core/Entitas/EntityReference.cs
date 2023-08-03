// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
namespace TEngine
{
    public readonly struct EntityReference<T> where T : Entity
    {
        private readonly T _entity;
        private readonly long _runTimeId;

        private EntityReference(T t)
        {
            _entity = t;
            _runTimeId = t.RuntimeId;
        }

        public static implicit operator EntityReference<T>(T t)
        {
            return new EntityReference<T>(t);
        }

        public static implicit operator T(EntityReference<T> v)
        {
            if (v._entity == null)
            {
                return null;
            }

            return v._entity.RuntimeId != v._runTimeId ? null : v._entity;
        }
    }
}