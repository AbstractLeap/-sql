namespace TildeSql.Schema {
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    using TildeSql.Utilities;

    class MultipleKeyMemberColumnValueAccessor : IKeyColumnValueAccessor {
        private readonly int tupleIdx;

        private readonly MemberInfo keyMemberInfo;

        public MultipleKeyMemberColumnValueAccessor(int tupleIdx, MemberInfo keyMemberInfo) {
            this.tupleIdx      = tupleIdx;
            this.keyMemberInfo = keyMemberInfo;
        }

        public object GetValue<TKey>(TKey key) {
            if (!(key is ITuple tuple)) {
                throw new NotSupportedException();
            }

            var compositeKeyValue = tuple[this.tupleIdx];
            if (this.keyMemberInfo == null) {
                // equivalent to a primitive key type i.e. it's an int or string or something
                return compositeKeyValue;
            }

            return ReflectionUtils.GetMemberValue(this.keyMemberInfo, compositeKeyValue);
        }
    }
}