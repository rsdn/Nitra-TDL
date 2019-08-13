using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Tdl2Json;

namespace Tdl.Transformator.Tests.CommonServices
{
    public static class Context
    {
        public static TransformationContext GetContext([NotNull] string path, [NotNull, ItemNotNull] IEnumerable<string> files)
        {
            TransformationContext context = null;
            var references = new[] { Path.Combine(Directory.GetCurrentDirectory(), "TestDll.dll") };
            //Generate(
            //string workingDirectory, string[] sourceFiles, string[] references,
            // string deploymentScriptHeader, string deploymentToolPath, bool isMethodTypingEnabled, Lazy< TextWriter > output, string transformatorOutput, TransfomationFunc transformatorOpt, bool isTestMode, BooleanMarshalMode booleanMarshalMode, string jsonSchemaType);
            var compilerMessages = JsonGenerator.Generate(
                path,
                files.ToArray(),
                references: references,
                deploymentScriptHeader: "",
                deploymentToolPath: "", 
                isMethodTypingEnabled: true,
                output: null,
                transformatorOutput: null,
                transformatorOpt: transformationContext => context = transformationContext,
                isTestMode: false,
                booleanMarshalMode: BooleanMarshalMode.Boolean,
                jsonSchemaType: "prod");

            if (context == null)
            {
                var compileOutput = string.Join("\r\n", compilerMessages.Messages.Select(msg => msg.ToString()));
                throw new InvalidOperationException($"Transformation context was not generated. Perhaps syntax error. Compiler output:\r\n{compileOutput}\r\n");
            }

            return context;
        }
    }
} 
