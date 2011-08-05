using System;
using System.Collections.Generic;

namespace agilex.fabrication
{
    ///<summary>
    ///  The default fabricator class. Usage:
    ///  1. Inherit from this class to provide registered fabricators for tests.
    ///  Use the FabricationRules method to configure your fabricator using FabricatorConfig
    ///  2. Use instances of the generic form to fabricate instances or collections. eg.
    ///  MyClass myInstance = new Fabricator<MyClass>().FabricateInstance();    
    ///
    ///  You can optionally provide constructor args and object initialisers to both 
    ///  instance fabrication and collection fabrication
    ///</summary>
    ///<typeparam name = "T">The class you wish to fabricate</typeparam>
    public abstract class Fabricator<T> : IFabricator<T> where T : class
    {
        Func<T> _constructionDelegate;
        object[] _constructorArgs;
        Action<T> _objectInitialiser;

        protected Fabricator()
        {
            FabricationRules(new FabricatorConfig(this));
        }

        #region IFabricator<T> Members

        /// <summary>
        ///   Fabricate!
        /// </summary>
        /// <returns>Instance of type T</returns>
        public T FabricateInstance()
        {
            return FabricateInstance(_constructorArgs, _objectInitialiser);
        }

        /// <summary>
        ///   Fabricate collection!
        /// </summary>
        /// <param name = "size">Size of collection to create</param>
        /// <returns>Collection of type T</returns>
        public IEnumerable<T> FabricateCollectionOf(int size)
        {
            return FabricateCollectionOf(size, null, null);
        }

        /// <summary>
        ///   Fabricate collection and override the object initialisation
        /// </summary>
        /// <param name = "size">Size of collection to create</param>
        /// <param name = "collectionObjectInitialiser">Action to call for each object created (collection index is supplied)</param>
        /// <returns>Collection of type T</returns>
        public IEnumerable<T> FabricateCollectionOf(int size, Action<T, int> collectionObjectInitialiser)
        {
            return FabricateCollectionOf(size, null, collectionObjectInitialiser);
        }

        /// <summary>
        ///   Fabricate collection and override the constructor args
        /// </summary>
        /// <param name = "size">Size of collection to create</param>
        /// <param name = "collectionConstructorArgs"></param>
        /// <returns>Collection of type T</returns>
        public IEnumerable<T> FabricateCollectionOf(int size, Func<int, object[]> collectionConstructorArgs)
        {
            return FabricateCollectionOf(size, collectionConstructorArgs, null);
        }

        /// <summary>
        ///   Fabricate collection and override the default constructor args and object intiialisation
        /// </summary>
        /// <param name = "size">Size of collection to create</param>
        /// <param name = "collectionConstructorArgs">Func to call prior to construction of each object (collection index is supplied), Func must return constuctor args</param>
        /// <param name = "collectionObjectInitialiser">Action to call for each object created (collection index is supplied)</param>
        /// <returns>Collection of type T</returns>
        public IEnumerable<T> FabricateCollectionOf(int size, Func<int, object[]> collectionConstructorArgs,
                                                    Action<T, int> collectionObjectInitialiser)
        {
            var collection = new List<T>();
            for (var i = 0; i < size; i++)
            {
                var instance = FabricateInstance(collectionConstructorArgs == null ? null : collectionConstructorArgs(i));
                if (collectionObjectInitialiser != null) collectionObjectInitialiser.Invoke(instance, i);
                collection.Add(instance);
            }
            return collection;
        }

        /// <summary>
        ///   Fabricate but override default constructor args
        /// </summary>
        /// <param name = "constructorArgs">Array of constructor args to use in fabrication</param>
        /// <returns>Instance of type T</returns>
        public T FabricateInstance(object[] constructorArgs)
        {
            return FabricateInstance(constructorArgs, _objectInitialiser);
        }

        /// <summary>
        ///   Fabrciate but override default object initialisation
        /// </summary>
        /// <param name = "objectInitialiser">Action to call for object once fabricated (handy for setting properties)</param>
        /// <returns>Instance of type T</returns>
        public T FabricateInstance(Action<T> objectInitialiser)
        {
            return FabricateInstance(_constructorArgs, objectInitialiser);
        }

        /// <summary>
        ///   Fabricate but override default object initialisation and constructor arguments
        /// </summary>
        /// <param name = "constructorArgs">Array of constructor args to use in fabrication</param>
        /// <param name = "objectInitialiser">Action to call for object once fabricated (handy for setting properties)</param>
        /// <returns>Instance of type T</returns>
        public T FabricateInstance(object[] constructorArgs, Action<T> objectInitialiser)
        {
            try
            {
                if (_constructionDelegate != null) return _constructionDelegate();

                var item = Activator.CreateInstance(typeof (T), constructorArgs) as T;
                if (item == null) throw new Exception("Could not fabricate item");
                if (objectInitialiser != null) objectInitialiser.Invoke(item);
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Could not fabricate instance, does it have a default constructor? If not you need to provide constructor args to fabricate");
            }
        }

        public abstract void FabricationRules(FabricatorConfig fabricatorConfig);

        #endregion

        /// <summary>
        ///   Updates the default rules for fabrication of objects of type T
        /// </summary>
        /// <param name = "constructorArgs">Array of constructor args to use in fabrication</param>
        /// <param name = "objectInitialiser">Action to call for object once fabricated (handy for setting properties)</param>
        public void UpdateFabricationRules(Object[] constructorArgs, Action<T> objectInitialiser)
        {
            _constructorArgs = constructorArgs;
            _objectInitialiser = objectInitialiser;
        }

        /// <summary>
        ///   Updates the default rules for fabrication of objects of type T
        /// </summary>
        /// <param name = "objectInitialiser">Action to call for object once fabricated (handy for setting properties)</param>
        public void UpdateFabricationRules(Action<T> objectInitialiser)
        {
            _objectInitialiser = objectInitialiser;
        }

        /// <summary>
        ///   Updates the default rules for fabrication of objects of type T
        /// </summary>
        /// <param name = "constructorArgs">Array of constructor args to use in fabrication</param>
        public void UpdateFabricationRules(Object[] constructorArgs)
        {
            _constructorArgs = constructorArgs;
        }

        /// <summary>
        ///   Updates the default rules for fabrication of objects of type T
        /// </summary>
        /// <param name = "constructionDelegate"></param>
        public void UpdateFabricationRules(Func<T> constructionDelegate)
        {
            _constructionDelegate = constructionDelegate;
        }

        #region Nested type: FabricatorConfig

        /// <summary>
        /// The fabrication configuration object, allows the user to configure a fabricator
        /// Is supplied to you via the FabricationRules abstract method
        /// </summary>
        public class FabricatorConfig
        {
            public Action<object[]> SetConstructorArgs;
            public Action<Func<T>> SetConstructorDelegate;
            public Action<Action<T>> SetObjectInitializer;

            public FabricatorConfig(Fabricator<T> fabricator)
            {
                SetConstructorArgs = fabricator.UpdateFabricationRules;
                SetConstructorDelegate = fabricator.UpdateFabricationRules;
                SetObjectInitializer = fabricator.UpdateFabricationRules;
            }
        }

        #endregion
    }
}