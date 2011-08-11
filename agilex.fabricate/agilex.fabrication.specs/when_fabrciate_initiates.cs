using Machine.Specifications;
using agilex.fabrication.specs.Classes;

namespace agilex.fabrication.specs
{
    [Subject(typeof(Fabricate), "Auto registration")]
    public class when_fabrciate_initiates
    {

        It should_be_able_to_fabricate = () => Fabricate.InstanceOf<Class1>();
    }
}