using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniBot.Core.Utils
{
    internal static class Reflector
    {
        public static List<Type> FindInterfaceImplementations(Assembly assembly, string interfaceName)
        {
            var interfaceImpls = assembly
                .GetTypes()
                .Where(x => x.GetInterface(interfaceName) != null && !x.IsAbstract && !x.IsInterface)
                .ToList();

            return interfaceImpls;
        }

        public static List<Type> FindAttributeUsage<TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            var attributedTypes = assembly
                .GetTypes()
                .Where(t => t.IsDefined(typeof(TAttribute)))
                .ToList();

            return attributedTypes;
        }

        public static bool CheckAssemblyAttribute<TAttribute>(Assembly assembly)
            where TAttribute : Attribute
        {
            return assembly.GetCustomAttribute<TAttribute>() != null;
        }

        public static TType? GetInstance<TType>(Type type)
            where TType : class
        {
            if (type.IsValueType)
                return default!;
            if (type.GetConstructor(Type.EmptyTypes) != null)
                return Activator.CreateInstance(type) as TType;

            throw new ArgumentException("No available parameterless constructor", nameof(type));
        }
    }
}