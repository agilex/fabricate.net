using agilex.fabrication;
using agilex.fabrication.specs;
using agilex.fabrication.specs.Classes;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Fabricate.New<Class1>().Instance();
        }
    }
}