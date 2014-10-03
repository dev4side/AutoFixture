﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Moq;
using Ploeh.AutoFixture.AutoMoq.Extensions;
using Ploeh.AutoFixture.Kernel;

namespace Ploeh.AutoFixture.AutoMoq
{
    /// <summary>
    /// Sets up a mocked object's methods so that the return values will be retrieved from a fixture,
    /// instead of being created directly by Moq.
    /// 
    /// This will setup any virtual methods that are either non-void or have "out" parameters.
    /// </summary>
    /// <remarks>
    /// This will setup any virtual methods that are either non-void or have "out" parameters.
    /// This includes:
    ///  - interface's methods/property getters;
    ///  - class's abstract/virtual/overridden/non-sealed methods/property getters.
    /// 
    /// Notes:
    /// - Due to a limitation in Moq, methods with "ref" parameters are skipped.
    /// - Automatic mocking of generic methods isn't feasible either - we'd have to antecipate any type parameters that this method could be called with. 
    /// </remarks>
    public class MockVirtualMethodsCommand : ISpecimenCommand
    {
        /// <summary>
        /// Sets up a mocked object's methods so that the return values will be retrieved from a fixture,
        /// instead of being created directly by Moq.
        /// </summary>
        /// <param name="specimen">The mock to setup.</param>
        /// <param name="context">The context of the mock.</param>
        public void Execute(object specimen, ISpecimenContext context)
        {
            if (specimen == null) throw new ArgumentNullException("specimen");
            if (context == null) throw new ArgumentNullException("context");

            var mock = specimen as Mock;
            if (mock == null)
                return;

            var mockType = mock.GetType();
            var mockedType = mockType.GetMockedType();
            var methods = GetConfigurableMethods(mockedType);

            foreach (var method in methods)
            {
                var returnType = method.ReturnType;
                var methodInvocationLambda = MakeMethodInvocationLambda(mockedType, method, context);

                if (method.IsVoid())
                {
                    //call `Setup`
                    mock.Setup(methodInvocationLambda);
                }
                else
                {
                    //call `Setup`
                    var setup = mock.Setup(returnType, methodInvocationLambda);

                    //call `Returns`
                    setup.ReturnsUsingContext(context, mockedType, returnType);
                }
            }
        }

        /// <summary>
        /// Gets a list of methods to configure.
        /// </summary>
        /// <param name="type">The type being mocked and whose methods need to be configured.</param>
        private static IEnumerable<MethodInfo> GetConfigurableMethods(Type type)
        {
            // If "type" is an interface, "GetMethods" does not return methods declared on other interfaces extended by "type".
            // In these cases, we use the "GetInterfaceMethods" extension method instead.
            var methods = type.IsInterface
                              ? type.GetInterfaceMethods()
                              : type.GetMethods();

            return methods.Where(CanBeConfigured);
        }

        /// <summary>
        /// Determines whether a method can be mocked.
        /// </summary>
        /// <param name="method">The candidate method.</param>
        /// <returns>Whether <paramref name="method"/> can be configured.</returns>
        private static bool CanBeConfigured(MethodInfo method)
        {
            return method.IsOverridable() &&
                   !method.IsGenericMethod &&
                   !method.HasRefParameters() &&
                   (!method.IsVoid() || method.HasOutParameters());
        }

        /// <summary>
        /// Returns a lambda expression thats represents an invocation of a mocked type's method.
        /// E.g., <![CDATA[ x => x.Method(It.IsAny<string>(), out parameter) ]]> 
        /// </summary>
        private static Expression MakeMethodInvocationLambda(Type mockedType, MethodInfo method,
                                                             ISpecimenContext context)
        {
            var lambdaParam = Expression.Parameter(mockedType, "x");

            var methodCallParams = method.GetParameters()
                                         .Select(param => MakeParameterExpression(param, context))
                                         .ToList();

            //e.g. "x.Method(It.IsAny<string>(), out parameter)"
            var methodCall = Expression.Call(lambdaParam, method, methodCallParams);

            //e.g. "x => x.Method(It.IsAny<string>(), out parameter)"
            return Expression.Lambda(methodCall, lambdaParam);
        }

        private static Expression MakeParameterExpression(ParameterInfo parameter, ISpecimenContext context)
        {
            //check if parameter is an "out" parameter
            if (parameter.IsOut)
            {
                //gets the type corresponding to this "byref" type
                //e.g., the underlying type of "System.String&" is "System.String"
                var underlyingType = parameter.ParameterType.GetElementType();

                //resolve the "out" param from the context
                object variable = context.Resolve(underlyingType);

                return Expression.Constant(variable, underlyingType);
            }
            else
            {
                //for any non-out parameter, invoke "It.IsAny<T>()"
                var isAnyMethod = typeof (It).GetMethod("IsAny")
                                             .MakeGenericMethod(parameter.ParameterType);

                return Expression.Call(isAnyMethod);
            }
        }
    }
}
