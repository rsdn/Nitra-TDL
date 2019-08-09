using System;
using System.Collections.Generic;
using System.Linq;
using DotNet;
using JetBrains.Annotations;
using KL.TdlTransformator.Models;
using KL.TdlTransformator.Models.Barrier;
using KL.TdlTransformator.Models.Deployments;
using KL.TdlTransformator.Models.Externals;
using KL.TdlTransformator.Models.Modules;
using KL.TdlTransformator.Models.Platforms;
using KL.TdlTransformator.Models.Products;
using KL.TdlTransformator.Models.Scenario;
using KL.TdlTransformator.Models.Suite;
using Nitra.Declarations;
using NLog;
using Tdl;
using Tdl2Json;

namespace KL.TdlTransformator.Services
{
    public sealed class SymbolConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [NotNull]
        public ModelContainer ParseSymbols([NotNull] TransformationContext context)
        {
            var container = new ModelContainer();
            CreateModels(context.RootNamespace, container);
            container.InitializeModels();

            ParseComments(context, container);
            container.CreateSnapshot();
            return container;
        }

        [ItemNotNull]
        [NotNull]
        public static List<CommentBlock> SearchComments(
            [NotNull] CommentBag commentBag,
            [CanBeNull] CommentBlockModel comments)
        {
            var results = new List<CommentBlock>();
            if (comments != null)
            {
                if (comments.Location.HasValue)
                {
                    var before = commentBag.TryFindBefore(comments.Location.Value);
                    var after = commentBag.TryFindAfter(comments.Location.Value);
                    if (before != null)
                    {
                        // this comment is on the same line. it's probably for current model
                        var trimmedStart = before.GetText().TrimStart(' ').Substring(0, 5);
                        var nIndex = trimmedStart.IndexOf("\n", StringComparison.Ordinal);
                        if (nIndex >= 0 || before.Location.StartPos == 0)
                        {
                            results.Add(before);
                            comments.AddComments(before);
                        }
                    }

                    if (after != null)
                    {
                        var trimmedStart = after.GetText().TrimStart(' ').Substring(0, 5);
                        var nIndex = trimmedStart.IndexOf("\n", StringComparison.Ordinal);

                        // this comment is on the same line. it's probably for current model
                        if (nIndex < 0)
                        {
                            results.Add(after);
                            comments.AddComments(after);
                        }
                    }
                }
            }

            return results;
        }

        private static void ParseComments([NotNull] TransformationContext context, [NotNull] ModelContainer container)
        {
            Logger.Info("parsing comments");

            var sources = context.Comments.GetSourcesWithComments();
            var notFoundCounter = 0;
            foreach (var sourceSnapshot in sources)
            {
                var models = container.GetAll<Model>().Where(model =>
                    model.Comments.FullPath == sourceSnapshot.File.FullName
                    && model.ModelType != ModelType.Module);
                var comments = context.Comments.GetAllComments(sourceSnapshot).ToList();

                var results = new List<CommentBlock>();
                results.AddRange(models.SelectMany(model => model.FindCommentsForMembers(context.Comments)));

                Logger.Trace("finding parent for orphaned comments");
                var orphanedComments = comments
                    .Where(comment => results.All(found => comment.Location != found.Location))
                    .Select(comment => new CommentWrapper(comment))
                    .ToArray();

                Logger.Info($"total orphaned: {orphanedComments.Length} in {sourceSnapshot.File.FullName}");
                var modules = container.GetAll<ModuleModel>().Where(model =>
                    model.Comments.FullPath == sourceSnapshot.File.FullName)
                    .ToArray();
                foreach (var notFoundComment in orphanedComments)
                {
                    var targetModule =
                        modules.FirstOrDefault(module => module.Name == notFoundComment.Comment.Location.Source.File.FullName);
                    if (targetModule == null)
                    {
                        continue;
                    }

                    foreach (var member in targetModule.Items)
                    {
                        if (member.Comments.Location.HasValue
                            && notFoundComment.Comment.Location.IntersectsWith(member.Comments.Location.Value))
                        {
                            member.Comments.AddComments(notFoundComment.Comment);
                            notFoundComment.WasFound = true;
                            break;
                        }
                    }
                }

                var notFoundComments = orphanedComments.Where(comm => !comm.WasFound).ToArray();
                Logger.Trace($"total not found: {notFoundComments.Length}");
                notFoundCounter += notFoundComments.Length;
                foreach (var notFoundComment in notFoundComments)
                {
                    Logger.Trace(notFoundComment.Comment.Location);
                }
            }

            Logger.Trace($"total not found: {notFoundCounter}");

            Logger.Info($"finished parsing comments");
        }

        private static void CreateModels([NotNull] NamespaceSymbol root, [NotNull] ModelContainer container)
        {
            foreach (var symbol in root.MemberTable.AllSymbols)
            {
                switch (symbol)
                {
                    case ArraySymbol array:
                        break;
                    case BarrierSymbol barrier:
                        container.Add(
                            new BarrierModel(barrier));
                        break;
                    case Deployment.SetSymbol deployment:
                        container.Add(
                            new DeploymentSetModel(deployment));
                        break;
                    case Deployment.ScriptSymbol deployment:
                        container.Add(
                            new DeploymentScriptModel(deployment));
                        break;
                    case Deployment.CurryingSymbol deployment:
                        container.Add(
                            new DeploymentCurryingModel(deployment));
                        break;
                    case DeploymentRebootSymbol deployment:
                        container.Add(
                            new DeploymentRebootModel(deployment));
                        break;
                    case Deployment.SelectSymbol deployment:
                        container.Add(
                            new DeploymentSelectModel(deployment));
                        break;
                    case ExternalSymbol external:
                        container.Add(external.GetExternalFields(container));
                        break;
                    case TypeAliasSymbol type:
                        break;
                    case NamespaceSymbol nameSpace:
                        break;
                    case Platform.DefSymbol platform:
                        container.Add(new PlatformModel(platform));
                        break;
                    case Platform.SetSymbol platform:
                        container.Add(new PlatformsSetModel(platform));
                        break;
                    case Product.DefSymbol product:
                        container.Add(new ProductModel(product));
                        break;
                    case Product.SetSymbol product:
                        container.Add(new ProductSetModel(product));
                        break;
                    case ProductTypeSymbol productType:
                        container.Add(new ProductTypeModel(productType));
                        break;
                    case Scenario.DefSymbol scenario:
                        container.Add(new ScenarioModel(scenario));
                        break;
                    case Scenario.SetSymbol scenario:
                        container.Add(new ScenarioSetModel(scenario));
                        break;
                    case SuiteTypeSymbol suiteTypeSymbol:
                        container.Add(new SuiteTypeModel(suiteTypeSymbol));
                        break;
                    case Suite.DefSymbol suite:
                        container.Add(new SuiteDefModel(suite));
                        break;
                    case ModuleSymbol module:
                        container.Add(new ModuleModel(module));
                        break;
                    case TypeSymbol type:
                        break;
                    default:
                        Logger.Warn(
                            $"Unknown symbol {symbol.Name}{Constants.TabulationSymbol}{symbol.Kind}{Constants.TabulationSymbol}{symbol.GetType()}");
                        break;
                }
            }
        }

        private sealed class CommentWrapper
        {
            public CommentWrapper([NotNull] CommentBlock comment)
            {
                Comment = comment;
                WasFound = false;
            }

            public bool WasFound { get; set; }

            [NotNull]
            public CommentBlock Comment { get; }
        }
    }
}
