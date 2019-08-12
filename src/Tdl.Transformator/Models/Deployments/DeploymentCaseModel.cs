using JetBrains.Annotations;
using KL.TdlTransformator.Services;
using System;
using System.Linq;
using System.Collections.Immutable;
using Tdl;
using System.Collections.Generic;

namespace KL.TdlTransformator.Models.Deployments
{
    public sealed class DeploymentCaseModel : IdenticalBase<DeploymentCaseModel>
    {
        private readonly ImmutableArray<int> _deploymentIds;

        // ReSharper disable once NotNullMemberIsNotInitialized
        public DeploymentCaseModel([NotNull] string deploymentCase, ImmutableArray<DeploymentSymbol> deploymentSymbols)
        {
            DeploymentCase = deploymentCase;
            _deploymentIds = deploymentSymbols.Select(x => x.Id).ToImmutableArray();
        }

        [NotNull]
        public IEnumerable<DeploymentBaseModel> Deployments { get; set; }

        [NotNull]
        public string DeploymentCase { get; set; }

        [NotNull]
        public string Print()
        {
            var caseDescription = string.IsNullOrWhiteSpace(DeploymentCase)
                    ? "_" 
                    : $"\"{DeploymentCase}\"";
            return $"{caseDescription} => \"{string.Join("; ", Deployments.Select(x => x.Name))}\"";
        }

        public void Init([NotNull] ModelContainer modelContainer)
        {
            Deployments = _deploymentIds.Select(x => modelContainer.Get<DeploymentBaseModel>(x)).ToImmutableArray();
        }

        protected override bool IsIdentic(DeploymentCaseModel other)
        {
            if (other == this)
            {
                return true;
            }

            return string.Equals(DeploymentCase, other.DeploymentCase)
                   && Deployments.All2(other.Deployments, (x, y) => x.IsIdentical(y));
        }
    }
} 
