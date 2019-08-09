using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Services;
using Tdl;

namespace KL.TdlTransformator.Models.Deployments
{
    public sealed class DeploymentRebootModel : DeploymentBaseModel
    {
        public DeploymentRebootModel([NotNull]DeploymentRebootSymbol symbol)
            : base(symbol, ModelType.DeploymentReboot)
        {
            RebootTimeout = symbol.Timeout.Value;
        }

        [NotNull]
        public string RebootTimeout { get; set; }

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"deployment \"{Name}\" = reboot \"{RebootTimeout}\";");
            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (DeploymentRebootModel)other;

            return RebootTimeout.Equals(another.RebootTimeout);
        } 
    }
} 