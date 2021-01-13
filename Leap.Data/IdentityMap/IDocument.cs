﻿namespace Leap.Data.IdentityMap {
    using Leap.Data.Internal;

    public interface IDocument<out TEntity> {
        TEntity Entity { get; }

        DatabaseRow Row { get; }

        DocumentState State { get; set; }
    }
}