using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace JsonToTdl
{
    partial class Program
    {
        private const string ProductTypeName = "Kavkis";

        private static JsonTextReader                               _reader;
        private static          StringBuilder                       _builder             = new StringBuilder();
        private static readonly Dictionary<string, HashSet<string>> _productSets         = new Dictionary<string, HashSet<string>>();
        private static readonly Dictionary<string, HashSet<string>> _platformSets        = new Dictionary<string, HashSet<string>>();
        private static readonly Dictionary<string, HashSet<string>> _scenarioSets        = new Dictionary<string, HashSet<string>>();
        private static Dictionary<string, HashSet<string>>          _deploymentSets      = new Dictionary<string, HashSet<string>>();
        private static readonly Dictionary<string, JsonToken>       _parametersCollector = new Dictionary<string, JsonToken>();
        private static string                                       _scenarios;
        private static string                                       _suites;
        private static string                                       _platforms;
        private static string                                       _products;
        private static string                                       _deployments;

        static void Main(string[] args)
        {
            if (args.Length != 1 || args[0] == "-h" || args[0] == "/h" || args[0] == "-?" || args[0] == "/?")
            {
                Print("JsonToTdl.exe json_file_name", ConsoleColor.Yellow);
                return;
            }
            try
            {
                ConvertJsons(args[0]);
                Print("The TDL files was created successfully.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Print(ex.Message, ConsoleColor.Red);
            }
        }

        private static void Print(string text, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }

        private static void ConvertJsons(string startFilePath)
        {
            var path = Path.GetDirectoryName(startFilePath);
            var paths = new List<string>() { startFilePath };
            for (int i = 0; i < paths.Count; i++)
            {
                var filePath = paths[i];
                if (!File.Exists(filePath))
                    throw new Exception($"The '{filePath}' file does not exists.");
                using (StreamReader file = File.OpenText(filePath))
                using (_reader = new JsonTextReader(file))
                {
                    while (_reader.Read())
                    {
                        if (AcceptProperty("Deployments"))
                        {
                            _builder.Length = 0;
                            ConvertTestDeployments();
                            _deployments = _builder.ToString();
                        }
                        if (AcceptProperty("TestScenarios"))
                        {
                            _builder.Length = 0;
                            ConvertTestScenarios();
                            _scenarios = _builder.ToString();
                        }
                        if (AcceptProperty("Suites"))
                        {
                            _builder.Length = 0;
                            ConvertTestSuites();
                            _suites = _builder.ToString();
                        }
                        if (AcceptProperty("Platforms"))
                        {
                            _builder.Length = 0;
                            ConvertTestPlatforms();
                            _platforms = _builder.ToString();
                        }
                        if (AcceptProperty("Products"))
                        {
                            _builder.Length = 0;
                            ConvertTestProducts();
                            _products = _builder.ToString();
                        }
                        if (AcceptProperty("$include"))
                        {
                            Expect(JsonToken.StartArray);
                            while (Accept(JsonToken.String, out var value) && value is string filreName)
                                paths.Add(Path.Combine(path, filreName));
                            Expect(JsonToken.EndArray);
                        }

                        if (_reader.Value != null)
                            Debug.WriteLine("Token: {0}, Value: {1}", _reader.TokenType, _reader.Value);
                        else
                            Debug.WriteLine("Token: {0}", _reader.TokenType);

                    }
                }
            }
            _builder.Length = 0;
            GenereteSet("product", _productSets);
            GenereteSet("platform", _platformSets);
            GenereteSet("scenario", _scenarioSets);
            GenereteSet("deployment", _deploymentSets);
            _builder.AppendLine(_scenarios);
            File.WriteAllText("scenarios.tdl", _builder.ToString());
            File.WriteAllText("suites.tdl", _suites);
            File.WriteAllText("platforms.tdl", _platforms);
            File.WriteAllText("products.tdl", GenerateProductType() + _products);
            File.WriteAllText("deployments.tdl", _deployments);
        }

        private static void ConvertTestDeployments()
        {
            Expect(JsonToken.StartObject);

            while (Accept(JsonToken.PropertyName, out var value))
            {
                _builder.Append("\r\ndeployment ");
                _builder.Append((string)value);
                Expect(JsonToken.StartObject);
                ExpectProperty("Type");
                Expect(JsonToken.String, out var type);
                var typeStr = (string)type;
                if (typeStr.Equals("Script", StringComparison.OrdinalIgnoreCase))
                    ConvertScript();
                else if (typeStr.Equals("Sequence", StringComparison.OrdinalIgnoreCase))
                    ConvertCarryng();
            }

            Expect(JsonToken.EndObject);
        }

        private static void ConvertCarryng()
        {
            ExpectProperty("Deployments");
            _builder.Append(" = ");
            Expect(JsonToken.StartArray);
            while (Accept(JsonToken.StartObject))
            {
                while (AcceptProperty(out var deploymentSetName))
                {
                    Expect(JsonToken.StartObject);
                    _builder.Append(deploymentSetName);
                    _builder.Append("(");
                    var args = new List<string>();
                    while (AcceptProperty(out var argName))
                        args.Add(argName + ": " + GetValue());
                    _builder.Append(string.Join(", ", args));
                    _builder.AppendLine(");");
                    Expect(JsonToken.EndObject);
                }
                Expect(JsonToken.EndObject);
            }

            var deploymentSet = new List<string>();
            while (Accept(JsonToken.String, out var value) && value is string name) // Set of deployments
                deploymentSet.Add(name);

            if (deploymentSet.Count > 0)
            {
                _builder.Append(string.Join(", ", deploymentSet));
                _builder.AppendLine(";");
            }

            Expect(JsonToken.EndArray);
            Expect(JsonToken.EndObject);
        }

        private static void ConvertScript()
        {
            _builder.Append("(");
            ExpectProperty("ScriptPath");
            ExpectString(out var scriptPath);


            if (AcceptProperty("ScriptArgs"))
            {
                _builder.AppendLine();
                Expect(JsonToken.StartObject);
                var args = new List<(string t, string n, string c)>();
                var defs = new List<(string n, string c)>();
                while (AcceptProperty(out var name))
                {
                    switch (_reader.TokenType)
                    {
                        case JsonToken.Integer:
                            args.Add(("int    ", name, _reader.Value.ToString()));
                            break;
                        case JsonToken.Float:
                            args.Add(("double ", name, _reader.Value.ToString()));
                            break;
                        case JsonToken.String:
                            var str = (string)_reader.Value;
                            if (str.StartsWith("$("))
                            {
                                var content = str.Substring(2, str.Length - 3);
                                if (content == name)
                                    args.Add(("string ", name, null));
                                else
                                {
                                    defs.Add((name, content));
                                    args.Add(("string ", content, null));
                                }
                            }
                            else
                                args.Add(("string ", name, $"@\"{str}\""));//.Replace(@"\\", @"\")
                            break;
                        case JsonToken.Boolean:
                            args.Add(("bool   ", name, (bool)_reader.Value ? "true" : "false"));
                            break;
                        case JsonToken.Null:
                            args.Add(("string ", name, "null"));
                            break;
                        default:
                            Unexpected();
                            break;
                    }
                    Expect(_reader.TokenType);
                }
                var parms = args.OrderBy(a => a.c != null)
                                .ThenByDescending(a => a.t)
                                .ThenBy(a => a.n)
                                .Select(a => "    " + a.t + a.n + (a.c == null ? "" : " = " + a.c))
                                .ToArray();
                _builder.AppendLine(string.Join(",\r\n", parms));
                _builder.AppendLine(")");
                _builder.Append("    script ");
                _builder.AppendLine("@\"" + scriptPath + "\"");
                _builder.AppendLine("{");
                var defsStrs = defs.Select(d => "    " + d.n + " = " + d.c + ";");
                foreach (var def in defsStrs)
                    _builder.AppendLine(def);

                Expect(JsonToken.EndObject);
            }
            else
            {
                _builder.AppendLine(")");
                _builder.Append("    script ");
                _builder.AppendLine("@\"" + scriptPath + "\"");
                _builder.AppendLine("{");
            }

            if (AcceptProperty("ReturnValue"))
            {
                _builder.Append("    expected ");
                Expect(JsonToken.Integer, out var expected);
                _builder.Append(expected);
                _builder.AppendLine(";");
            }

            _builder.AppendLine("}");

            Expect(JsonToken.EndObject);
        }

        private static string GetValue()
        {
            try
            {
                switch (_reader.TokenType)
                {
                    case JsonToken.Integer:
                        return _reader.Value.ToString();
                    case JsonToken.Float:
                        return _reader.Value.ToString();
                    case JsonToken.String:
                        var str = (string)_reader.Value;
                        return $"@\"{str}\""; //.Replace(@"\\", @"\")
                    case JsonToken.Boolean:
                        return (bool)_reader.Value ? "true" : "false";
                    case JsonToken.Null:
                        return "null";
                    default:
                        Unexpected();
                        return null;
                }
            }
            finally
            {
                Expect(_reader.TokenType);
            }
        }

        private static string GenerateProductType()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"product type {ProductTypeName}");
            builder.AppendLine("{");
            foreach (var parameter in _parametersCollector)
            {
                builder.Append("    ");
                switch (parameter.Value)
                {
                    case JsonToken.Integer: builder.Append("int    "); break;
                    case JsonToken.Float:   builder.Append("double "); break;
                    case JsonToken.String:  builder.Append("string "); break;
                    case JsonToken.Boolean: builder.Append("bool   "); break;
                    case var x:
                        Trace.Assert(false, "Unsupported product parameter type: " + x);
                        break;
                }
                builder.Append(parameter.Key);
                builder.AppendLine(";");
            }

            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void ConvertTestProducts()
        {
            Expect(JsonToken.StartObject);
            while (Accept(JsonToken.PropertyName, out var value) && value is string name)
            {
                _builder.Append("\r\nproduct ");
                _builder.Append(AsName(name));
                _builder.Append($" : {ProductTypeName} ");

                if (Accept(JsonToken.StartObject))
                    ConvertTestProduct();
                else
                    ConvertSet();
            }
            Expect(JsonToken.EndObject);
        }

        private static void ConvertTestProduct()
        {
            _builder.AppendLine();
            _builder.AppendLine("{");
            while (Accept(JsonToken.PropertyName, out var value) && value is string name)
            {
                if (name == "Parameters")
                    ConvertParameters(_parametersCollector);
                else
                    ConvertSimpleProperty((string)name, _parametersCollector);
            }
            Expect(JsonToken.EndObject);
            _builder.AppendLine("}");
        }

        private static void ConvertTestPlatforms()
        {
            Expect(JsonToken.StartObject);
            while (Accept(JsonToken.PropertyName, out var value))
            {
                _builder.Append("platform ");
                _builder.Append(value);
                if (Accept(JsonToken.StartObject))
                {
                    _builder.AppendLine(";");
                    Expect(JsonToken.EndObject);
                }
                else
                    ConvertSet();
            }
            Expect(JsonToken.EndObject);
        }

        private static void ConvertTestScenarios()
        {
            while (Accept(JsonToken.StartObject))
            {
                while (Accept(JsonToken.PropertyName, out var value))
                {
                    _builder.Append("\r\nscenario ");
                    _builder.Append((string)value);
                    Expect(JsonToken.StartObject);

                    if (AcceptProperty("TestScenarios"))
                        ConvertSet();
                    else
                    {
                        _builder.AppendLine("\r\n{");

                        ConvertTestScenario();

                        _builder.AppendLine("}");
                    }

                    Expect(JsonToken.EndObject);
                }

                Expect(JsonToken.EndObject);
            }
        }

        private static void ConvertTestScenario()
        {
            string deployment = null;
            string environments = null;
            string method = null;
            var oldBuilder = _builder;
            _builder = new StringBuilder();
            while (Accept(JsonToken.PropertyName, out var value))
            {
                switch ((string)value)
                {
                    case "Deployments":
                        deployment = ConvertDeployments();
                        break;
                    case "Environments":
                        environments = ConvertEnvironments();
                        break;
                    case "TestMethod":
                        method = ConvertTestMethod();
                        break;
                    default:
                        ConvertSimpleProperty((string)value);
                        break;
                }
            }

            oldBuilder.AppendLine(deployment);
            oldBuilder.AppendLine(environments);
            oldBuilder.AppendLine(method);
            oldBuilder.Append(_builder);

            _builder = oldBuilder;
        }

        private static void ConvertSet()
        {
            Expect(JsonToken.StartArray);
            var scenarios = new List<string>();
            while (Accept(JsonToken.String, out var value))
                scenarios.Add((string)value);
            _builder.Append(" =\r\n    ");
            _builder.Append(string.Join(",\r\n    ", scenarios));
            _builder.AppendLine(";");
            Expect(JsonToken.EndArray);
        }

        private static void ConvertSimpleProperty(string name, Dictionary<string, JsonToken> parametersCollector = null)
        {
            _builder.Append("    ");
            _builder.Append(name);
            _builder.Append(" = ");
            var type = _reader.TokenType;
            Debug.Assert(type == JsonToken.String || type == JsonToken.Integer || type == JsonToken.Boolean);
            Expect(type, out var value);
            if (type == JsonToken.String)
            {
                var str = (string)value;
                if (str.StartsWith("#(", StringComparison.Ordinal))
                    _builder.Append(str.Substring(2, Math.Max(str.Length - 3, 0)));
                else
                {
                    _builder.Append("@\"");
                    _builder.Append(str);
                    _builder.Append('"');
                }
            }
            else if (type == JsonToken.Boolean)
                _builder.Append((bool)value ? "true" : "false");
            else
                _builder.Append(value);
            _builder.AppendLine(";");

            if (parametersCollector != null)
            {
                if (parametersCollector.TryGetValue(name, out var prevType))
                    Trace.Assert(prevType == type);
                else
                    parametersCollector.Add(name, type);
            }
        }

        private static string ConvertTestMethod()
        {
            var methods = new List<string>();
            Expect(JsonToken.StartObject);
            void expectMethod()
            {
                ExpectProperty("AssemblyName");
                Expect(JsonToken.String);
                ExpectProperty("MethodName");
                Expect(JsonToken.String, out var value);
                methods.Add("    method " + value + ";");
            }

            if (AcceptProperty("TestSequence"))
            {
                Expect(JsonToken.StartArray);

                while (Accept(JsonToken.StartObject))
                {
                    expectMethod();

                    Expect(JsonToken.EndObject);
                }

                Expect(JsonToken.EndArray);

                if (AcceptProperty("AllowReboot"))
                    ConvertSimpleProperty("AllowReboot");
            }
            else
                expectMethod();

            Expect(JsonToken.EndObject);

            return string.Join(Environment.NewLine, methods);
        }

        private static string ConvertEnvironments()
        {
            Expect(JsonToken.StartArray);

            var environments = new List<string>();

            while (Accept(JsonToken.StartObject))
            {

                string platform = GetPlatform();
                string product  = GetProduct();

                environments.Add($"({platform}, {product})");

                Expect(JsonToken.EndObject);
            }

            Expect(JsonToken.EndArray);

            return "    environments " + string.Join(", ", environments) + ";";
        }

        private static string GetProduct()
        {
            var products = new HashSet<string>();
            ExpectProperty("Products");
            Expect(JsonToken.StartArray);
            while (Accept(JsonToken.String, out var value))
                products.Add((string)value);
            Expect(JsonToken.EndArray);
            string product = GetProduct(products);
            return product;
        }

        private static string GetPlatform()
        {
            var platforms = new HashSet<string>();
            ExpectProperty("Platforms");
            Expect(JsonToken.StartArray);
            while (Accept(JsonToken.String, out var value))
                platforms.Add((string)value);
            Expect(JsonToken.EndArray);
            string platform = GetPlatform(platforms);
            return platform;
        }

        private static string GetScenario()
        {
            var scenarios = new HashSet<string>();
            ExpectProperty("TestScenarios");
            Expect(JsonToken.StartArray);
            while (Accept(JsonToken.String, out var value))
                scenarios.Add((string)value);
            Expect(JsonToken.EndArray);
            string platform = GetScenario(scenarios);
            return platform;
        }

        private static string GetDeployment()
        {
            var set = new HashSet<string>();
            Expect(JsonToken.StartArray);
            while (Accept(JsonToken.String, out var value))
                set.Add((string)value);
            Expect(JsonToken.EndArray);
            string platform = GetDeployment(set);
            return platform;
        }

        private static string ReduceSet(Dictionary<string, HashSet<string>> sets, HashSet<string> set)
        {
            Debug.Assert(set.Count > 0);

            if (set.Count == 1)
                return AsName(set.First());

            var compositSetName = AsName(string.Join("__", set.OrderBy(x => x)));

            if (sets.TryGetValue(compositSetName, out var result))
            {
                Debug.Assert(result.SetEquals(set));
                return compositSetName;
            }

            sets.Add(compositSetName, set);
            return compositSetName;
        }

        private static string GetProduct(HashSet<string> set)
        {
            return ReduceSet(_productSets, set);
        }

        private static string GetPlatform(HashSet<string> set)
        {
            return ReduceSet(_platformSets, set);
        }

        private static string GetScenario(HashSet<string> set)
        {
            return ReduceSet(_scenarioSets, set);
        }

        private static string GetDeployment(HashSet<string> set)
        {
            return ReduceSet(_deploymentSets, set);
        }

        private static string ConvertDeployments()
        {
            return "    deployment " + GetDeployment() + ";";
        }

        private static void ConvertTestSuites()
        {
            Expect(JsonToken.StartObject);

            while (Accept(JsonToken.PropertyName, out var value))
            {
                _builder.Append("\r\nsuite ");
                _builder.AppendLine(AsName(value));
                Expect(JsonToken.StartObject);
                _builder.AppendLine("{");
                string scenario = null;

                while (_reader.TokenType == JsonToken.PropertyName)
                {
                    switch ((string)_reader.Value)
                    {
                        case "Platforms":
                            _builder.Append("    platform ");
                            _builder.Append(GetPlatform());
                            _builder.AppendLine(";");
                            break;
                        case "Products":
                            _builder.Append("    product ");
                            _builder.Append(GetProduct());
                            _builder.AppendLine(";");
                            break;
                        case "TestScenarios":
                            scenario = GetScenario();
                            break;
                        case "Parameters":
                            ExpectProperty("Parameters");
                            ConvertParameters();
                            break;
                        default:
                            Unexpected();
                            break;
                    }
                }

                if (scenario == null)
                    ExpectProperty("TestScenarios");

                _builder.Append("    ");
                _builder.Append(scenario);
                _builder.AppendLine("();");

                _builder.AppendLine("}");
                Expect(JsonToken.EndObject);
            }

            Expect(JsonToken.EndObject);
        }

        private static string AsName(object value)
        {
            var text = (string)value;
            if (!char.IsLetter(text.FirstOrDefault()) || text.Any(c => !(char.IsLetterOrDigit(c) || c == '_' || c == '-')))
                return '"' + text + '"';
            return text;
        }

        private static void ConvertParameters(Dictionary<string, JsonToken> parametersCollector = null)
        {
            Expect(JsonToken.StartObject);
            while (Accept(JsonToken.PropertyName, out var name))
                ConvertSimpleProperty((string)name, parametersCollector);
            Expect(JsonToken.EndObject);
        }

        private static void GenereteSet(string name, Dictionary<string, HashSet<string>> _sets)
        {
            foreach (var set in _sets)
            {
                _builder.Append(name);
                _builder.Append(" ");
                _builder.Append(set.Key);
                if (name == "product")
                    _builder.Append($" : {ProductTypeName} ");
                _builder.Append(" = ");
                _builder.Append(string.Join(", ", set.Value.OrderBy(x => x).Select(AsName)));
                _builder.AppendLine(";");
            }
        }
    }
}
