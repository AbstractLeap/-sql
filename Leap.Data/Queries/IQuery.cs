namespace Leap.Data.Queries {
    using System;

    internal interface IQuery {
        Guid Identifier { get; }
        
        Type EntityType { get; }
    }
}