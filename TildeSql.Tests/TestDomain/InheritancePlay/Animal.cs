namespace TildeSql.Tests.TestDomain.InheritancePlay
{
    abstract class Animal : IAnimal
    {
        protected bool Equals(Animal other)
        {
            return Equals(this.Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Animal)obj);
        }

        public override int GetHashCode()
        {
            return (this.Id != null ? this.Id.GetHashCode() : 0);
        }

        public Animal()
        {
            this.Id = new AnimalId();
        }

        public AnimalId Id { get; init; }

        public abstract void Eat();
    }
}
