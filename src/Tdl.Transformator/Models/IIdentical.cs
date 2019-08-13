using JetBrains.Annotations;

namespace Tdl.Transformator.Models
{
    public interface IIdentical<TIdentic>
    {
        bool IsIdentical([CanBeNull] TIdentic other);
    }
} 