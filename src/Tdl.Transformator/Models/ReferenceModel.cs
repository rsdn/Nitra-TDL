using System;
using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Services;
using Nitra;

namespace Tdl.Transformator.Models
{
    public sealed class ReferenceModel<TRefereedModel> : IdenticalBase<ReferenceModel<TRefereedModel>>, ICloneable
        where TRefereedModel : Model
    {
        private readonly int _modelId;

        // deferred initialization
        // ReSharper disable once NotNullMemberIsNotInitialized
        public ReferenceModel(int id)
        {
            _modelId = id;
            Comments = new CommentBlockModel();
        }

        // deferred initialization
        // ReSharper disable once NotNullMemberIsNotInitialized
        public ReferenceModel(int id, Location referenceLocation)
        {
            _modelId = id;
            Comments = new CommentBlockModel(referenceLocation);
        }

        public ReferenceModel([NotNull] TRefereedModel referred)
        {
            _modelId = referred.Id;
            Model = referred;
            Comments = new CommentBlockModel();
        } 

        [NotNull]
        public TRefereedModel Model { get; private set; }

        [NotNull]
        public CommentBlockModel Comments { get; private set; }

        [NotNull]
        public string Print()
        {
            return Print(PrintParameters.Default);
        }

        [NotNull]
        public string Print(PrintParameters modifier)
        {
            var builder = new StringBuilder();
            var startTab = modifier.HasFlag(PrintParameters.NoStartTab)
                ? string.Empty
                : Constants.TabulationSymbol;

            if (!modifier.HasFlag(PrintParameters.NoComments))
            {
                Comments.AppendComments(builder);
            }

            builder.Append(modifier.HasFlag(PrintParameters.AddQuotes)
                ? $"{startTab}\"{Model.Name}\""
                : $"{startTab}{Model.Name}");

            return builder.ToString();
        }

        public void Init([NotNull] ModelContainer modelContainer)
        {
            Model = modelContainer.Get<TRefereedModel>(_modelId);
        }

        public object Clone()
        {
            var clone = (ReferenceModel<TRefereedModel>)MemberwiseClone();

            clone.Comments = (CommentBlockModel)Comments.Clone();

            return clone;
        }

        protected override bool IsIdentic(ReferenceModel<TRefereedModel> other)
        {
            if (other.GetType() != GetType())
            {
                return false;
            }

            return Model.IsIdentical(other.Model);
        }        
    }
} 