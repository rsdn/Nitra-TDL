using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KL.TdlTransformator.Tests.CommonServices
{
    public static class TdlAssert
    {
        private static Func<string, string, bool> _comparator;

        static TdlAssert()
        {
            _comparator = Compare;
        }

        public static void AreEqual(string expected, string actual)
        {
            Assert.IsTrue(_comparator(expected, actual));
        }

        public static void AreEqual(string expected, string actual, string message)
        {
            Assert.IsTrue(_comparator(expected, actual), message);
        }

        public static void SetTdlComparator([NotNull] Func<string, string, bool> comparator)
            => _comparator = comparator;

        private static bool Compare([NotNull] string original, [NotNull] string generated)
        {
            const string breakLinesPattern = @"\n|\r";
            original = Regex.Replace(original, breakLinesPattern, string.Empty);
            generated = Regex.Replace(generated, breakLinesPattern, string.Empty);

            const string tabPattern = @"\t|\s+";
            const string whitespace = " ";
            original = Regex.Replace(original, tabPattern, whitespace).Trim();
            generated = Regex.Replace(generated, tabPattern, whitespace).Trim();

            Console.WriteLine(original);
            Console.WriteLine(generated);

            return original == generated;
        }
    }
}