namespace Leap.Data.Schema {
    using System;

    using Fasterflect;

    internal static class SchemaExtensions {
        // TODO maybe delete?
        public static Collection GetDefaultCollection(this ISchema schema, Type collectionType) {
            return (Collection)schema.CallMethod(new[] { collectionType }, nameof(ISchema.GetDefaultCollection));
        }
    }
}