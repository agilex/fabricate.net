using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;

namespace agilex.fabrication
{
    ///<summary>
    // ReSharper disable CSharpWarnings::CS1570
    ///
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
    ///  Fabricate.CollectionOf<MyClass>(index => new []{string.Format("carg1 {0}", index)});
    /// 6. var myCollectionWithObjectInitialiserOverriden = Fabricate.CollectionOf<MyClass>();
    ///  Fabricate.CollectionOf<MyClass>((myclass, index) => myclass.Prop1 = string.Format("prop1 {0}", index));
    /// Etc.
    ///
    // ReSharper restore CSharpWarnings::CS1570
    ///</summary>
    public static class Fabricate
    {
        static IContainer _container;

        static Fabricate()
        {
            ConfigureAutomapper(new[] {Assembly.GetCallingAssembly()});
        }

        static void ConfigureAutomapper(Assembly[] assemblies)
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
        /// <param name = "assemblies">Assemblies to scan for Fabricators</param>
        public static void RegisterFabricatorsIn(Assembly[] assemblies)
        {
            ConfigureAutomapper(assemblies);
        }

        static bool ImplementesFabricatorInterface(Type implementingType)
        {
            return GetFabricationInterface(implementingType) != null;
        }

        static Type GetFabricationInterface(Type implementingType)
        {
            return implementingType.GetInterfaces().FirstOrDefault(
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IFabricator<>));
        }

        /// <summary>
        /// Fabricates instance
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name = "constructorArgs">Arguments to construct instance with</param>
        /// <returns>Fabricated instance</returns>
        public static TResult InstanceOf<TResult>(Object[] constructorArgs) where TResult : class
        {
            return InstanceOf<TResult>(fab => fab.FabricateInstance(constructorArgs));
        }

        /// <summary>
        /// Fabricates instance
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name = "objectInitialiser">Action to call for object initialisation</param>
        /// <returns>Fabricated instance</returns>
        public static TResult InstanceOf<TResult>(Action<TResult> objectInitialiser) where TResult : class
        {
            return InstanceOf<TResult>(fab => fab.FabricateInstance(objectInitialiser));
        }

        /// <summary>
        /// Fabricates instance
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name = "constructorArgs">Arguments to construct instance with</param>
        /// <param name = "objectInitialiser">Action to call for object initialisation</param>
        /// <returns>Fabricated instance</returns>
        public static TResult InstanceOf<TResult>(object[] constructorArgs, Action<TResult> objectInitialiser)
            where TResult : class
        {
            return InstanceOf<TResult>(fab => fab.FabricateInstance(constructorArgs, objectInitialiser));
        }

        /// <summary>
        /// Fabricates instance
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <returns>Fabricated instance</returns>
        public static TResult InstanceOf<TResult>() where TResult : class
        {
            return InstanceOf<TResult>(fab => fab.FabricateInstance());
        }

        static IFabricator<TResult> BuildDefaultFabricator<TResult>() where TResult : class
        {
            var cons =
                typeof (TResult).GetConstructors(BindingFlags.Public | BindingFlags.Default | BindingFlags.Instance);
            return cons.Length == 1 ? new DefaultFabricator<TResult>() : null;
        }

        /// <summary>
        /// Fabricates collection of instances
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name="size">Number of items to fabricate</param>
        /// <param name = "constructorArgs">Arguments to construct instance with</param>
        /// <param name = "objectInitialiser">Action to call for object initialisation</param>
        /// <returns>Collection of fabricated instances</returns>
        public static IEnumerable<TResult> CollectionOf<TResult>(int size, Func<int, Object[]> constructorArgs,
                                                                 Action<TResult, int> objectInitialiser)
            where TResult : class
        {
            return
                CollectionOf<TResult>(
                    fabricator => fabricator.FabricateCollectionOf(size, constructorArgs, objectInitialiser));
        }

        /// <summary>
        /// Fabricates collection of instances
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name="size">Number of items to fabricate</param>
        /// <returns>Collection of fabricated instances</returns>
        public static IEnumerable<TResult> CollectionOf<TResult>(int size,
                                                                 Action<TResult, int> collectionObjectInitialiser)
            where TResult : class
        {
            return CollectionOf<TResult>(fab => fab.FabricateCollectionOf(size, collectionObjectInitialiser));
        }

        static IFabricator<TResult> ResolveFabricator<TResult>() where TResult : class
        {
            IFabricator<TResult> fabricator;
            _container.TryResolve(out fabricator);
            return fabricator;
        }

        static TResult InstanceOf<TResult>(Func<IFabricator<TResult>, TResult> fabricate) where TResult : class
        {
            var fabricator = ResolveFabricator<TResult>() ?? BuildDefaultFabricator<TResult>();
            if (fabricator != null) return fabricate(fabricator);

            throw new Exception("Could not fabricate collection");
        }

        static IEnumerable<TResult> CollectionOf<TResult>(Func<IFabricator<TResult>, IEnumerable<TResult>> fabricate)
            where TResult : class
        {
            var fabricator = ResolveFabricator<TResult>() ?? BuildDefaultFabricator<TResult>();
            if (fabricator != null) return fabricate(fabricator);

            throw new Exception("Could not fabricate collection");
        }

        /// <summary>
        /// Fabricates collection of instances
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name="size">Number of items to fabricate</param>
        /// <param name = "constructorArgs">Arguments to construct instance with</param>
        /// <returns>Collection of fabricated instances</returns>
        public static IEnumerable<TResult> CollectionOf<TResult>(int size, Func<int, object[]> constructorArgs)
            where TResult : class
        {
            return CollectionOf<TResult>(fab => fab.FabricateCollectionOf(size, constructorArgs));
        }
    }
}