using System;
using System.Collections.Generic;

namespace agilex.fabrication
{
    /// <summary>
    /// The interface a fabricator must support to be used by Fabricate. Any classes which implement
    /// IFabricator will automagically be picked up by Fabricate and will be available in the DSL. The
    /// easiest way to do this is to simply inherit from Fabricator<>.
    /// </summary>
    /// <typeparam name="T">The type you wish to be fabricated</typeparam>
    public interface IFabricator<out T>
    {
        T FabricateInstance();
        T FabricateInstance(object[] constructorArgs);
        T FabricateInstance(Action<T> objectInitialiser);
        T FabricateInstance(object[] constructorArgs, Action<T> objectInitialiser);
        IEnumerable<T> FabricateCollectionOf(int size);
        IEnumerable<T> FabricateCollectionOf(int size, Action<T, int> collectionObjectInitialiser);
        IEnumerable<T> FabricateCollectionOf(int size, Func<int, object[]> collectionConstructorArgs,
                                             Action<T, int> collectionObjectInitialiser);
    }
}