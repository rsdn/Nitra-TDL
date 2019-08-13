using JetBrains.Annotations;
using Tdl.Transformator.Models;
using Tdl.Transformator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tdl;
using Tdl2Json;

namespace Tdl.Transformator.Models.Deployments
{
    public abstract class ScriptReference : IIdentical<ScriptReference>, ICloneable
    {
        public static ScriptReference Create([NotNull] Deployment.ScriptSymbol symbol)
        {
            switch (symbol.ScriptReference)
            {
                case Tdl.ScriptReference.FilePath   x:
                    return new ScriptReference.FilePath(new ValueModel(symbol, x.Path));
                case Tdl.ScriptReference.SourceCode x:
                    return new ScriptReference.SourceCode(new ValueModel(symbol, x.Text), new ValueModel(symbol, x.Extension));
                case Tdl.ScriptReference.EmbedFile  x:
                    return new ScriptReference.EmbedFile(new ValueModel(symbol, x.Path));
                default:
                    throw new ArgumentException($"Unsupported type of ScriptReference '{symbol.ScriptReference.GetType().FullName}'.",
                        nameof(symbol));
            }
        }

        public abstract object Clone();
        public abstract bool IsIdentical([CanBeNull] ScriptReference other);
        public abstract string Print();
        public abstract IEnumerable<CommentBlock> FindCommentsForMembers(CommentBag commentBag);

        public sealed class FilePath : ScriptReference
        {
            ValueModel Path { get; set; }

            public FilePath(ValueModel path)
            {
                Path = path ?? throw new ArgumentNullException(nameof(path));
            }

            public override string Print() => Path.Print();

            public override object Clone()
            {
                throw new NotImplementedException();
            }

            public override bool IsIdentical([CanBeNull] ScriptReference other) =>
                other is FilePath otherFilePath && Path.IsIdentical(otherFilePath.Path);

            public override IEnumerable<CommentBlock> FindCommentsForMembers(CommentBag commentBag) =>
                SymbolConverter.SearchComments(commentBag, Path.Comments);
        } // class FilePath

        public sealed class SourceCode : ScriptReference
        {
            ValueModel Data { get; set; }
            ValueModel Extension { get; set; }

            public SourceCode(ValueModel data, ValueModel extension)
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                Extension = extension ?? throw new ArgumentNullException(nameof(extension));
            }

            public override string Print()
            {
                throw new NotImplementedException();
            }

            public override object Clone()
            {
                throw new NotImplementedException();
            }

            public override bool IsIdentical([CanBeNull] ScriptReference other) =>
                other is SourceCode otherSourceCode
                    && Data.IsIdentical(otherSourceCode.Data)
                    && Extension.IsIdentical(otherSourceCode.Extension);

            public override IEnumerable<CommentBlock> FindCommentsForMembers(CommentBag commentBag) =>
                SymbolConverter.SearchComments(commentBag, Data.Comments)
                    .Concat(SymbolConverter.SearchComments(commentBag, Extension.Comments));
        } // class SourceCode

        public sealed class EmbedFile : ScriptReference
        {
            ValueModel Path { get; set; }

            public EmbedFile(ValueModel path)
            {
                Path = path ?? throw new ArgumentNullException(nameof(path));
            }

            public override string Print()
            {
                throw new NotImplementedException();
            }

            public override object Clone()
            {
                throw new NotImplementedException();
            }

            public override bool IsIdentical([CanBeNull] ScriptReference other) =>
                other is EmbedFile otherEmbedFile && Path.IsIdentical(otherEmbedFile.Path);

            public override IEnumerable<CommentBlock> FindCommentsForMembers(CommentBag commentBag) =>
                SymbolConverter.SearchComments(commentBag, Path.Comments);
        } // class EmbedFile
    } // class ScriptReference
} // namespace
