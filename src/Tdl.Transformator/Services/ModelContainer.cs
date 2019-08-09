using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using KL.TdlTransformator.Models;
using KL.TdlTransformator.Models.Modules;
using NLog;

namespace KL.TdlTransformator.Services
{
    public sealed class ModelContainer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<int, Model> _models;
        private readonly Dictionary<int, string> _initialSnapshot;

        public ModelContainer()
        {
            _models = new Dictionary<int, Model>();
            _initialSnapshot = new Dictionary<int, string>();
        }

        [NotNull]
        public TModel Get<TModel>(int id) where TModel : Model
        {
            if (!_models.ContainsKey(id))
            {
                throw new InvalidOperationException(
                    $"the id {id} is not found.");
            }

            var target = _models[id];

            switch (target)
            {
                case null:
                    throw new InvalidOperationException(
                        $"the value at id {id} is null.");
                case TModel model:
                    return model;
                default:
                    throw new InvalidCastException(
                        $"The item at index is not a {typeof(TModel)},"
                        + $" it is a {target.ModelType}");
            }
        }

        [NotNull, ItemNotNull]
        public IEnumerable<ModuleModel> GetPrintable()
        {
            return _models.Values.OfType<ModuleModel>();
        }

        public void Add([ItemNotNull, NotNull] IEnumerable<Model> symbols)
        {
            foreach (var symbol in symbols)
            {
                Add(symbol);
            }
        }

        public void Add([NotNull] Model model)
        {
            if (_models.ContainsKey(model.Id))
            {
                throw new InvalidOperationException(
                    $"the value with id {model.Id} is already present.");
            }

            _models[model.Id] = model;
        }

        public void InitializeModels()
        {
            foreach (var model in _models)
            {
                model.Value.Init(this);
            }
        }

        public void CreateSnapshot()
        {
            foreach (var model in _models)
            {
                _initialSnapshot[model.Key] = model.Value.Print();
            }
        }

        [ItemNotNull, NotNull]
        public IEnumerable<TModel> GetAll<TModel>() where TModel : Model
        {
            return _models.Values.OrderBy(mod => mod.Id).OfType<TModel>();
        }

        [ItemNotNull, NotNull]
        public IEnumerable<TModel> GetAll<TModel>([NotNull]string pathMask) where TModel : Model
        {
            var regex = new Regex(pathMask, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach (var model in _models.Values.OrderBy(mod => mod.Id).OfType<TModel>())
            {
                var matches = regex.Match(model.FilePath);

                if (matches.Success)
                {
                    yield return model;
                }
            }
        }

        [ItemNotNull, NotNull]
        public IEnumerable<ModuleModel> GetChanged()
        {
            foreach (var model in _models.Values)
            {
                if (model is ModuleModel moduleModel)
                {
                    if (_initialSnapshot[moduleModel.Id] == null)
                    {
                        yield return moduleModel;
                    }

                    if (!string.Equals(moduleModel.Print(), _initialSnapshot[moduleModel.Id]))
                    {
                        yield return moduleModel;
                    }
                }
            }
        }

        public void Remove(int id)
        {
            if (!_models.ContainsKey(id))
            {
                Logger.Error($"Id {id} is not found.");
            }

            _models.Remove(id);
        }

        [NotNull, ItemNotNull]
        public IEnumerable<Model> GetAll() => _models.Values.ToList();

        public int NextId() => _models.Keys.Max() + 1; 
    }
} 