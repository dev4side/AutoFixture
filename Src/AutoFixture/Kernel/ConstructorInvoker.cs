﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Kernel;

namespace Ploeh.AutoFixture.Kernel
{
    /// <summary>
    /// Creates a new instance of the requested type by invoking the first constructor it can
    /// satisfy.
    /// </summary>
    [Obsolete("Use MethodInvoker instead.", true)]
    public class ConstructorInvoker : ISpecimenBuilder
    {
        private readonly IConstructorQuery query;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorInvoker"/> class with the
        /// supplied <see cref="IConstructorQuery"/>.
        /// </summary>
        /// <param name="query">
        /// The <see cref="IConstructorQuery"/> that defines which constructors are attempted.
        /// </param>
        public ConstructorInvoker(IConstructorQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            this.query = query;
        }

        /// <summary>
        /// Gets the <see cref="IConstructorQuery"/> that defines which constructors will be
        /// attempted.
        /// </summary>
        public IConstructorQuery Query
        {
            get { return this.query; }
        }

        /// <summary>
        /// Creates a specimen of the requested type by invoking the first constructor it can
        /// satisfy.
        /// </summary>
        /// <param name="request">The request that describes what to create.</param>
        /// <param name="context">A context that can be used to create other specimens.</param>
        /// <returns>
        /// A specimen generated from a constructor of the requested type, if possible;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method uses the first constructor returned by <see cref="Query"/> where
        /// <paramref name="context"/> can create values for all parameters.
        /// </para>
        /// </remarks>
        public object Create(object request, ISpecimenContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            foreach (var ci in this.GetConstructors(request))
            {
                var paramValues = (from pi in ci.Parameters
                                   select context.Resolve(pi)).ToList();

                if (paramValues.All(ConstructorInvoker.IsValueValid))
                {
                    return ci.Invoke(paramValues.ToArray());
                    //return ObjectFactory.Get<object>(paramValues.ToArray());
                }
            }

            return new NoSpecimen(request);
        }

        private IEnumerable<IMethod> GetConstructors(object request)
        {
            var requestedType = request as Type;
            if (requestedType == null)
            {
                return Enumerable.Empty<IMethod>();
            }

            return this.query.SelectConstructors(requestedType);
        }

        private static bool IsValueValid(object value)
        {
            return !(value is NoSpecimen);
        }
    }
}
