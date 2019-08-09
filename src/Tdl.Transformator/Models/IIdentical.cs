using JetBrains.Annotations;

namespace KL.TdlTransformator.Models
{
    public interface IIdentical<TIdentic>
    {
        bool IsIdentical([CanBeNull] TIdentic other);
    }
} 