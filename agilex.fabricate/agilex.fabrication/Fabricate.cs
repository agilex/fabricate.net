using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;

namespace agilex.fabrication
{
    /// <summary>
    /// Static class which provides DSL for Fabrication
    /// 
    /// Currently relies on Autofac to pick up custom Fabricators. Any class which implements IFabricator<T> will
    /// automagically be registered and available through the Fabricate DSL if it's visible in the executing assembly.
    /// If your Fabricators are in a separate assembly use the RegisterFabricatorsIn(Assembly[] assemblies) method
    /// to register your Fabricators with the DSL (n.b this will wipe the registry of any existing fabricators and 
    /// regenerate from the assemblies provided).
    /// 
    /// Usage:
    /// 1. var myInstance = Fabricate.InstanceOf<MyClass>();
    /// 2. var myInstanceWithConstructorArgsOverriden = Fabricate.InstanceOf<MyClass>(new []{"carg1", "carg2"});
    /// 3. var myInstanceWithObjectInitialiserOverriden = Fabricate.InstanceOf<MyClass>(myclass => myClass.Prop1 = "prop1");
    /// 4. var myCollection = Fabricate.CollectionOf<MyClass>();
    /// 5. var myCollectionWithConstructorArgsOverriden = 
    ///     Fabricate.CollectionOf<MyClass>(index => new []{string.Format("carg1 {0}", index)});
    /// 6. var myCollectionWithObjectInitialiserOverriden = Fabricate.CollectionOf<MyClass>();
    ///     Fabricate.CollectionOf<MyClass>((myclass, index) => myclass.Prop1 = string.Format("prop1 {0}", index));
    /// Etc.
    /// </summary>
    public static class Fabricate
    {
        private static IContainer _container;

        static Fabricate()
        {
            ConfigureAutomapper(new[] {Assembly.GetCallingAssembly()});
        }

        private static void ConfigureAutomapper(Assembly[] assemblies)
        {
            var cBuilder = new ContainerBuilder();
            if (assemblies.Count() > 0 && assemblies[0] != null)
            {
                cBuilder.RegisterAssemblyTypes(assemblies)
                    .Where(t => t.Name.ToLower() != "fabricator")
                    .Where(t => t.Name.ToLower().EndsWith("fabricator"))
                    .Where(ImplementesFabricatorInterface)
                    .As(t => GetFabricationInterface(t));
            }
            _container = cBuilder.Build();
        }

        /// <summary>
        /// Register fabricators with the DSL. Classes must implement IFabricator<> normally 
        /// through inheriting from Fabricator<>.
        /// </summary>
        /// <param name="assemblies">Assemblies to scan for Fabricators</param>
        public static void RegisterFabricatorsIn(Assembly[] assemblies)
        {
            ConfigureAutomapper(assemblies);
        }

        private static bool ImplementesFabricatorInterface(Type implementingType)
        {
            return GetFabricationInterface(implementingType) != null;
        }

        private static Type GetFabricationInterface(Type implementingType)
        {
            return implementingType.GetInterfaces().FirstOrDefault(
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IFabricator<>));
        }
        
        /// <summary>
        /// Fabricates instance and overrides any constructor arguments
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="constructorArgs"></param>
        /// <returns></returns>
        public static TResult InstanceOf<TResult>(Object[] constructorArgs) where TResult : class
        {
            IFabricator<TResult> fabricator;
            bool resolved = _container.TryResolve(out fabricator);
            if (!resolved) fabricator = new DefaultFabricator<TResult>();

            try
            {
                return fabricator.FabricateInstance(constructorArgs);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not fabricate item", ex);
            }
        }

        /// <summary>
        /// Fabricates instance and overrides and object initialisation
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="objectInitialiser"></param>
        /// <returns></returns>
        public static TResult InstanceOf<TResult>(Action<TResult> objectInitialiser) where TResult : class
        {
            IFabricator<TResult> fabricator;
            bool resolved = _container.TryResolve(out fabricator);
            if (!resolved) fabricator = new DefaultFabricator<TResult>();

            try
            {
                return fabricator.FabricateInstance(objectInitialiser);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not fabricate item", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="constructorArgs"></param>
        /// <param name="objectInitialiser"></param>
        /// <returns></returns>
        public static TResult InstanceOf<TResult>(object[] constructorArgs, Action<TResult> objectInitialiser)
            where TResult : class
        {
            IFabricator<TResult> fabricator;
            bool resolved = _container.TryResolve(out fabricator);
            if (!resolved) fabricator = new DefaultFabricator<TResult>();

            try
            {
                return fabricator.FabricateInstance(constructorArgs, objectInitialiser);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not fabricate item", ex);
            }
        }

        /// <summary>
        /// Fabricates instance
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static TResult InstanceOf<TResult>() where TResult : class
        {
            IFabricator<TResult> fabricator;
            bool resolved = _container.TryResolve(out fabricator);
            if (resolved) return fabricator.FabricateInstance();

            IFabricator<TResult> defaultFab = BuildDefaultFabricator<TResult>();
            if (defaultFab != null) return defaultFab.FabricateInstance();

            throw new Exception("Could not fabricate item");
        }

        private static IFabricator<TResult> BuildDefaultFabricator<TResult>() where TResult : class
        {
            ConstructorInfo[] cons =
                typeof (TResult).GetConstructors(BindingFlags.Public | BindingFlags.Default | BindingFlags.Instance);
            return cons.Length == 1 ? new DefaultFabricator<TResult>() : null;
        }

        public static IEnumerable<TResult> CollectionOf<TResult>(int size, Func<int, Object[]> constructorArgs,
                                                                 Action<TResult, int> objectInitialiser)
            where TResult : class
        {
            IFabricator<TResult> fabricator;
            bool resolved = _container.TryResolve(out fabricator);
            if (!resolved) fabricator = new DefaultFabricator<TResult>();

            try
            {
                return fabricator.FabricateCollectionOf(size, constructorArgs, objectInitialiser);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not fabricate item", ex);
            }
        }

        public static IEnumerable<TResult> CollectionOf<TResult>(int size, Action<TResult, int> objectInitialiser)
            where TResult : class
        {
            IFabricator<TResult> fabricator;
            bool resolved = _container.TryResolve(out fabricator);
            if (resolved) return fabricator.FabricateCollectionOf(size, objectInitialiser);

            IFabricator<TResult> defaultFab = BuildDefaultFabricator<TResult>();
            if (defaultFab != null) return defaultFab.FabricateCollectionOf(size, objectInitialiser);

            throw new Exception("Could not fabricate collection");
        }
    }
}