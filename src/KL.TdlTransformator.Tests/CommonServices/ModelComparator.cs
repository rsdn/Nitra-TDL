using JetBrains.Annotations;
using KL.TdlTransformator.Models;

namespace KL.TdlTransformator.Tests.CommonServices
{
    public static class ModelComparator
    {
        public static bool Compare<TIdentic>(
            [CanBeNull] IIdentical<TIdentic> model, 
            [CanBeNull] TIdentic other)
        {
            if (model == null && other == null)
            {
                return true;
            }

            if (model == null)
            {
                return false;
            }

            return model.IsIdentical(other);
        }
    }
} 