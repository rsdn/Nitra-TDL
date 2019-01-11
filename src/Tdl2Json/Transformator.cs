using Nitra.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tdl
{
    /// <summary>An example implementation of custom processing for Tdl2Json.exe.</summary>
    public static class Test
    {
        public static List<CompilerMessage> Transformator(DotNet.NamespaceSymbol root, string output)
        {
            List<CompilerMessage> errorList = new List<CompilerMessage>();
            Console.WriteLine($"output:'{output}'");
            Console.WriteLine("Symbols:");
            // Enumerate all symbolss in the global namespace...
            foreach (var symbol in root.MemberTable.AllSymbols)
                Console.WriteLine("    " + symbol.Name);
            return errorList;
        }
    }
}
