namespace Leap.Data.Schema.Columns {
    using System;

    public abstract record Column(Type Type, string Name, Table Table) {
        public bool IsComputed { get; init; }
    };
}