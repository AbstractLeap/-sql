
using System;

namespace Leap.Data.Tests.TestDomain.GenericType
{
    class Entity<T>
    {
        public Entity(T thing)
        {
            Thing = thing;
        }

        public Guid Id { get; } = Guid.NewGuid();

        public T Thing { get; set; }

        public DateTime Date = DateTime.UtcNow;
    }
}