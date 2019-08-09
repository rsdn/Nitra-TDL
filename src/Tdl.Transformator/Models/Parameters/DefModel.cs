using JetBrains.Annotations;
using KL.TdlTransformator.Models.Expressions;
using KL.TdlTransformator.Services;
using Tdl;

namespace KL.TdlTransformator.Models.Parameters
{
    public sealed class DefModel : Model
    {
        public DefModel(Def def, ModelType modelType) 
            : base(def.Symbol, modelType)
        {
            Expression = def.Expr.ToExpression();
        }

        [CanBeNull]
        public Expression Expression { get; set; }

        public override bool HasBackReference => false;

        public override string Print()
        {
            return $"{Name}: {Expression}";
        }

        public override void Init(ModelContainer modelContainer)
        {
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (DefModel)other;

            return Name == another.Name
                   && Expression == another.Expression;
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (DefModel)model;
            clone.Expression = (Expression)Expression.Clone();

            return clone;
        }
    }
} 