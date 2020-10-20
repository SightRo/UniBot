using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniBot.Core.Utils
{
    public static class Reflector
    {
        public static List<Type> FindInterfaceImplementations<TInterface>(Assembly assembly)
            // No way to specify interface as type constraint.
            where TInterface : class
        {
            var interfaceImpls = assembly
                .GetTypes()
                .Where(x => x.GetInterface(nameof(TInterface)) != null && !x.IsAbstract && !x.IsInterface)
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
            return assembly.GetCustomAttribute<Attribute>() != null;
        }
    }
}