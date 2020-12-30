﻿namespace Leap.Data.Internal {
    using System.Collections.Generic;
    using System.Linq;

    using Leap.Data.Queries;

    class ExecuteResult {
        public ExecuteResult(IEnumerable<IQuery> executedQueries, IEnumerable<IQuery> nonExecutedQueries) {
            this.ExecutedQueries    = executedQueries ?? Enumerable.Empty<IQuery>();
            this.NonExecutedQueries = nonExecutedQueries ?? Enumerable.Empty<IQuery>();
        }

        public ExecuteResult() : this(Enumerable.Empty<IQuery>(), Enumerable.Empty<IQuery>()) {
        }

        public IEnumerable<IQuery> ExecutedQueries { get; }

        public IEnumerable<IQuery> NonExecutedQueries { get; }
    }
}