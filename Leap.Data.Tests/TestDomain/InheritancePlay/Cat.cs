using System;

namespace Leap.Data.Tests.TestDomain.InheritancePlay
{
    class Cat : Pet
    {
        public Cat(string name)
        {
            this.Name = name;
        }

        public override void Eat()
        {
            throw new NotImplementedException();
        }
    }
}
