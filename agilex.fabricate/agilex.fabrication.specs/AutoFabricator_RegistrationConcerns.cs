using agilex.fabrication.specs.Classes;

namespace agilex.fabrication.specs
{
    public class AutoFabricator_RegistrationConcerns
    {
        public AutoFabricator_RegistrationConcerns()
        {
            // fabricators should be available for Class1 and Class2
            var instanceOfClass1 = Fabricate.InstanceOf<Class1>();
            // should be populated as per construction registered in Class1Fabricator
            var instanceOfClass2 = Fabricate.InstanceOf<Class2>();
            // should be populated as per construction registered in Class1Fabricator
            var instanceOfClass3 = Fabricate.InstanceOf<Class1>(new object[] {});
        }
    }
}