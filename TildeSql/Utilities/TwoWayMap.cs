namespace TildeSql.Utilities {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    ///     Two-way dictionary
    /// </summary>
    /// <remarks>Heavily inspired by https://stackoverflow.com/a/41907561 </remarks>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class TwoWayMap<T1, T2> : IEnumerable<KeyValuePair<T1, T2>> {
        private readonly Dictionary<T1, T2> forward = new();

        private readonly Dictionary<T2, T1> reverse = new();

        public TwoWayMap() {
            Forward = new Indexer<T1, T2>(this.forward);
            Reverse = new Indexer<T2, T1>(this.reverse);
        }

        public Indexer<T1, T2> Forward { get; private set; }

        public Indexer<T2, T1> Reverse { get; private set; }

        public T2 this[T1 t1] => this.forward[t1];

        public T1 this[T2 t2] => this.reverse[t2];

        public void Add(T1 t1, T2 t2) {
            this.forward.Add(t1, t2);
            this.reverse.Add(t2, t1);
        }

        public void Remove(T1 t1) {
            T2 revKey = Forward[t1];
            this.forward.Remove(t1);
            this.reverse.Remove(revKey);
        }

        public void Remove(T2 t2) {
            T1 forwardKey = Reverse[t2];
            this.reverse.Remove(t2);
            this.forward.Remove(forwardKey);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() {
            return this.forward.GetEnumerator();
        }

        public class Indexer<T3, T4> {
            private readonly Dictionary<T3, T4> dictionary;

            public Indexer(Dictionary<T3, T4> dictionary) {
                this.dictionary = dictionary;
            }

            public T4 this[T3 index] {
                get {
                    return this.dictionary[index];
                }
                set {
                    this.dictionary[index] = value;
                }
            }

            public IEnumerable<T3> Keys => this.dictionary.Keys;

            public IEnumerable<T4> Values => this.dictionary.Values;

            public bool Contains(T3 key) {
                return this.dictionary.ContainsKey(key);
            }

            public bool TryAdd(T3 key, T4 value) {
                return this.dictionary.TryAdd(key, value);
            }

            public void Remove(T3 key) {
                this.dictionary.Remove(key);
            }
        }

        public bool TryAdd(T1 t1, T2 t2) {
            var forwardAdded = this.Forward.TryAdd(t1, t2);
            var reverseAdded = this.reverse.TryAdd(t2, t1);
            if (forwardAdded) {
                if (reverseAdded) return true;
                this.Reverse.Remove(t2);
                return false;
            }

            if (reverseAdded) {
                this.forward.Remove(t1);
            }

            return false;
        }

        public bool Contains(T1 t1, T2 t2) {
            return this.Forward.Contains(t1) && this.Reverse.Contains(t2);
        }
    }
}