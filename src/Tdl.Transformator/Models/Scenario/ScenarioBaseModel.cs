using JetBrains.Annotations;
using KL.TdlTransformator.Models.Suite;
using KL.TdlTransformator.Models.TypedReference;
using Nitra.Declarations;

namespace KL.TdlTransformator.Models.Scenario
{
    public abstract class ScenarioBaseModel : Model
    {
        protected ScenarioBaseModel([NotNull]DeclarationSymbol symbol, ModelType modelType)
            : base(symbol, modelType)
        {
        }

        protected ScenarioBaseModel(ModelType modelType, int id, Nitra.Location? location, [NotNull] string name, [NotNull] string path)
            : base(modelType, id, location, name, path)
        {
        }

        [NotNull]
        public BackReference<SuiteDefModel> SuiteRefs { get; private set; }

        [NotNull]
        public BackReference<ScenarioSetModel> ScenarioSetRefs { get; private set; }

        public override bool HasBackReference => true; 

        protected override bool IsAnyBackReferenceSetCore => SuiteRefs.Any() || ScenarioSetRefs.Any();

        [NotNull]
        protected virtual BackReference<SuiteDefModel> CreateSuitesRefs() => new BackReference<SuiteDefModel>();

        [NotNull]
        protected virtual BackReference<ScenarioSetModel> CreateScenarioSetRefs() => new BackReference<ScenarioSetModel>();

        protected override void CreateBackReferenceObjects()
        {
            SuiteRefs = CreateSuitesRefs();
            ScenarioSetRefs = CreateScenarioSetRefs();
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (ScenarioBaseModel)model;

            clone.SuiteRefs = (BackReference<SuiteDefModel>)SuiteRefs.Clone();
            clone.ScenarioSetRefs = (BackReference<ScenarioSetModel>)ScenarioSetRefs.Clone();

            return clone;
        }
    }
} 