﻿namespace TildeSql.Tests.TestDomain.Blog {
    public class Blog {
        private string title;

        public Blog(string title) {
            this.BlogId = new BlogId();
            this.Title  = title;
        }

        public BlogId BlogId { get; init; }

        public string Title {
            get => this.title;
            set => this.title = value;
        }

        protected bool Equals(Blog other) {
            return Equals(this.BlogId, other.BlogId);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Blog)obj);
        }

        public override int GetHashCode() {
            return (this.BlogId != null ? this.BlogId.GetHashCode() : 0);
        }
    }
}