﻿namespace Leap.Data.Schema.Columns {
    using System;
    using System.Reflection;

    public record KeyColumn(Type Type, string Name, Collection Collection, MemberInfo KeyMemberInfo, MemberInfo[] MemberAccessors) : Column(Type, Name, Collection);
}