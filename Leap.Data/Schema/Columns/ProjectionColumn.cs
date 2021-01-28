namespace Leap.Data.Schema.Columns {
    using System;

    public record ProjectionColumn<TEntity, TColumn> : Column {
        public ProjectionColumn(Table table, string name, Func<TEntity, TColumn> projectionFunc) : base(typeof(TColumn), name, table) {
            this.ProjectionFunc = projectionFunc ?? throw new ArgumentNullException(nameof(projectionFunc));
        }
        
        public Func<TEntity, TColumn> ProjectionFunc { get; set; }
    }
}