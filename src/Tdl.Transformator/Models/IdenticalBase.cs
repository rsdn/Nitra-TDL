using JetBrains.Annotations;

namespace Tdl.Transformator.Models
{
    public abstract class IdenticalBase<TIdentic> : IIdentical<TIdentic>
    {
        public bool IsIdentical(TIdentic other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }            

            return IsIdentic(other);
        }

        protected abstract bool IsIdentic([NotNull] TIdentic other);
    }
} 