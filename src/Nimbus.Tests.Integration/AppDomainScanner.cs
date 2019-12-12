using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Extensions;

namespace Nimbus.Tests.Integration
{
    public static class AppDomainScanner
    {
        private static readonly Lazy<Assembly[]> _myAssemblies = new Lazy<Assembly[]>(ScanAppDomain);

        public static Assembly[] MyAssemblies => _myAssemblies.Value;

        private static Assembly[] ScanAppDomain()
        {
            ForceLoadAllReferencedAssemblies();

            var assemblies = AppDomain.CurrentDomain
                                      .GetAssemblies()
                                      .Where(a => IsMyAssembly(a.GetName()))
                                      .ToArray();

            return assemblies;
        }

        private static bool IsMyAssembly(AssemblyName an)
        {
            return an.Name.Split('.').First() == "Nimbus";
        }

        private static void ForceLoadAllReferencedAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var alreadyLoaded = assemblies.Select(a => a.GetName()).ToList();
            ForceLoadAllReferencedAssemblies(assemblies, alreadyLoaded);
        }

        private static void ForceLoadAllReferencedAssemblies(Assembly[] assembliesToTraverse, ICollection<AssemblyName> alreadyLoaded)
        {
            foreach (var assembly in assembliesToTraverse)
            {
                if (!IsMyAssembly(assembly.GetName())) continue;

                var referencedAssemblies = assembly.GetReferencedAssemblies();
                var unloadedReferencedAssemblies = referencedAssemblies
                    .Where(an => alreadyLoaded.None(already => already.FullName == an.FullName))
                    .ToArray();
                var newlyLoadedAssemblies = unloadedReferencedAssemblies
                    .Do(alreadyLoaded.Add)
                    .Select(Assembly.Load)
                    .ToArray();
                ForceLoadAllReferencedAssemblies(newlyLoadedAssemblies, alreadyLoaded);
            }
        }
    }
}