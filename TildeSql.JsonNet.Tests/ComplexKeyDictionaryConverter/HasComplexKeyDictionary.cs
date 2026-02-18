namespace TildeSql.JsonNet.Tests.ComplexKeyDictionaryConverter {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class HasComplexKeyDictionary {

        private int id;

        private IDictionary<IdLike, SomeValue> map;

        private Dictionary<CompoundId, SomeOtherValue> lookup;

        public int Id {
            get => this.id;
            set => this.id = value;
        }

        public Dictionary<CompoundId, SomeOtherValue> Lookup {
            get => this.lookup;
            set => this.lookup = value;
        }

        public IDictionary<IdLike, SomeValue> Map {
            get => this.map;
            set => this.map = value;
        }
    }

    public class SomeValue {
        private string name;

        public string Name {
            get => this.name;
            set => this.name = value;
        }
    }

    public class SomeOtherValue {
        private int x;

        private int y;

        public int X {
            get => this.x;
            set => this.x = value;
        }

        public int Y {
            get => this.y;
            set => this.y = value;
        }
    }

    public record IdLike {
        private int id;

        public int Id {
            get => this.id;
            set => this.id = value;
        }
    }

    public record CompoundId {
        private Guid idOne;

        private Guid idTwo;

        public Guid IdOne {
            get => this.idOne;
            set => this.idOne = value;
        }

        public Guid IdTwo {
            get => this.idTwo;
            set => this.idTwo = value;
        }
    }
}
