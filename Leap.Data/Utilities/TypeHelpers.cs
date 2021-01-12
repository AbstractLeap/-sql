namespace Leap.Data.Utilities {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    static class TypeHelpers {
        public static Type PropertyOrFieldType(this MemberInfo memberInfo) {
            if (memberInfo is FieldInfo fieldInfo) {
                return fieldInfo.FieldType;
            }

            if (memberInfo is PropertyInfo propertyInfo) {
                return propertyInfo.PropertyType;
            }

            throw new Exception($"Expected FieldInfo or PropertyInfo but got {memberInfo}");
        }

        public static IEnumerable<Type> GetAssignableTypes(this Type type, IEnumerable<Type> candidateTypes) {
            foreach (var candidateType in candidateTypes) {
                if (type.IsInterface) {
                    if (candidateType.GetInterfaces().Any(t => t == type)) {
                        yield return candidateType;
                    }
                }
                else {
                    var baseType = candidateType;
                    do {
                        if (baseType == type) {
                            yield return candidateType;
                            break;
                        }
                        baseType = baseType.BaseType;
                    }
                    while (baseType != null);
                }
            }
        }

        public static Type FindAssignableWith(this IEnumerable<Type> types) {
            var commonBaseClass = FindBaseClassWith(types);
            return commonBaseClass == typeof(object) ? FindInterfaceWith(types) : commonBaseClass;
        }

        public static Type FindAssignableWith(this Type typeLeft, Type typeRight) {
            if (typeLeft == null || typeRight == null) return null;

            var commonBaseClass = typeLeft.FindBaseClassWith(typeRight) ?? typeof(object);

            return commonBaseClass == typeof(object) ? typeLeft.FindInterfaceWith(typeRight) : commonBaseClass;
        }

        public static Type FindBaseClassWith(this IEnumerable<Type> types) {
            return GetMultipleTypeIntersection(types, type => type.GetClassHierarchy());
        }

        // searching for common base class (either concrete or abstract)
        public static Type FindBaseClassWith(this Type typeLeft, Type typeRight) {
            if (typeLeft == null || typeRight == null) return null;

            return typeLeft.GetClassHierarchy().Intersect(typeRight.GetClassHierarchy()).FirstOrDefault(type => !type.IsInterface);
        }

        public static Type FindInterfaceWith(this IEnumerable<Type> types) {
            return GetMultipleTypeIntersection(types, type => type.GetInterfaceHierarchy());
        }

        private static Type GetMultipleTypeIntersection(IEnumerable<Type> superTypes, Func<Type, IEnumerable<Type>> hierarchyAccessor) {
            HashSet<Type> intersection = null;
            foreach (var entry in superTypes.AsSmartEnumerable()) {
                var hierarchy = hierarchyAccessor(entry.Value);
                if (entry.IsFirst) {
                    intersection = new HashSet<Type>(hierarchy);
                }
                else {
                    intersection.IntersectWith(hierarchy);
                }
            }

            return intersection == null ? typeof(object) : (intersection.FirstOrDefault() ?? typeof(object));
        }

        // searching for common implemented interface
        // it's possible for one class to implement multiple interfaces, 
        // in this case return first common based interface
        public static Type FindInterfaceWith(this Type typeLeft, Type typeRight) {
            if (typeLeft == null || typeRight == null) return null;

            return typeLeft.GetInterfaceHierarchy().Intersect(typeRight.GetInterfaceHierarchy()).FirstOrDefault();
        }

        // iterate on interface hierarhy
        public static IEnumerable<Type> GetInterfaceHierarchy(this Type type) {
            if (type.IsInterface) return new[] { type }.AsEnumerable();

            return type.GetInterfaces().OrderByDescending(current => current.GetInterfaces().Count()).AsEnumerable();
        }

        // interate on class hierarhy
        public static IEnumerable<Type> GetClassHierarchy(this Type type) {
            if (type == null) yield break;

            Type typeInHierarchy = type;

            do {
                yield return typeInHierarchy;
                typeInHierarchy = typeInHierarchy.BaseType;
            }
            while (typeInHierarchy != null && !typeInHierarchy.IsInterface);
        }
    }
}