namespace TildeSql.Internal {
    using System.Collections.Generic;

    using TildeSql.Queries;

    public class ExecuteResult {
        public ExecuteResult(IEnumerable<IQuery> executedQueries, IEnumerable<(IQuery, IQuery, IQuery)> partiallyExecutedQueries, IEnumerable<IQuery> nonExecutedQueries)
            : this(executedQueries, nonExecutedQueries) {
            this.PartiallyExecutedQueries = partiallyExecutedQueries ?? [];
        }

        public ExecuteResult(IEnumerable<IQuery> executedQueries, IEnumerable<IQuery> nonExecutedQueries) {
            this.ExecutedQueries    = executedQueries ?? [];
            this.NonExecutedQueries = nonExecutedQueries ?? [];
        }

        public ExecuteResult()
            : this([], []) { }

        public IEnumerable<IQuery> ExecutedQueries { get; }

        public IEnumerable<(IQuery Original, IQuery Executed, IQuery Remaining)> PartiallyExecutedQueries { get; } = [];

        public IEnumerable<IQuery> NonExecutedQueries { get; }
    }
}