using System;
using JetBrains.Annotations;
using Nitra;

namespace Tdl.Transformator.Models.Scenario.Actions
{
    public sealed class WaitForRebootActionModel : BaseActionModel
    {
        public WaitForRebootActionModel(
            [NotNull] Tdl.ScenarioAction.WaitForReboot waitForReboot, Location location) 
            : base(location)
        {
            Timeout =
                waitForReboot.TimeSpan == string.Empty
                ? TimeSpan.Zero
                : TimeSpan.Parse(waitForReboot.TimeSpan);
        }

        public TimeSpan Timeout { get; set; }

        public override string Print()
        {
            return Timeout == TimeSpan.Zero
                ? "wait for reboot;"
                : $"wait for reboot {Timeout:hh\\:mm\\:ss};";
        }

        protected override bool IsIdentic(BaseActionModel other)
        {
            return other is WaitForRebootActionModel model
                   && Timeout.Equals(model.Timeout);
        }
    }
} 