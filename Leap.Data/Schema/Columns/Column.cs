namespace Leap.Data.Schema.Columns {
    using System;

    public abstract record Column(Type Type, string Name, Collection Collection) {
        public bool IsComputed { get; init; }
    };
}