using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ploeh.AutoFixture.Kernel;

namespace Ploeh.AutoFixture
{
    class NullableGenerator : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            var type = request as Type;

            if (type == null || !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>)))
            {
                return new NoSpecimen(request);
            }

            return null;
            // TODO if we have a nullable<int>, we should return a request for int
        }
    }
}
