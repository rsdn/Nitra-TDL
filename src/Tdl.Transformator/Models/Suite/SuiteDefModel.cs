using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Models.Parameters;
using KL.TdlTransformator.Models.Platforms;
using KL.TdlTransformator.Models.Products;
using KL.TdlTransformator.Models.Scenario;
using KL.TdlTransformator.Services;
using Tdl;

namespace KL.TdlTransformator.Models.Suite
{
    public sealed class SuiteDefModel : SuiteModel
    {
        private readonly int _platformId;
        private readonly int _productId;
        private readonly int? _suiteTypeId;
        private readonly int[] _statementsIds;

        // ReSharper disable once NotNullMemberIsNotInitialized
        public SuiteDefModel([NotNull] Tdl.Suite.DefSymbol symbol)
            : base(symbol, ModelType.Suite)
        {
            _platformId = symbol.Platform.Id;
            _productId = symbol.Product.Id;

            Definitions = DefinitionGenerator.GetDefinitions(symbol.Defs,
                ((Tdl.Suite.Def)symbol.FirstDeclarationOrDefault).Definitions);

            if (symbol.Type.HasDeclarations)
            {
                _suiteTypeId = symbol.Type.Id;
            }

            Statements = new List<ScenarioBaseModel>();

            _statementsIds = symbol.Statements
                .OfType<SuiteStatement.CallScenario>()
                .Select(statement => statement.Reference.Id)
                .ToArray();
        } 

        [NotNull]
        public PlatformBase Platform { get; set; }

        [NotNull]
        public ProductBaseModel Product { get; set; }

        [CanBeNull]
        public SuiteTypeModel SuiteType { get; set; }

        [NotNull, ItemNotNull]
        public List<DefinitionModel> Definitions { get; set; }

        [NotNull, ItemNotNull]
        public List<ScenarioBaseModel> Statements { get; set; }

        public override bool HasBackReference => false;

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine(SuiteType != null
                ? $"suite \"{Name}\" : {SuiteType.Name}"
                : $"suite \"{Name}\"");

            builder.AppendLine("{");
            builder.AppendLine($"{Constants.TabulationSymbol}platform {Platform.Name};");
            builder.AppendLine($"{Constants.TabulationSymbol}product {Product.Name};");
            foreach (var def2 in Definitions)
            {
                builder.AppendLine($"{Constants.TabulationSymbol}{def2.Print()};");
            }

            foreach (var statement in Statements)
            {
                builder.AppendLine($"{Constants.TabulationSymbol}{statement.Name}();");
            }

            builder.Append("}");

            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
            if (_suiteTypeId.HasValue)
            {
                SuiteType = modelContainer.Get<SuiteTypeModel>(_suiteTypeId.Value);
                SuiteType.SuiteRefs.Add(this);
            }

            Platform = modelContainer.Get<PlatformBase>(_platformId);
            Platform.SuiteRefs.Add(this);
            Product = modelContainer.Get<ProductBaseModel>(_productId);
            Product.SuiteRefs.Add(this);

            var scenarioBaseModels = _statementsIds.Select(modelContainer.Get<ScenarioBaseModel>);
            Statements.AddRange(scenarioBaseModels);
            foreach (var scenario in scenarioBaseModels)
            {
                scenario.SuiteRefs.Add(this);
            }
        }

        protected override bool IsIdenticCore(Model model)
        {
            var other = (SuiteDefModel)model;

            bool suiteTypeCorrespond = true;
            if (SuiteType != null)
            {
                suiteTypeCorrespond = SuiteType.IsIdentical(other.SuiteType);
            }

            return Platform.IsIdentical(other.Platform)
                   && Product.IsIdentical(other.Product)
                   && Definitions.SequenceEqual(other.Definitions)
                   && Statements.IsIdentical(other.Statements)
                   && suiteTypeCorrespond;
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (SuiteDefModel)model;

            clone.Definitions = Definitions.Select(d => (DefinitionModel)d.Clone()).ToList();
            clone.Statements = Statements.ToList();

            return clone;
        }
    }
}
