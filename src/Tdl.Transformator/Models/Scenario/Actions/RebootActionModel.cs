using JetBrains.Annotations;
using Nitra;

namespace Tdl.Transformator.Models.Scenario.Actions
{
    public sealed class RebootActionModel : BaseActionModel
    {
        public RebootActionModel([NotNull] Tdl.ScenarioAction.Reboot reboot, Location location) 
            : base(location)
        {
        }

        public override string Print()
        {
            return "reboot;";
        }

        protected override bool IsIdentic(BaseActionModel other)
        {
            return other is RebootActionModel;
        }
    }
} 