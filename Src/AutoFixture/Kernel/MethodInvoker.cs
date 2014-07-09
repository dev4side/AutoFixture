using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ploeh.AutoFixture.Kernel.Utilities;

namespace Ploeh.AutoFixture.Kernel
{
    /// <summary>
    /// Creates a new instance of the requested type by invoking the first method it can
    /// satisfy.
    /// </summary>
    public class MethodInvoker : ISpecimenBuilder
    {
        private readonly IMethodQuery query;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodInvoker"/> class with the supplied
        /// <see cref="IMethodQuery" />.
        /// </summary>
        /// <param name="query">
        /// The <see cref="IMethodQuery"/> that defines which methods are attempted.
        /// </param>
        public MethodInvoker(IMethodQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            this.query = query;
        }

        /// <summary>
        /// Gets the <see cref="IMethodQuery"/> that defines which constructors will be
        /// attempted.
        /// </summary>
        public IMethodQuery Query
        {
            get { return this.query; }
        }

        /// <summary>
        /// Creates a specimen of the requested type by invoking the first constructor or method it
        /// can satisfy.
        /// </summary>
        /// <param name="request">The request that describes what to create.</param>
        /// <param name="context">A context that can be used to create other specimens.</param>
        /// <returns>
        /// A specimen generated from a method of the requested type, if possible;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method uses the first constructor or method returned by <see cref="Query"/> where
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
                
                //var paramValues = (from pi in ci.Parameters
                //                   select context.Resolve(pi)).ToList();

                var paramValues = (from pi in ci.Parameters
                                   select GetDefaultValue(pi)).ToList(); // solve workbookProtocolManager bug

                if (paramValues.All(MethodInvoker.IsValueValid))
                {
                    // begin my new code
                    object result;
                    if (!MyFactory.TryGet(request, ci.Parameters.ToList(), paramValues, out result))
                        result = ci.Invoke(paramValues.ToArray());

                    return result;
                    // target -> return ObjectFactory.Get<T>(arguments);
                    // end   my new code

                    //return ci.Invoke(paramValues.ToArray());
                }
            }

            if (request is Type && ((Type) request).IsInterface)
            {
                object result;
                if (MyFactory.TryGet(request, out result))
                    return result;
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

            return this.query.SelectMethods(requestedType);
        }

        private static bool IsValueValid(object value)
        {
            return !(value is NoSpecimen)
                && !(value is OmitSpecimen);
        }

        private static object GetDefaultValue(ParameterInfo pi)
        {
            var type = pi.ParameterType;
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
