using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Products;
using Tdl.Transformator.Models.Scenario;
using NLog;

namespace Tdl.Transformator.Processors
{
    public sealed class EnvironmentProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [ItemNotNull]
        [NotNull]
        public IEnumerable<ScenarioEnvironmentModel> ConcatenateEnvironments(
            [ItemNotNull, NotNull] IEnumerable<ScenarioEnvironmentModel> environments)
        {
            var result = new List<ScenarioEnvironmentModel>();

            foreach (var scenarioEnvironmentModel in environments)
            {
                if (result.Any(item => item.IsIdentical(scenarioEnvironmentModel)))
                {
                    continue;
                }

                result.Add(scenarioEnvironmentModel);
            }

            return result;
        }

        [ItemNotNull]
        [NotNull]
        public IEnumerable<ScenarioEnvironmentModel> MergeEnvironments(
            [ItemNotNull, NotNull] IEnumerable<ScenarioEnvironmentModel> environments,
            [ItemNotNull, NotNull] IEnumerable<ProductSetModel> productsSets)
        {
            var result = new List<ScenarioEnvironmentModel>();

            var groupings = environments.OrderBy(env => env.Platform.Name).GroupBy(env => env.Platform.Id);

            var productProcessor = new ProductProcessor();
            foreach (var grouping in groupings)
            {
                var products = grouping.Select(environmentModel => environmentModel.Product);
                Logger.Trace($"checking platform {grouping.First().Platform.Name} products: {string.Join(", ", products.Select(prod => prod.Name))}");
                var mergedProducts = productProcessor.MergeProducts(products, productsSets);
                var mergedEnvironments = mergedProducts.Select(product => new ScenarioEnvironmentModel
                {
                    Platform = grouping.First().Platform,
                    Product = product
                });
                result.AddRange(mergedEnvironments);
            }

            return result;
        }
    }
}