== Fabricate

for quickly constructing instances of objects, particularly useful in unit testing environments where
you often want to construct an object that looks a certain way many times

define the object creation rules in fabricator and allow fabricate to construct you a new object anywhere you require it

// example pre-defined fabricator
class user_fabricator : Fabricator<User>
{
	UpdateFabricationRules(new object[] {Guid.NewGuid()}, user => user.EmailAddress = "email.address@something.com");
}

// register all your fabricators
Fabricate.RegisterFabricatorsIn(new []{Assembly.GetAssembly(typeof (user_fabricator))});

// fabricate using pre-defined fabricator
var user = Fabricate.InstanceOf<User>();
 
== Alternatively you can have fabricate construct your objects on the fly according to the parameters you provide

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