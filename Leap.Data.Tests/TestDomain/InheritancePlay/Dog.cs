﻿using System;

namespace Leap.Data.Tests.TestDomain.InheritancePlay
{
    abstract class Dog : Pet
    {
        public Dog(string name)
        {
            this.Name = name;
        }

        public override void Eat()
        {
            throw new NotImplementedException();
        }
    }
}
