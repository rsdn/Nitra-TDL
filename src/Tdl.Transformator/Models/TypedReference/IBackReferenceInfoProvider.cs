using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KL.TdlTransformator.Models.TypedReference
{
    public interface IBackReferenceInfoProvider
    {
        bool HasBackReference { get; }

        bool IsAnyBackReferenceSet { get; }
    }
}
