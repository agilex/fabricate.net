using agilex.fabrication.specs.Classes;

namespace agilex.fabrication.specs.Fabricators
{
    public class Class1Fabricator : Fabricator<Class1>
    {
        public Class1Fabricator()
        {
            UpdateFabricationRules(null, c1 => c1.SettableProperty = "fabricated instance of class 1");
        }
    }
}