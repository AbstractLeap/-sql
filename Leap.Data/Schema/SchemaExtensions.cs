namespace Leap.Data.Schema {
    using System;

    using Fasterflect;

    internal static class SchemaExtensions {
        // TODO maybe delete?
        public static Table GetDefaultTable(this ISchema schema, Type tableType) {
            return (Table)schema.CallMethod(new[] { tableType }, nameof(ISchema.GetDefaultTable));
        }
    }
}