namespace Leap.Data.IdentityMap {
    using System;

    using Fasterflect;

    using Leap.Data.Internal;
    using Leap.Data.Schema;

    interface IDocument {
        Collection Collection { get; }

        DatabaseRow Row { get; set; }

        DocumentState State { get; set; }
    }

    interface IDocument<out TEntity> : IDocument {
        TEntity Entity { get; }
    }

    static class DocumentExtensions {
        public static Type EntityType(this IDocument document) {
            return document.GetType().GenericTypeArguments[0];
        }

        public static object GetEntity(this IDocument document) {
            return document.GetPropertyValue(nameof(IDocument<string>.Entity));
        }

        public static IDocument Create(object entity, Collection collection) {
            return (IDocument)typeof(Document<>).MakeGenericType(entity.GetType()).CreateInstance(entity, collection);
        }
    }
}