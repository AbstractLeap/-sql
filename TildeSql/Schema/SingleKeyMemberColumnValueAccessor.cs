namespace TildeSql.Schema {
    using System.Reflection;

    using TildeSql.Utilities;

    class SingleKeyMemberColumnValueAccessor : IKeyColumnValueAccessor {
        private readonly MemberInfo memberInfo;

        public SingleKeyMemberColumnValueAccessor(MemberInfo memberInfo) {
            this.memberInfo = memberInfo;
        }

        public object GetValue<TKey>(TKey key) {
            return ReflectionUtils.GetMemberValue(this.memberInfo, key);
        }
    }
}