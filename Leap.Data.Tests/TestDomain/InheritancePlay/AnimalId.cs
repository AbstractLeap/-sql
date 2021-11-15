using System;

namespace Leap.Data.Tests.TestDomain.InheritancePlay
{
    record AnimalId
    {
        public AnimalId()
        {
            this.Id = Guid.NewGuid();
        }

        public AnimalId(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; init; }
    }
}
