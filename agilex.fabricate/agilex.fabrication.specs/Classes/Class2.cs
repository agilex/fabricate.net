using System.Collections.Generic;

namespace agilex.fabrication.specs.Classes
{
    public class Class2
    {
        public Class2(IEnumerable<Class1> collectionOfDependencies)
        {
            CollectionOfDependencies = collectionOfDependencies;
        }

        public IEnumerable<Class1> CollectionOfDependencies { get; protected set; }
    }
}