using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Services;
using Tdl;

namespace KL.TdlTransformator.Models.Deployments
{
    public sealed class DeploymentSelectModel : DeploymentBaseModel
    {
        public DeploymentSelectModel([NotNull] Deployment.SelectSymbol symbol) 
            : base(symbol, ModelType.DeploymentSelect)
        {
            ParameterNames = new List<string>();
            foreach (var formalParameterSymbol in symbol.Parameters)
            {
                ParameterNames.Add(formalParameterSymbol.FullName);
            }

            Cases = new List<DeploymentCaseModel>();

            for (var i = 0; i < symbol.Cases.Length; i++)
            {
                Cases.Add(new DeploymentCaseModel(symbol.Cases[i][0], symbol.Deployments[i]));
            }
        }

        [NotNull, ItemNotNull]
        public List<string> ParameterNames { get; set; }

        [NotNull, ItemNotNull]
        public List<DeploymentCaseModel> Cases { get; set; }

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"select deployment \"{Name}\"({string.Join(", ", ParameterNames)})");
            builder.AppendLine("{");
            foreach (var deploymentCase in Cases)
            {
                builder.AppendLine($"{Constants.TabulationSymbol}| {deploymentCase.Print()}");
            }

            builder.Append("}");

            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
            foreach (var deploymentCaseModel in Cases)
            {
                deploymentCaseModel.Init(modelContainer);
                deploymentCaseModel.Deployment.SelectDeploymentRefs.Add(this);
            }
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (DeploymentSelectModel)other;

            return Cases.IsIdentical(another.Cases);
        }

        protected override object Clone([NotNull] object model)
        {
            model = base.Clone(model);

            var clone = (DeploymentSelectModel)model;

            clone.Cases = Cases.ToList();
            clone.ParameterNames = ParameterNames.ToList();

            return clone;
        }
    }
} 