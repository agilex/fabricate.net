using agilex.fabrication.specs.Classes;

namespace agilex.fabrication.specs.Fabricators
{
    public class Class1Fabricator : Fabricator<Class1>
    {
        public override void FabricationRules(FabricatorConfig fabricatorConfig)
        {
 //           fabricatorConfig.SetConstructorArgs(new object[] {});
 //           fabricatorConfig.SetObjectInitializer(newObject => newObject.SettableProperty = "fabricated instance of class 1");
 //           fabricatorConfig.SetConstructorDelegate(() => new Class1());
        }
    }
}