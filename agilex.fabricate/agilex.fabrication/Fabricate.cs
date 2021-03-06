﻿using System;
using System.Collections.Generic;
using System.IO;
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
    /// 1. var myInstance = Fabricate.New<MyClass>();
    /// 2. var myInstanceWithConstructorArgsOverriden = Fabricate.New<MyClass>(new []{"carg1", "carg2"});
    /// 3. var myInstanceWithObjectInitialiserOverriden = Fabricate.New<MyClass>(myclass => myClass.Prop1 = "prop1");
    /// 4. var myCollection = Fabricate.NewCollection<MyClass>();
    /// 5. var myCollectionWithConstructorArgsOverriden = 
    ///  Fabricate.NewCollection<MyClass>(index => new []{string.Format("carg1 {0}", index)});
    /// 6. var myCollectionWithObjectInitialiserOverriden = Fabricate.NewCollection<MyClass>();
    ///  Fabricate.NewCollection<MyClass>((myclass, index) => myclass.Prop1 = string.Format("prop1 {0}", index));
    /// Etc.
    ///
    // ReSharper restore CSharpWarnings::CS1570
    ///</summary>
    public static class Fabricate
    {
        static IContainer _container;

        static Fabricate()
        {
            var executingLocation = new FileInfo(Assembly.GetExecutingAssembly().Location);
            if (executingLocation.Directory != null)
            {
                // look for potential unloaded fabricator assemblies in the executing dir
                var potentialFabricatorAssemblies = executingLocation.Directory.EnumerateFiles().Where(
                    x =>
                    (((x.FullName.ToLower().Contains("fabrication")) || (x.FullName.ToLower().Contains("fabricator"))) &&
                     (x.Extension.ToLower() == ".dll") && (!x.FullName.ToLower().Contains("agilex.fabrication"))));

                // load additional assemblies
                potentialFabricatorAssemblies.ToList().ForEach(x => Assembly.LoadFile(x.FullName));
            }

            // refresh assembly list
            var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies();

            // finally configure the container
            ConfigureAutomapper(assembliesToScan);
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
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IFabricator<>));
        }

        /// <summary>
        /// Fabricates instance
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name = "constructorArgs">Arguments to construct instance with</param>
        /// <returns>Fabricated instance</returns>
        public static PostFabrication<TResult> New<TResult>(Object[] constructorArgs) where TResult : class
        {
            return New<TResult>(fab => fab.FabricateInstance(constructorArgs));
        }

        /// <summary>
        /// Fabricates instance
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name = "objectInitialiser">Action to call for object initialisation</param>
        /// <returns>Fabricated instance</returns>
        public static PostFabrication<TResult> New<TResult>(Action<TResult> objectInitialiser) where TResult : class
        {
            return New<TResult>(fab => fab.FabricateInstance(objectInitialiser));
        }

        /// <summary>
        /// Fabricates instance
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name = "constructorArgs">Arguments to construct instance with</param>
        /// <param name = "objectInitialiser">Action to call for object initialisation</param>
        /// <returns>Fabricated instance</returns>
        public static PostFabrication<TResult> New<TResult>(object[] constructorArgs, Action<TResult> objectInitialiser)
            where TResult : class
        {
            return New<TResult>(fab => fab.FabricateInstance(constructorArgs, objectInitialiser));
        }

        /// <summary>
        /// Fabricates instance
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <returns>Fabricated instance</returns>
        public static PostFabrication<TResult> New<TResult>() where TResult : class
        {
            return New<TResult>(fab => fab.FabricateInstance());
        }

        static IFabricator<TResult> BuildDefaultFabricator<TResult>() where TResult : class
        {
            var cons =
                typeof(TResult).GetConstructors(BindingFlags.Public | BindingFlags.Default | BindingFlags.Instance);
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
        public static PostFabrication<IEnumerable<TResult>> NewCollection<TResult>(int size, Func<int, Object[]> constructorArgs,
                                                                 Action<TResult, int> objectInitialiser)
            where TResult : class
        {
            return
                NewCollection<TResult>(
                    fabricator => fabricator.FabricateCollectionOf(size, constructorArgs, objectInitialiser));
        }

        /// <summary>
        /// Fabricates collection of instances
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name="size">Number of items to fabricate</param>
        /// <returns>Collection of fabricated instances</returns>
        public static PostFabrication<IEnumerable<TResult>> NewCollection<TResult>(int size,
                                                                 Action<TResult, int> collectionObjectInitialiser)
            where TResult : class
        {
            return NewCollection<TResult>(fab => fab.FabricateCollectionOf(size, collectionObjectInitialiser));
        }

        static IFabricator<TResult> ResolveFabricator<TResult>() where TResult : class
        {
            IFabricator<TResult> fabricator;
            _container.TryResolve(out fabricator);
            return fabricator;
        }

        static PostFabrication<TResult> New<TResult>(Func<IFabricator<TResult>, TResult> fabricate) where TResult : class
        {
            var fabricator = ResolveFabricator<TResult>() ?? BuildDefaultFabricator<TResult>();
            if (fabricator != null) return new PostFabrication<TResult>(fabricate(fabricator));

            throw new Exception("Could not fabricate collection");
        }

        static PostFabrication<IEnumerable<TResult>> NewCollection<TResult>(Func<IFabricator<TResult>, IEnumerable<TResult>> fabricate)
            where TResult : class
        {
            var fabricator = ResolveFabricator<TResult>() ?? BuildDefaultFabricator<TResult>();
            if (fabricator != null) return new PostFabrication<IEnumerable<TResult>>(fabricate(fabricator));

            throw new Exception("Could not fabricate collection");
        }

        /// <summary>
        /// Fabricates collection of instances
        /// </summary>
        /// <typeparam name = "TResult">Type to fabricate</typeparam>
        /// <param name="size">Number of items to fabricate</param>
        /// <param name = "constructorArgs">Arguments to construct instance with</param>
        /// <returns>Collection of fabricated instances</returns>
        public static PostFabrication<IEnumerable<TResult>> NewCollection<TResult>(int size, Func<int, object[]> constructorArgs)
            where TResult : class
        {
            return NewCollection<TResult>(fab => fab.FabricateCollectionOf(size, constructorArgs));
        }
    }
}