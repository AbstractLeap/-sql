namespace Leap.Data.Schema {
    using System.Reflection;

    using Leap.Data.Utilities;

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