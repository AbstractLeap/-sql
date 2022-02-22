using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TildeSql.Internal
{
    using TildeSql.Utilities;

    internal class TypeSerializer
    {
        private readonly TwoWayMap<string, Type> entityNameTypeMap = new();
        
        public void AddType(Type entityType) {
            var typeToStore = entityType.IsGenericType 
                                  ? entityType.GetGenericTypeDefinition() 
                                  : entityType;
            this.entityNameTypeMap.Add(typeToStore.Name, typeToStore);
        }

        public string GetTypeName(Type entityType) {
            if (!entityType.IsGenericType) 
                return this.entityNameTypeMap[entityType];

            var genericDefinition = entityType.GetGenericTypeDefinition();
            var genericArgs = genericDefinition.GetGenericArguments().Length == 1 
                                  ? entityType.GetGenericArguments().Single().AssemblyQualifiedName
                                  : string.Join(", ", genericDefinition.GetGenericArguments().Select(t => $"[{t.AssemblyQualifiedName}]"));
            return this.entityNameTypeMap[genericDefinition] + $"[{genericArgs}]";

        }

        public Type GetTypeFromName(string typeName) {
            // todo remove this when we don't have fully qualified assembly names
            var tryFindType = Type.GetType(typeName);
            if (tryFindType != null) {
                return tryFindType;
            }

            if (!typeName.Contains('[')) { // 
                return this.entityNameTypeMap[typeName];
            }

            var genericDef = this.entityNameTypeMap[typeName.Substring(0, typeName.IndexOf('['))];
            var genericTypeArgs = typeName.Substring(typeName.IndexOf('[') + 1).Split("], [")
                                       .Select(s => Type.GetType(s.TrimStart('[').TrimEnd(']')))
                                       .ToArray();

            return genericDef.MakeGenericType(genericTypeArgs);
        }
    }
}
