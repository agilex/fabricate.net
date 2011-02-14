using agilex.fabrication;
using agilex.fabrication.specs;
using agilex.fabrication.specs.Classes;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Fabricate.RegisterFabricatorsIn(new[] {typeof (Class1).Assembly});
            var spec1 = new AutoFabricator_RegistrationConcerns();


            


        }
    }
}