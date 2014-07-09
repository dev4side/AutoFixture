using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ploeh.AutoFixture.Kernel;

namespace Ploeh.AutoFixture
{
    public class RandomBooleanGenerator : ISpecimenBuilder
    {
        private readonly Random _r = new Random();

        private readonly int _probabilityTrue;

        public RandomBooleanGenerator(int probabilityTrue)
        {
            probabilityTrue = Math.Min(100, probabilityTrue);
            probabilityTrue = Math.Max(0, probabilityTrue);
            _probabilityTrue = probabilityTrue;
        }

        public RandomBooleanGenerator(Double probabilityTrue)
            : this ((int)(probabilityTrue*100.0)) {}

        public RandomBooleanGenerator() : this(50){}


        public object Create(object request, ISpecimenContext context)
        {
            if (!typeof(bool).Equals(request))
            {
                return new NoSpecimen(request);
            }

            return (_r.Next(0, 100) < _probabilityTrue);
        }
    }
}
