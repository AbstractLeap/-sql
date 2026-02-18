namespace TildeSql.IdentityMap {
    public enum DocumentState {
        Persisted = 1, // indicates that it's in the database and tracked

        New = 2,

        Deleted = 3,
        
        NotAttached = 4 // indicates that this entity is not being tracked
    }
}