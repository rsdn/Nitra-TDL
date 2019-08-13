using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Parameters;
using Tdl.Transformator.Services;
using Tdl;

namespace Tdl.Transformator.Models.Deployments
{
    public sealed class DeploymentCurryingModel : CurryingApplicableDeployment
    {
        private readonly int _baseDeploymentId;

        // ReSharper disable once NotNullMemberIsNotInitialized
        public DeploymentCurryingModel([NotNull]Deployment.CurryingSymbol symbol) 
            : base(symbol, ModelType.DeploymentCurrying)
        {
            var defModels = symbol.ParameterValues.Select(p => new DefModel(p, ModelType.Def));
            Parameters = new List<DefModel>(defModels);
            _baseDeploymentId = symbol.BaseDeployment.Id;
        }

        [NotNull]
        public CurryingApplicableDeployment BaseDeployment { get; set; }

        [NotNull, ItemNotNull]
        public List<DefModel> Parameters { get; set; }

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"deployment \"{Name}\" = \"{BaseDeployment.Name}\"(");
            if (Parameters.Count == 0)
            {
                builder.AppendLine($"{Constants.TabulationSymbol});");
            }
            else
            {
                for (var i = 0; i < Parameters.Count; i++)
                {
                    builder.Append($"{Constants.TabulationSymbol}{Parameters[i].Print()}");
                    if (i != Parameters.Count - 1)
                    {
                        builder.AppendLine(",");
                    }
                }

                builder.Append(");");
            }

            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
            BaseDeployment = modelContainer.Get<CurryingApplicableDeployment>(_baseDeploymentId); 
            BaseDeployment.CurryingDeploymentRefs.Add(this);
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (DeploymentCurryingModel)other;

            return BaseDeployment.IsIdentical(another.BaseDeployment)
                   && Parameters.IsIdentical(another.Parameters);
        }

        protected override object Clone([NotNull] object model)
        {
            model = base.Clone(model);

            var clone = (DeploymentCurryingModel)model;

            clone.Parameters = Parameters.Select(p => (DefModel)p.Clone()).ToList();

            return clone;
        }
    }
} 