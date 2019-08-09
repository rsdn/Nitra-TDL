using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Services;
using Tdl;

namespace KL.TdlTransformator.Models.Platforms
{
    public sealed class PlatformModel : PlatformBase
    {
        public PlatformModel([NotNull]Platform.DefSymbol symbol)
            : base(symbol, ModelType.Platform)
        {
            Name = symbol.Name;
        }

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.Append($"platform {Name};");
            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        protected override bool IsIdenticCore(Model other)
        {
            other = (PlatformModel)other;

            return other.Name == Name;
        }
    }
} 