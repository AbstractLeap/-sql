namespace Leap.Data.Schema {
    using System.Reflection;

    using Leap.Data.Utilities;

    class SingleKeyMemberNestedColumnValueAccessor : IKeyColumnValueAccessor {
        private readonly MemberInfo memberInfo;

        private readonly MemberInfo nestedMemberInfo;

        public SingleKeyMemberNestedColumnValueAccessor(MemberInfo memberInfo, MemberInfo nestedMemberInfo) {
            this.memberInfo       = memberInfo;
            this.nestedMemberInfo = nestedMemberInfo;
        }

        public object GetValue<TKey>(TKey key) {
            var outerKey = ReflectionUtils.GetMemberValue(this.memberInfo, key);
            return ReflectionUtils.GetMemberValue(this.nestedMemberInfo, outerKey);
        }
    }
}