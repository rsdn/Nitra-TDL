using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonToTdl
{
    partial class Program
    {
        static bool AcceptProperty(string name)
        {
            if (_reader.TokenType == JsonToken.PropertyName)
            {
                if (_reader.Value is string value && value == name)
                {
                    _reader.Read();
                    return true;
                }
            }
            return false;
        }

        static bool AcceptProperty(out string name)
        {
            if (_reader.TokenType == JsonToken.PropertyName)
            {
                if (_reader.Value is string value)
                {
                    _reader.Read();
                    name = value;
                    return true;
                }
            }
            name = null;
            return false;
        }

        static bool Accept(JsonToken token, out object value)
        {
            value = _reader.Value;
            if (_reader.TokenType == token)
            {
                _reader.Read();
                return true;
            }
            return false;
        }

        static bool Accept(JsonToken token)
        {
            if (_reader.TokenType == token)
            {
                _reader.Read();
                return true;
            }
            return false;
        }

        static void ExpectString(out string value)
        {
            Expect(JsonToken.String, out var x);
            value = (string)x;
        }

        static void Expect(JsonToken token, out object value)
        {
            if (Accept(token, out value))
                return;
            Debug.Assert(false, $"Unexpected token {_reader.TokenType}");
            Unexpected();
        }

        static void Expect(JsonToken token)
        {
            if (Accept(token))
                return;
            Debug.Assert(false, $"Unexpected token {_reader.TokenType}");
            Unexpected();
        }

        static void ExpectProperty(string name)
        {
            if (Accept(JsonToken.PropertyName, out var value) && value is string propertyName)
            {
                Debug.Assert(propertyName == name);
                return;
            }
            Debug.Assert(false, $"Unexpected token {_reader.TokenType}");
            Unexpected();
        }

        static void Unexpected()
        {
            throw new Exception($"Unexpected '{_reader.Value}' ({_reader.TokenType}) at line {_reader.LineNumber} column {_reader.LinePosition}");
        }
    }
}
