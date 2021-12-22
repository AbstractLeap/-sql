namespace TildeSql.Schema.Conventions {
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IKeyMemberSchemaConvention : ISchemaConvention {
        MemberInfo[] GetKeyMember(string collectionName, IEnumerable<Type> entityTypes);
    }
}