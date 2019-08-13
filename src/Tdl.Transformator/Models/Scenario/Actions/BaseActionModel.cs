using JetBrains.Annotations;
using Nitra;

namespace Tdl.Transformator.Models.Scenario.Actions
{
    public abstract class BaseActionModel : IdenticalBase<BaseActionModel>
    {
        protected BaseActionModel(Location location)
        {
            Comments = new CommentBlockModel(location);
        }

        [NotNull]
        public CommentBlockModel Comments { get; }

        [NotNull]
        public abstract string Print();
    }
}
