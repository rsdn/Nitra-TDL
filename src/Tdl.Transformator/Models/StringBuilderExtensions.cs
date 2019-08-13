using JetBrains.Annotations;

namespace Tdl.Transformator.Models
{
    internal static class StringBuilderExtensions
    {
        [NotNull]
        public static string Print([CanBeNull] this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "\"\"";
            }

            return str.Contains("\\") 
                ? $"@\"{str}\"" 
                : $"\"{str}\"";
        }
    }
}
