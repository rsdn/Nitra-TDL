using JetBrains.Annotations;
using KL.TdlTransformator.Services;
using Tdl;

namespace KL.TdlTransformator.Models.Deployments
{
    public sealed class DeploymentCaseModel : IdenticalBase<DeploymentCaseModel>
    {
        private readonly int _deploymentId;

        // ReSharper disable once NotNullMemberIsNotInitialized
        public DeploymentCaseModel([NotNull] string deploymentCase, [NotNull] DeploymentSymbol deploymentSymbol)
        {
            DeploymentCase = deploymentCase;
            _deploymentId = deploymentSymbol.Id;
        }

        [NotNull]
        public DeploymentBaseModel Deployment { get; set; }

        [NotNull]
        public string DeploymentCase { get; set; }

        [NotNull]
        public string Print()
        {
            var caseDescription = string.IsNullOrWhiteSpace(DeploymentCase)
                    ? "_" 
                    : $"\"{DeploymentCase}\"";
            return $"{caseDescription} => \"{Deployment.Name}\"";
        }

        public void Init([NotNull] ModelContainer modelContainer)
        {
            Deployment = modelContainer.Get<DeploymentBaseModel>(_deploymentId);
        }

        protected override bool IsIdentic(DeploymentCaseModel other)
        { 
            return string.Equals(DeploymentCase, other.DeploymentCase)
                   && Deployment.IsIdentical(other.Deployment);
        }
    }
} 