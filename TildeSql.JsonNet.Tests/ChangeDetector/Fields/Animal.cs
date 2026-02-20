namespace TildeSql.JsonNet.Tests.ChangeDetector.Fields {
    public abstract class Animal {
        private string name;

        public string Name {
            get => this.name;
            set => this.name = value;
        }
    }

    public class Dog : Animal {
        private int? barksPerMinute;

        public int? BarksPerMinute {
            get => this.barksPerMinute;
            set => this.barksPerMinute = value;
        }
    }

    public class Cat : Animal {
        private bool? likesSun;

        public bool? LikesSun {
            get => this.likesSun;
            set => this.likesSun = value;
        }
    }

    public class HasAnimal {
        private Animal animal;

        public Animal Animal {
            get => this.animal;
            set => this.animal = value;
        }
    }

    public class HasAnimals {
        private Animal[] animals;

        public Animal[] Animals {
            get => this.animals;
            set => this.animals = value;
        }
    }
}