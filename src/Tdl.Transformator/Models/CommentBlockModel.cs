using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Tdl2Json;

namespace Tdl.Transformator.Models
{
    public sealed class CommentBlockModel : ICloneable
    {
        public CommentBlockModel(Nitra.Location? symbolLocation)
        {
            Comments = new List<string>();
            Location = symbolLocation;
        }

        public CommentBlockModel()
            : this(null)
        {
        }

        [NotNull, ItemNotNull]
        public List<string> Comments { get; set; }

        public Nitra.Location? Location { get; }

        [CanBeNull]
        public string FullPath => Location?.Source.File.FullName;

        public void AddComments([NotNull] CommentBlock commentBlock)
        {
            foreach (var comment in commentBlock.Comments)
            {
                var text = comment.GetText();
                if (!text.Contains("method guid:"))
                {
                    var trimmedText = text.Trim(' ', '\r', '\n', '\t');
                    Comments.Add(trimmedText);
                }
            }
        }

        public void AppendComments([NotNull]StringBuilder builder)
        {
            foreach (var comment in Comments)
            {
                builder.AppendLine(comment);
            }
        }

        public object Clone()
        {
            var clone = new CommentBlockModel(Location);
            clone.Comments.AddRange(Comments);

            return clone;
        }
    }
}
