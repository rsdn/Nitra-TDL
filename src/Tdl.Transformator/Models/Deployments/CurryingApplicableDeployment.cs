using JetBrains.Annotations;
using KL.TdlTransformator.Models.TypedReference;
using Nitra.Declarations;

namespace KL.TdlTransformator.Models.Deployments
{
    public abstract class CurryingApplicableDeployment : DeploymentBaseModel
    {
        protected CurryingApplicableDeployment([NotNull] DeclarationSymbol symbol, ModelType modelType)
           : base(symbol, modelType)
        {
        }

        [NotNull]
        public BackReference<DeploymentCurryingModel> CurryingDeploymentRefs { get; private set; }

        public override bool IsAnyBackReferenceSet => base.IsAnyBackReferenceSet || CurryingDeploymentRefs.Any();

        [NotNull]
        protected virtual BackReference<DeploymentCurryingModel> CreateCyrryingRefs() => new BackReference<DeploymentCurryingModel>();

        protected override void CreateBackReferenceObjects()
        {
            base.CreateBackReferenceObjects();

            CurryingDeploymentRefs = CreateCyrryingRefs();
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (CurryingApplicableDeployment)model;

            clone.CurryingDeploymentRefs = (BackReference<DeploymentCurryingModel>)CurryingDeploymentRefs.Clone();

            return clone;
        }
    }
} 