using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Services;
using Tdl;

namespace KL.TdlTransformator.Models.Parameters
{
    public sealed class FieldModel : Model
    {
        public FieldModel([NotNull]FieldSymbol symbol) 
            : base(symbol, ModelType.Field)
        {
            Type = symbol.Type.Name;
        }

        [NotNull]
        public string Type { get; set; }

        public override bool HasBackReference => false;

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.Append($"{Type} {Name}");
            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (FieldModel)other;

            return Name == another.Name
                   && Type == another.Type;
        }
    }
} 