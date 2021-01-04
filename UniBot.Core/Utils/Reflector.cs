using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniBot.Core.Utils
{
    internal static class Reflector
    {
        public static IList<Type> FindInterfaceImplementations(Assembly assembly, string interfaceName)
        {
            var interfaceImpls = assembly
                .GetTypes()
                .Where(x => x.GetInterface(interfaceName) != null && !x.IsAbstract && !x.IsInterface)
                .ToList();

            return interfaceImpls;
        }

        public static Type FindInterfaceImplementation(Assembly assembly, string interfaceName)
        {
            var implementations = FindInterfaceImplementations(assembly, interfaceName);

            return implementations.Count switch
            {
                0 => throw new ArgumentException(
                    $"Not found any type that implements {interfaceName} in {assembly.FullName}"),
                > 1 => throw new ArgumentException(
                    $"Found more than one type that implements {interfaceName} in {assembly.FullName}"),
                _ => implementations[0]
            };
        }

        public static IList<Type> FindTypesWithAttribute<TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            var attributedTypes = assembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<TAttribute>() != null)
                .ToList();

            return attributedTypes;
        }

        public static Type FindTypeWithAttribute<TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            var usages = FindTypesWithAttribute<TAttribute>(assembly);

            return usages.Count switch
            {
                0 => throw new ArgumentException(
                    $"Not found any type with {typeof(TAttribute)} attribute in {assembly.FullName}"),
                > 1 => throw new ArgumentException(
                    $"Found more than one type with {typeof(TAttribute)} attribute in {assembly.FullName}"),
                _ => usages[0]
            };
        }

        public static IList<TAttribute> FindAssemblyAttributes<TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            return assembly.GetCustomAttributes<TAttribute>().ToList();
        }

        public static TAttribute FindAssemblyAttribute<TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            var usages = FindAssemblyAttributes<TAttribute>(assembly);

            return usages.Count switch
            {
                0 => throw new ArgumentException($"{assembly.FullName} assembly has no {typeof(TAttribute)} attribute"),
                > 1 => throw new ArgumentException(
                    $"{assembly.FullName} assembly has more than one {typeof(TAttribute)} attribute"),
                _ => usages[0]
            };
        }

        // TODO Add support for passing constructor parameters in any order
        public static TType GetInstance<TType>(params object[] constructorParameters)
            where TType : class
        {
            var type = typeof(TType);

            if (constructorParameters.Length > 0)
            {
                var constructor = type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    constructorParameters.Select(o => o.GetType()).ToArray(),
                    null);

                if (constructor == null)
                    throw new ArgumentException($"{type} doesn't have constructor with specified parameters");

                return (TType) constructor.Invoke(constructorParameters);
            }

            var parameterlessConstructor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);

            if (parameterlessConstructor == null)
                throw new ArgumentException($"{type} doesn't have parameterless constructor.");

            return (TType) (Activator.CreateInstance(type, true) ??
                            throw new ArgumentException($"Can not create an instance of {type}"));
        }

        public static Type? FindSubType(Assembly assembly, Type type)
        {
            return assembly.GetTypes().FirstOrDefault(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
        }
    }
}