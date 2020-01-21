using System;
using Tdl2Json;

namespace Tdl
{
    /// <summary>An example implementation of custom processing for Tdl2Json.exe.</summary>
    public static class Test
    {
        static Test()
        {
            // проверка компилируемости
            _ = (TransfomationFunc)DoTransformator;
        }

        public static void DoTransformator(TransformationContext context)
        {
            Console.WriteLine($"output:'{context.OutputPath}'");
            Console.WriteLine("Symbols:");
            // Enumerate all symbolss in the global namespace...
            foreach (var symbol in context.RootNamespace.MemberTable.AllSymbols)
                Console.WriteLine("    " + symbol.Name);
        }
    }
}
