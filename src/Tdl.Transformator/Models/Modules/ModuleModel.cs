using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Models.Parameters;
using KL.TdlTransformator.Services;
using Nitra.Declarations;
using Tdl;

namespace KL.TdlTransformator.Models.Modules
{
    public sealed class ModuleModel : Model
    {
        private readonly int[] _itemsIds;

        public ModuleModel([NotNull] ModuleSymbol symbol)
            : base(symbol, ModelType.Module)
        {
            Path = symbol.FullName;
            Items = new List<Model>();

            var itemsIds = new List<int>();
            foreach (var element in symbol.Declarations.SelectMany(declaration => declaration.Members))
            {
                switch (element)
                {
                    case Declaration decl:
                        if (!(decl.Symbol is ExternalSymbol))
                            itemsIds.Add(decl.Symbol.Id);
                        break;
                    default:
                        Logger.Error($@"unknown element {element.GetType()}");
                        break;
                }
            }

            _itemsIds = itemsIds.ToArray();
        }

        [NotNull]
        public string Path { get; set; }

        [NotNull, ItemNotNull]
        public List<Model> Items { get; set; }

        public override bool HasBackReference => false;

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            var externalFields = Items.OfType<FieldModel>().ToArray();
            var otherItems = Items.Except(externalFields).ToArray();
            if (externalFields.Any())
            {
                builder.AppendLine("external");
                builder.AppendLine("{");
                foreach (var field in externalFields)
                {
                    builder.AppendLine($"{Constants.TabulationSymbol}{field.Print()};");
                }

                builder.Append("}");

                if (otherItems.Any())
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }
            }

            for (var i = 0; i < otherItems.Length; i++)
            {
                if (i != otherItems.Length - 1)
                {
                    // add line between items
                    builder.AppendLine($"{otherItems[i].Print()}");
                    builder.AppendLine();
                }
                else
                {
                    // last item: don't change line
                    builder.Append($"{otherItems[i].Print()}");
                }
            }

            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
            var items = _itemsIds.Select(modelContainer.Get<Model>);
            Items.AddRange(items);
            foreach (var model in items)
            {
                model.Module = this;
            }
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (ModuleModel)other;
            if (another.Items.Count != Items.Count)
            {
                return false;
            }

            var models = new Dictionary<string, Model>();
            foreach (var item in Items)
            {
                models.Add(item.Name + item.GetType().Name, item);
            }

            foreach (var otherModel in another.Items)
            {
                if (models.TryGetValue(otherModel.Name + otherModel.GetType().Name, out var originalModel)
                     && originalModel.IsIdentical(otherModel))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }

            return true;  
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (ModuleModel)model;

            clone.Items = Items.ToList();

            return clone;
        }
    }
}
