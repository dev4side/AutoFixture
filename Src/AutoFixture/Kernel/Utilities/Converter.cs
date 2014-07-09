using System.Collections.Generic;
using System.Reflection;
using Ninject.Parameters;

namespace Ploeh.AutoFixture.Kernel.Utilities
{
    public class Converter
    {
        public static IEnumerable<ConstructorArgument> GetArguments(IList<ParameterInfo> parameters, IList<object> values)
        {
            var total = parameters.Count;
            for (int i = 0; i < total; i++)
            {
                yield return new ConstructorArgument(parameters[i].Name, values[i]);
            }
        }
    }
}
