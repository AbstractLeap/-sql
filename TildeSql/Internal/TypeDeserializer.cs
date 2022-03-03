namespace TildeSql.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class TypeDeserializer : ITypeDeserializer {
        private readonly Type singleTypeType;

        private readonly Type[] entityTypes;

        public TypeDeserializer(IEnumerable<Type> collectionTypes) {
            // we store the type or the generic type definition
            this.entityTypes = collectionTypes.Select(t => t.IsGenericType ? t.GetGenericTypeDefinition() : t).ToArray();
            if (this.entityTypes.Length == 1) {
                this.singleTypeType = this.entityTypes[0].IsGenericType ? this.entityTypes[0].GetGenericTypeDefinition() : this.entityTypes[0];
            }
        }

        public Type Deserialize(string typeName) {
            // HOT PATH
            if (this.singleTypeType != null) {
                if (this.singleTypeType.IsGenericType) {
                    if (typeName.StartsWith(this.singleTypeType.Name)) {
                        return CreateGenericType(this.singleTypeType);
                    }
                }
                else {
                    if (string.Equals(typeName, this.singleTypeType.Name)) {
                        return this.singleTypeType;
                    }
                }
            }

            foreach (var entityType in this.entityTypes) {
                if (string.Equals(typeName, entityType.Name)) {
                    return entityType.IsGenericType ? CreateGenericType(entityType) : entityType;
                }
            }

            return Type.GetType(typeName);

            Type CreateGenericType(Type baseType) {
                var genericTypeArgs = typeName.Substring(typeName.IndexOf('[') + 1).Split("], [").Select(s => Type.GetType(s.TrimStart('[').TrimEnd(']'))).ToArray();
                return baseType.MakeGenericType(genericTypeArgs);
            }
        }
    }
}