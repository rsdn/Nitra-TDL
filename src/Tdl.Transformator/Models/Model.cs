using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Modules;
using Tdl.Transformator.Models.TypedReference;
using Tdl.Transformator.Services;
using Nitra.Declarations;
using NLog;
using Tdl2Json;

namespace Tdl.Transformator.Models
{
    public abstract class Model : IdenticalBase<Model>, ICloneable, IBackReferenceInfoProvider
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected Model([NotNull] DeclarationSymbol symbol, ModelType modelType)
            : this(
                modelType,
                symbol.Id,
                symbol.FirstDeclarationOrDefault.Location,
                symbol.Name,
                symbol.FirstDeclarationOrDefault.Location.Source.File.FullName)
        {
        }

        protected Model(
            ModelType modelType,
            int id,
            Nitra.Location? location,
            [NotNull] string name,
            [NotNull] string path)
        {
            ModelType = modelType;
            Id = id;
            Comments = new CommentBlockModel(location);
            Name = name;
            FilePath = path;

            CreateBackReferenceObjects();
        }

        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        public ModelType ModelType { get; }

        [CanBeNull]
        public ModuleModel Module { get; set; }

        [NotNull]
        public string FilePath { get; set; }

        [NotNull]
        public CommentBlockModel Comments { get; set; }

        public abstract bool HasBackReference { get; }

        public virtual bool IsAnyBackReferenceSet => HasBackReference && IsAnyBackReferenceSetCore;

        [NotNull]
        public abstract string Print();

        public abstract void Init([NotNull]ModelContainer modelContainer);

        [NotNull, ItemNotNull]
        public virtual IEnumerable<CommentBlock> FindCommentsForMembers([NotNull]CommentBag commentBag)
        {
            return SymbolConverter.SearchComments(commentBag, Comments);
        }

        public object Clone()
        {
            var clone = MemberwiseClone();

            return Clone(clone);
        }

        protected virtual bool IsAnyBackReferenceSetCore => false;

        [NotNull]
        protected virtual object Clone([NotNull] object model) => model;

        protected virtual void CreateBackReferenceObjects()
        {
            return;
        }

        protected override bool IsIdentic(Model other)
        {
            if (Id == other.Id)
            {
                return true;
            }

            if (other.GetType() != GetType())
            {
                return false;
            }

            return IsIdenticCore(other);
        }

        protected abstract bool IsIdenticCore([NotNull] Model model);
    }
} 