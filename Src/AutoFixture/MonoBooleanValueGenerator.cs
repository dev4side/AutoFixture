using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ploeh.AutoFixture.Kernel;

namespace Ploeh.AutoFixture
{
    public class MonoBooleanValueGenerator : ISpecimenBuilder
    {
        private readonly bool _booleanValue;

        public MonoBooleanValueGenerator(){}

        public MonoBooleanValueGenerator(bool booleanValue)
        {
            _booleanValue = booleanValue;
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (!typeof(bool).Equals(request))
            {
                return new NoSpecimen(request);
            }

            return _booleanValue;
        }
    }
}
