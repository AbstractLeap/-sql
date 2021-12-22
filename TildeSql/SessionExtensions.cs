namespace TildeSql {
    using System;

    using Fasterflect;

    public static class SessionExtensions {
        public static void Add(this ISession session, Type type, object entity, string collectionName = null) {
            if (collectionName == null) {
                session.CallMethod(new[] { type }, nameof(ISession.Add), entity);
            }
            else {
                session.CallMethod(new[] { type }, nameof(ISession.Add), entity, collectionName);
            }
        }
    }
}