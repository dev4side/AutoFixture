using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Kernel;
using Ninject.Parameters;

namespace Ploeh.AutoFixture.Kernel.Utilities
{
    public class MyFactory
    {
        public static bool TryGet(object request, IList<ParameterInfo> parameters, IList<object> values, out object result)
        {
            var arguments = Converter.GetArguments(parameters, values).ToArray();

            var type = request as Type;
            // Get the generic type definition
            MethodInfo method = typeof(ObjectFactory).GetMethod("Get",
                BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any,
                new Type[] { typeof(IParameter[]) }, null);

            // Build a method with the specific type argument you're interested in
            var genericMethod = method.MakeGenericMethod(type);

            result = null;
            try
            {
                result = genericMethod.Invoke(null, new object[] { arguments });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryGet(object request, out object result)
        {
            var type = request as Type;

            MethodInfo method = typeof(ObjectFactory).GetMethod("Get",
                BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any,
                new Type[] { typeof(IParameter[]) }, null);

            var genericMethod = method.MakeGenericMethod(type);

            result = null;
            try
            {
                result = genericMethod.Invoke(null, new object[] { new List<IParameter>().ToArray() });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static object Get(object request, IList<ParameterInfo> parameters, IList<object> values)
        {
            var arguments = Converter.GetArguments(parameters, values).ToArray();
            var type = request as Type;

            MethodInfo method = typeof(ObjectFactory).GetMethod("Get",
                BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any,
                new Type[] { typeof(IParameter[]) }, null);

            var genericMethod = method.MakeGenericMethod(type);
            return genericMethod.Invoke(null, new object[] {arguments});
        }


        public static object Get(object request)
        {
            var type = request as Type;

            MethodInfo method = typeof(ObjectFactory).GetMethod("Get",
                BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any,
                new Type[] { typeof(IParameter[]) }, null);

            var genericMethod = method.MakeGenericMethod(type);
            return genericMethod.Invoke(null, new object[] { new List<IParameter>().ToArray() });
        }
    }
}
