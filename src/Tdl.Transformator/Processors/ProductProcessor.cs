using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Products;
using NLog;

namespace Tdl.Transformator.Processors
{
    public sealed class ProductProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [ItemNotNull]
        [NotNull]
        public IEnumerable<ProductBaseModel> MergeProducts(
            [ItemNotNull, NotNull] IEnumerable<ProductBaseModel> products,
            [ItemNotNull, NotNull] IEnumerable<ProductSetModel> productsSets)
        {
            var result = new List<ProductBaseModel>();

            var localProductsCopy = UnwrapProductSets(products).ToList();

            foreach (var possibleSet in productsSets.OrderByDescending(set => set.Members.Count))
            {
                if (possibleSet.Members.Count > localProductsCopy.Count())
                {
                    continue;
                }

                if (possibleSet.Members.All(member => localProductsCopy.Any(product => product.IsIdentical(member.Model))))
                {
                    Logger.Trace($"replacing {string.Join(", ", possibleSet.Members.Select(member => member.Model.Name))} to {possibleSet.Name}");
                    localProductsCopy.RemoveAll(item =>
                        possibleSet.Members.Any(member => member.Model.IsIdentical(item)));
                    result.Add(possibleSet);
                }

                if (!localProductsCopy.Any())
                {
                    break;
                }
            }

            result.AddRange(localProductsCopy);

            return result;
        }

        [ItemNotNull]
        [NotNull]
        private IEnumerable<ProductBaseModel> UnwrapProductSets(
            [ItemNotNull, NotNull] IEnumerable<ProductBaseModel> products)
        {
            var localProductsCopy = new List<ProductBaseModel>();
            foreach (var product in products)
            {
                switch (product)
                {
                    case ProductModel productModel:
                        localProductsCopy.Add(productModel);
                        break;
                    case ProductSetModel productSet:
                        if (productSet.Members.Any(member => member.Model.Name.Contains("_Main")))
                        {
                            localProductsCopy.Add(productSet);
                        }
                        else
                        {
                            localProductsCopy.AddRange(UnwrapProductSets(productSet.Members.Select(member => member.Model)));
                        }

                        break;
                }
            }

            return localProductsCopy;
        }
    }
}
