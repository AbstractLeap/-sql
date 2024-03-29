﻿namespace TildeSql.Schema {
    using System.Reflection;

    using TildeSql.Utilities;

    class NestedKeyColumnValueAccessor : IKeyColumnValueAccessor {
        private readonly MemberInfo[] memberInfos;

        public NestedKeyColumnValueAccessor(MemberInfo[] memberInfos) {
            this.memberInfos = memberInfos;
        }

        public object GetValue<TKey>(TKey key) {
            object val = key;
            foreach (var memberInfo in this.memberInfos) {
                val = ReflectionUtils.GetMemberValue(memberInfo, val);
            }

            return val;
        }
    }
}