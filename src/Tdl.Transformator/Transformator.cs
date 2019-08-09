using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using KL.TdlTransformator;
using KL.TdlTransformator.Models;
using KL.TdlTransformator.Models.Deployments;
using KL.TdlTransformator.Models.Modules;
using KL.TdlTransformator.Models.Products;
using KL.TdlTransformator.Models.Scenario;
using KL.TdlTransformator.Processors;
using KL.TdlTransformator.Services;
using Tdl2Json;

// ReSharper disable once CheckNamespace
namespace Tdl
{
    public static class Transformator
    { 
        // ReSharper disable once UnusedMember.Global
        public static void Main([NotNull] TransformationContext context)
        {
#if DEBUG
            Console.WriteLine("Waiting for debug connection");
            Console.Read();
#endif 
            var symbolConverter = new SymbolConverter();
            var modelContainer = symbolConverter.ParseSymbols(context); 

            Console.WriteLine("input command..");

            var clo = new KL.TdlTransformator.CommandLineOptions();
            
            while (clo.NeedExit == false)
            {
                Console.WriteLine();

                var commands = Console.ReadLine()?.ToLower().Split(' ');

                clo.Parse(commands);

                switch (clo.Command)
                {
                    case ArgumentCommand.GetFiles:
                        var affectedModules = modelContainer.GetAll<ModuleModel>(clo.Mask);
                        foreach (var affectedModule in affectedModules)
                        {
                            Console.WriteLine(affectedModule.FilePath);
                        }

                        break;
                    case ArgumentCommand.GetModels:
                        var models = modelContainer.GetAll<Model>(clo.Mask);
                        foreach (var model in models)
                        {
                            var path = clo.IsShortPath ? Path.GetFileName(model.FilePath) : model.FilePath;
                            Console.WriteLine(
                                $"model: '{model.Name}', type: '{model.GetType().Name}' path: '{path}'");
                        }

                        break;
                    case ArgumentCommand.RmUnused:
                        new ScenarioProcessor().RemoveUnused(modelContainer.GetAll<ScenarioModel>(clo.Mask));
                        break;
                    case ArgumentCommand.ClrDupEnv:
                        new ScenarioProcessor().ReplaceDuplicatesWIthDifferentScenarios(modelContainer.GetAll<ScenarioModel>(clo.Mask));
                        break;
                    case ArgumentCommand.ClrEnv:
                        new ScenarioProcessor().SimplifyEnvironments(
                            modelContainer.GetAll<ScenarioModel>(clo.Mask),
                            modelContainer.GetAll<ProductSetModel>(clo.Mask));
                        break;
                    case ArgumentCommand.ClrDup:
                        new ScenarioProcessor().ReplaceDuplicates(modelContainer.GetAll<ScenarioModel>(clo.Mask));
                        break;
                    case ArgumentCommand.ClrName:
                        new ScenarioProcessor().ClearScenarioNames(modelContainer.GetAll<ScenarioModel>(clo.Mask));
                        break;
                    case ArgumentCommand.ClrSets:
                        new ScenarioProcessor().ReplaceDuplicates(modelContainer.GetAll<ScenarioSetModel>(clo.Mask));
                        break;
                    case ArgumentCommand.Sort:
                        new ModuleProcessor().SortModuleMembers(modelContainer.GetAll<ModuleModel>(clo.Mask));
                        break;
                    case ArgumentCommand.Reset:
                        modelContainer = symbolConverter.ParseSymbols(context);
                        break;
                    case ArgumentCommand.Save:
                        SaveParsed(modelContainer.GetPrintable());
                        break;
                    case ArgumentCommand.SaveMod:
                        SaveParsed(modelContainer.GetChanged());
                        break;
                    case ArgumentCommand.Ungroup:
                        var scenarios = Ungroup(clo.UngroupType, modelContainer);
                        if (clo.NeedPrintUngroup)
                        {
                            PrintScenariosInfo(scenarios, clo.UngroupInfoOutput);
                        }

                        break;
                    case ArgumentCommand.Undefined: 
                        break;
                    default:
                        clo.PrintHelp(Console.Out);
                        break;
                }
            }
        }

        [NotNull, ItemNotNull]
        private static List<ScenarioModel> Ungroup([CanBeNull] string option, [NotNull] ModelContainer container)
        {
            var processor = new ScenarioProcessor();
            List<ScenarioModel> result;

            switch (option.ToLower())
            {
                case "area":
                    result = processor.UngroupingByArea(container);
                    break;
                case "qcid":
                    result = processor.UngroupingByQcId(container);
                    break;
                default:
                    result = processor.Ungrouping(container, null);
                    break;
            }

            return result;
        }

        private static void PrintScenariosInfo(
            [NotNull, ItemNotNull] IEnumerable<ScenarioModel> scenarios,
            [CanBeNull] string output)
        {
            var writer = string.IsNullOrEmpty(output.Trim())
                ? Console.Out
                : File.CreateText(output);

            foreach (var scenario in scenarios)
            {
                writer.WriteLine($"{scenario.Name}");
            }
        }

        private static void SaveParsed([ItemNotNull, NotNull]IEnumerable<ModuleModel> enumerable)
        {
            foreach (var current in enumerable)
            {
                var directoryName = Path.GetDirectoryName(current.Name);
                if (directoryName == null)
                {
                    throw new InvalidOperationException("Can't get directory from module path");
                }

                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllText(current.Name, current.Print());
            }
        }
    }
}