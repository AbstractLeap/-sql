﻿namespace TildeSql.Schema.Conventions {
    using System;

    public interface ICollectionNamingSchemaConvention : ISchemaConvention {
        string GetCollectionName(Type type);
    }
}