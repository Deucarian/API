using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.API.Core;
using Deucarian.API.Models;
using NUnit.Framework;

namespace Deucarian.API.Tests
{
    public sealed class ApiCallbackSurfaceTests
    {
        #region Test Methods

        [Test]
        public void EveryPublicRequestTaskHasTypedSuccessAndFailureCallbacks()
        {
            Assembly assembly = typeof(IApiClient).Assembly;
            Type[] types = assembly.GetExportedTypes();
            List<string> missing = new List<string>();
            int checkedCount = 0;
            foreach (Type type in types)
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance
                                                              | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    Type response = ResponseType(method.ReturnType);
                    if (response == null)
                        continue;

                    checkedCount++;
                    string callbackName = method.Name.EndsWith("Async", StringComparison.Ordinal)
                        ? method.Name.Substring(0, method.Name.Length - 5)
                        : method.Name;
                    IEnumerable<MethodInfo> candidates = type.GetMethods()
                        .Where(candidate => candidate.Name == callbackName);
                    if (type == typeof(IApiClient))
                    {
                        candidates = candidates.Concat(typeof(ApiClientCallbackExtensions).GetMethods()
                            .Where(candidate => candidate.Name == callbackName)
                        );
                    }
                    else if (type == typeof(ApiClientEndpointDefinitionExtensions))
                    {
                        candidates = candidates.Concat(typeof(ApiClientCallbackExtensions).GetMethods()
                            .Where(candidate => candidate.Name == callbackName)
                        );
                    }

                    if (!candidates.Any(candidate => Matches(method, candidate, response)))
                        missing.Add(type.Name + "." + method);
                }
            }

            Assert.That(checkedCount,
                        Is.GreaterThanOrEqualTo(30),
                        "The public request audit must remain comprehensive."
            );
            Assert.That(missing, Is.Empty, "Public API requests require typed success/failure counterparts.");
        }

        #endregion

        #region Methods

        private static Type ResponseType(Type returnType)
        {
            if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>))
                return null;

            Type result = returnType.GetGenericArguments()[0];
            if (!result.IsGenericType)
                return null;

            Type definition = result.GetGenericTypeDefinition();
            return definition == typeof(ApiResult<>) || definition == typeof(ApiCallResult<>)
                ? result.GetGenericArguments()[0]
                : null;
        }

        private static bool Matches(MethodInfo taskMethod, MethodInfo candidate, Type response)
        {
            if (candidate.ReturnType != typeof(void)
                || candidate.GetGenericArguments().Length != taskMethod.GetGenericArguments().Length)
                return false;

            ParameterInfo[] callbackParameters = candidate.GetParameters();
            if (!callbackParameters.Any(parameter => IsAction(parameter.ParameterType, response))
                || !callbackParameters.Any(parameter => IsAction(parameter.ParameterType, typeof(ApiError))))
                return false;

            IEnumerable<ParameterInfo> taskParameters = taskMethod.GetParameters();
            IEnumerable<ParameterInfo> parameters = callbackParameters;
            if (taskMethod.DeclaringType == typeof(IApiClient))
                parameters = parameters.Skip(1);

            return Normalize(taskParameters).SequenceEqual(Normalize(parameters));
        }

        private static IEnumerable<string> Normalize(IEnumerable<ParameterInfo> parameters)
        {
            return parameters.Where(parameter => parameter.ParameterType != typeof(CancellationToken)
                                                 && !typeof(Delegate).IsAssignableFrom(parameter.ParameterType)
                                                 && !parameter.IsDefined(typeof(CallerMemberNameAttribute), false)
                                                 && !parameter.IsDefined(typeof(CallerFilePathAttribute), false)
                                                 && !parameter.IsDefined(typeof(CallerLineNumberAttribute), false))
                .Select(parameter => parameter.ParameterType.ToString());
        }

        private static bool IsAction(Type type, Type argument)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Action<>)
                && type.GetGenericArguments()[0].ToString() == argument.ToString();
        }

        #endregion
    }
}
