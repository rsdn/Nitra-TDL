using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utils
{
    public static class StringHelpers
    {
        public static string AsName(this object value)
        {
            var text = (string)value;
            if (!char.IsLetter(text.FirstOrDefault()) || text.Any(c => !(char.IsLetterOrDigit(c) || c == '_' || c == '-')))
                return '"' + text + '"';
            return text;
        }

        public static string EsqManky(this string value)
        {
            return value.Replace("\"", "\"\"");
        }

        public static string AsManky(this string value)
        {
            return "@\"" + EsqManky(value) + "\"";
        }

        public static string AsString(this string value)
        {
            if (value.Contains('\\'))
                return AsManky(value);

            return ToLiteral(value);
        }

        public static string ToLiteral(this string value, bool lifted = false)
        {
            StringBuilder literal = new StringBuilder(value.Length + 2);
            if (lifted)
                literal.Append(@"\");
            literal.Append("\"");
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\'': literal.Append(@"\'"); break;
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        // ASCII printable character
                        if (c >= 0x20 && c <= 0x7e)
                        {
                            literal.Append(c);
                            // As UTF16 escaped character
                        }
                        else
                        {
                            literal.Append(@"\u");
                            literal.Append(((int)c).ToString("x4"));
                        }
                        break;
                }
            }
            if (lifted)
                literal.Append(@"\");
            literal.Append("\"");
            return literal.ToString();
        }
    }
}
