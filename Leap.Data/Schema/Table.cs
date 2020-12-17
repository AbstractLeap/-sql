namespace Leap.Data.Schema
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     metadata
    /// </summary>
    class Table {
        public string Name { get; init; }

        public string Schema { get; init; }
        
        public Type KeyType { get; init; }

        public IList<Column> Columns { get; init; }
        
        protected bool Equals(Table other) {
            return this.Name == other.Name && this.Schema == other.Schema;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Table)obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(this.Name, this.Schema);
        }
    }

    //public record Document<T, TId>
    //{
    //    public TId Id { get; init; }

    //    public T Aggregate { get; init; }

    //    public string JsonDocument { get; init; }

    //    public Guid VersionId { get; init; }
    //}

    //public class Session : ISession
    //{
    //    private readonly string connectionString;

    //    public Session(string? connectionString)
    //    {
    //        this.connectionString = connectionString;
    //    }

    //    public async Task<NpgsqlConnection> GetOrOpenConnectionAsync()
    //    {
    //        var connection = new NpgsqlConnection(this.connectionString);
    //        await connection.OpenAsync();
    //        await connection.BeginTransactionAsync(System.Data.IsolationLevel.Snapshot);
    //        return connection;
    //    }

    //    public Task CommitAsync()
    //    {
    //        // find changed data, execute sql, commit
    //    }

    //    public void Register<T>(T obj)
    //    {

    //    }

    //    public void RegisterNew<T>(T obj)
    //    {

    //    }

    //    public void RegisterDeleted<T>(T obj)
    //    {

    //    }
    //}
}
