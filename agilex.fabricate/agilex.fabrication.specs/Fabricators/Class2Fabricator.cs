using agilex.fabrication.specs.Classes;

namespace agilex.fabrication.specs.Fabricators
{
    public class Class2Fabricator : Fabricator<Class2>
    {
        public override void FabricationRules(FabricatorConfig fabricatorConfig)
        {
            fabricatorConfig.SetConstructorArgs(new object[]
                                                    {
                                                        Fabricate.CollectionOf<Class1>(10,
                                                                                       (c1, index) =>
                                                                                       c1.SettableProperty =
                                                                                       string.Format("c2 {0}", index))
                                                    });
        }
    }
}