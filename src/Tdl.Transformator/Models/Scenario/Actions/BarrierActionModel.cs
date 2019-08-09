using JetBrains.Annotations;
using KL.TdlTransformator.Models.Barrier;
using KL.TdlTransformator.Services;
using Nitra;

namespace KL.TdlTransformator.Models.Scenario.Actions
{
    public sealed class BarrierActionModel : BaseActionModel
    {
        private readonly int _barrierId;

        // ReSharper disable once NotNullMemberIsNotInitialized
        public BarrierActionModel([NotNull] Tdl.ScenarioAction.Barrier barrier, Location location)
            : base(location)
        {
            _barrierId = barrier.barrier.Id;
        }

        [NotNull]
        public BarrierModel Barrier { get; set; }

        public override string Print()
        {
            return $"barrier {Barrier.Name};";
        }

        public void Init([NotNull] ModelContainer modelContainer)
        {
            Barrier = modelContainer.Get<BarrierModel>(_barrierId);
        }

        protected override bool IsIdentic(BaseActionModel other)
        {
            return other is BarrierActionModel model
                   && Barrier.IsIdentical(model.Barrier);
        }
    }
}