using JetBrains.Annotations;
using Nitra;

namespace KL.TdlTransformator.Models.Scenario.Actions
{
    public sealed class MethodActionModel : BaseActionModel
    {
        public MethodActionModel([NotNull] Tdl.ScenarioAction.Method method, Location location) 
            : base(location)
        {
            FullName = method.MethodSymbol.FullName;
            MaxReboots = method.MaxRebootsCountOpt.Value;
            Method = method;
        }

        [NotNull]
        public string FullName { get; set; }

        public int MaxReboots { get; set; }

        [NotNull]
        public Tdl.ScenarioAction.Method Method { get; }

        public override string Print()
        {
            if (MaxReboots > 0)
            {
                return $"method {FullName} max-reboots {MaxReboots};";
            }

            return $"method {FullName};";
        }

        protected override bool IsIdentic(BaseActionModel other)
        {
            return other is MethodActionModel model
                   && string.Equals(FullName, model.FullName);
        }
    }
}
