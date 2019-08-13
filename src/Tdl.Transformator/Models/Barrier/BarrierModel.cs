using System;
using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Services;
using Tdl;

namespace Tdl.Transformator.Models.Barrier
{
    public sealed class BarrierModel : Model
    {
        public BarrierModel([NotNull] BarrierSymbol symbol)
            : base(symbol, ModelType.Barrier)
        {
            Timeout = TimeSpan.Parse(symbol.Timeout.Value);
        }

        public TimeSpan Timeout { get; set; }

        public override bool HasBackReference => false;

        public override string Print()
        {
            var stringBuilder = new StringBuilder();
            Comments.AppendComments(stringBuilder);
            stringBuilder.Append($"barrier {Name} timeout \"{Timeout:hh\\:mm\\:ss}\";");
            return stringBuilder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (BarrierModel)other;

            return Timeout.Equals(another.Timeout);
        }
    }
} 