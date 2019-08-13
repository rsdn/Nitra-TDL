using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Services;
using Tdl;

namespace Tdl.Transformator.Models.Platforms
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