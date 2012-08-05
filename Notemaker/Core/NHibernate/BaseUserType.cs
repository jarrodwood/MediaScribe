using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System.Data;

namespace JayDev.Notemaker.Core
{
    public abstract class BaseUserType<T> : IUserType
    {
        public abstract SqlType[] SqlTypes { get; }
        public System.Type ReturnedType { get { return typeof(T); } }
        public new bool Equals(object x, object y)
        {
            if (object.ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }
        public int GetHashCode(object x) { return x.GetHashCode(); }
        public abstract object NullSafeGet(IDataReader dr, string[] names,
                                           object owner);
        public abstract void NullSafeSet(IDbCommand cmd, object value, int index);
        public object DeepCopy(object value) { return value; }
        public bool IsMutable { get { return false; } }
        public object Replace(object original, object target, object owner)
        { return original; }
        public object Assemble(object cached, object owner)
        { return DeepCopy(cached); }
        public object Disassemble(object value) { return DeepCopy(value); }
    }
}