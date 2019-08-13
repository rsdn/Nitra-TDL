using JetBrains.Annotations;
using Tdl.Transformator.Models.Scenario;
using Tdl.Transformator.Models.TypedReference;
using Nitra.Declarations;

namespace Tdl.Transformator.Models.Deployments
{
    public abstract class DeploymentBaseModel : Model
    {
        protected DeploymentBaseModel([NotNull] DeclarationSymbol symbol, ModelType modelType) 
            : base(symbol, modelType)
        {
        }

        [NotNull]
        public BackReference<DeploymentSetModel> DeploymentSetRefs { get; private set; }

        [NotNull]
        public BackReference<DeploymentSelectModel> SelectDeploymentRefs { get; private set; }

        [NotNull]
        public BackReference<ScenarioBaseModel> ScenarioRefs { get; private set; }

        public override bool HasBackReference => true;

        public override bool IsAnyBackReferenceSet
            => DeploymentSetRefs.Any() || SelectDeploymentRefs.Any() || ScenarioRefs.Any();

        [NotNull]
        protected virtual BackReference<DeploymentSetModel> CreateDeploymentSetRefs() => new BackReference<DeploymentSetModel>();

        [NotNull]
        protected virtual BackReference<DeploymentSelectModel> CreateSelectDeploymentRefs() => new BackReference<DeploymentSelectModel>();

        [NotNull]
        protected virtual BackReference<ScenarioBaseModel> CreateScenarioRefs() => new BackReference<ScenarioBaseModel>();

        protected override void CreateBackReferenceObjects()
        {
            DeploymentSetRefs = CreateDeploymentSetRefs();
            SelectDeploymentRefs = CreateSelectDeploymentRefs();
            ScenarioRefs = CreateScenarioRefs();
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (DeploymentBaseModel)model;

            clone.DeploymentSetRefs = (BackReference<DeploymentSetModel>)DeploymentSetRefs.Clone();
            clone.SelectDeploymentRefs = (BackReference<DeploymentSelectModel>)SelectDeploymentRefs.Clone();
            clone.ScenarioRefs = (BackReference<ScenarioBaseModel>)ScenarioRefs.Clone();

            return clone;
        }
    }
} 