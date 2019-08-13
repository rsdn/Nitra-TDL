using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using System;

namespace Tdl.Transformator.Models.TypedReference
{
    public class BackReference<TReference> : ICloneable
    {
        [NotNull, ItemNotNull]
        public IList<TReference> References => BackRefs.ToList();

        public bool Any() => BackRefs.Count > 0;

        public virtual void Add([NotNull] TReference reference) => BackRefs.Add(reference);

        public virtual void AddRange([NotNull, ItemNotNull] IEnumerable<TReference> references) => BackRefs.AddRange(references);

        public virtual void Remove([NotNull] TReference reference) => BackRefs.Remove(reference);

        [NotNull]
        public object Clone()
        {
            var clone = new BackReference<TReference>();
            clone.AddRange(BackRefs);

            return clone;
        }

        [NotNull, ItemNotNull]
        protected List<TReference> BackRefs { get; set; } = new List<TReference>();
    }
}