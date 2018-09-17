namespace SignalXLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    //https://haacked.com/archive/2014/11/11/async-void-methods/
    public static class AssertExtensions
    {
        public static IEnumerable<MethodInfo> GetAsyncVoidMethods(this Assembly assembly)
        {
            return assembly.GetLoadableTypes()
                .SelectMany(
                    type => type.GetMethods(
                        BindingFlags.NonPublic
                        | BindingFlags.Public
                        | BindingFlags.Instance
                        | BindingFlags.Static
                        | BindingFlags.DeclaredOnly))
                .Where(method => method.HasAttribute<AsyncStateMachineAttribute>())
                .Where(method => method.ReturnType == typeof(void));
        }

        public static bool HasAttribute<TAttribute>(this MethodInfo method)
            where TAttribute : Attribute
        {
            return method.GetCustomAttributes(typeof(TAttribute), false).Any();
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static void AssertNoAsyncVoidMethods(Assembly assembly)
        {
            List<string> messages = assembly
                .GetAsyncVoidMethods()
                .Select(
                    method =>
                        string.Format(
                            "'{0}.{1}' is an async void method.",
                            method.DeclaringType.Name,
                            method.Name))
                .ToList();

            if (messages.Any())
                throw new Exception("Async void methods found!" + Environment.NewLine + string.Join(Environment.NewLine, messages));
        }
    }
}