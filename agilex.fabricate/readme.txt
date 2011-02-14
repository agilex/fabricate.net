== Fabricate

for quickly constructing instances of objects, particularly useful in unit testing environments where
you often want to construct an object that looks a certain way many times

define the object creation rules in fabricator and allow fabricate to construct you a new object anywhere you require it

// fabricated with custom pre-defined fabricator
var instanceOfMyClass = Fabricate.InstanceOf<MyClass>();
 
// fabricated with default fabricator constructed on the fly
var instanceOfDiffClass = Fabricate.InstanceOf<DifferentClass>();

// fabricated using overriden constructor args
var anotherInstanceOfDiffClass =
    Fabricate.InstanceOf<MyClass>(new Object[] {"title", new List<DifferentClass>()});

// fabricated using overriden object initialisation
var diffInstanceOfDiffClass = Fabricate.InstanceOf<DifferentClass>(dc => dc.Name = "dc1");

// fabricated using overriden constructor args and object initialisation
var anotherdiffInstanceOfDiffClass =
    Fabricate.InstanceOf<MyClass>(new Object[] {"title", new List<DifferentClass>()},
                                  mc => mc.AnotherProp = "set property");

// fabricated collection using overriden object initialisation
var fabCollection = Fabricate.CollectionOf<DifferentClass>(10,
                                                           (dc, i) => dc.Name = string.Format("dc{0}", i));

// fabricated collection using overriden object initialisation and constructor args
var anotherFabCollection = Fabricate.CollectionOf<MyClass>(10, i => new Object[]{ string.Format("title{0}", i), new List<DifferentClass>() },
                                                           (mc, j) => mc.AnotherProp = string.Format("prop {0}", j));