using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Services;
using Tdl;

namespace KL.TdlTransformator.Models.Deployments
{
    public sealed class DeploymentSetModel : DeploymentBaseModel
    {
        private bool _isReboot; // Reboot in TDL is syntax sugar and it is implemented as a deployment set

        public DeploymentSetModel([NotNull] Deployment.SetSymbol symbol)
            : base(symbol, ModelType.DeploymentSet)
        {
            Members = ((Deployment.Set)symbol.FirstDeclarationOrDefault).Deployments.Ref
                .Select(reference =>
                    new ReferenceModel<DeploymentBaseModel>(reference.Symbol.Id, reference.Location))
                .ToList();
        }

        [NotNull, ItemNotNull]
        public List<ReferenceModel<DeploymentBaseModel>> Members { get; set; }

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);

            if (_isReboot)
            {
                var rebootTimeout = ((DeploymentRebootModel)Members[0].Model).RebootTimeout;
                builder.Append($"deployment \"{Name}\" = reboot \"{rebootTimeout}\";");
            }
            else
            {
                builder.AppendLine($"deployment \"{Name}\" = ");
                for (var i = 0; i < Members.Count; i++)
                {
                    builder.Append($"{Members[i].Print(PrintParameters.AddQuotes)}");
                    if (i != Members.Count - 1)
                    {
                        builder.AppendLine(",");
                    }
                }

                builder.Append(";");
            }

            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
            foreach (var member in Members)
            {
                member.Init(modelContainer);
                member.Model.DeploymentSetRefs.Add(this);
            }

            if (Members.Count == 1
                && Members.First().Model.ModelType == ModelType.DeploymentReboot)
            {
                _isReboot = true;
            }
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (DeploymentSetModel)other;

            return Members.IsIdentical(another.Members);
        }

        protected override object Clone([NotNull] object model)
        {
            model = base.Clone(model);

            var clone = (DeploymentSetModel)model;

            clone.Members = Members.ToList();

            return clone;
        }
    }
} 