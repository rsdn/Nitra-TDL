using JetBrains.Annotations;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace KL.TdlTransformator
{
    public sealed class CommandLineOptions
    {
        private readonly OptionSet _optionSet;

        public CommandLineOptions()
        {
            _optionSet = new OptionSet()
            {
                { "?|h|help|list", "print help", _ => NeedHelp = true },
                { "set-mask", "set new mask", v => ChangeMask(v) },
                { "f|getfiles", "print all tdl files paths", _ => Command = ArgumentCommand.GetFiles },
                { "short-path", "print short paths",  _ => IsShortPath = true },
                { "m|getmodels", "print models names", _ => Command = ArgumentCommand.GetModels },
                { "r|rmunused", "remove unused scenario", _ => Command = ArgumentCommand.RmUnused },
                { "clrdupenv", "replace scenario dups env", _ => Command = ArgumentCommand.ClrDupEnv },
                { "e|clrenv", "simplify environments", _ => Command = ArgumentCommand.ClrEnv },
                { "d|clrdup", "replace scenario dups", _ => Command = ArgumentCommand.ClrDup },
                { "n|clrname", "clear scenario names", _ => Command = ArgumentCommand.ClrName },
                { "clrsets", "replace scenario_set dups", _ => Command = ArgumentCommand.ClrSets },
                { "s|sort", "sort module members", _ => Command = ArgumentCommand.Sort },
                { "reset", "parse existing context", _ => Command = ArgumentCommand.Reset },
                { "save", "save parsed models", _ => Command = ArgumentCommand.Save },
                { "exit|quit", "exit", _ => NeedExit = true },
                { "save-mod", "save changed models", _ => Command = ArgumentCommand.SaveMod },
                { "u:|ungrouping:", "ungrouping scenarios", type => { Command = ArgumentCommand.Ungroup; UngroupType = type; } },
                { "pu:|printungroup:", "specify ungroup output", output => { NeedPrintUngroup = true; UngroupInfoOutput = output; } }
            };
        }

        public bool NeedHelp { get; set; }

        public bool NeedExit { get; set; }

        public bool IsShortPath { get; set; }

        public bool NeedPrintUngroup { get; set; }

        [CanBeNull]
        public string UngroupInfoOutput { get; set; }

        [CanBeNull]
        public string UngroupType { get; set; }

        [NotNull]
        public string Mask { get; set; } = "."; 
        
        [NotNull]
        public string Command { get; set; }
        
        public void Parse([NotNull, ItemNotNull] IEnumerable<string> arguments)
        {
            Clear();

            var extra = _optionSet.Parse(arguments);

            if (extra.Count > 0)
            {
                var extraArgs = string.Join(", ", extra);
                Console.WriteLine($"extra argument(s): [{extraArgs}]");
                PrintHelp(Console.Out);
                Command = ArgumentCommand.Undefined;
            } 
        }

        public void PrintHelp([NotNull] TextWriter writer)
        {
            _optionSet.WriteOptionDescriptions(writer);
        }

        private void ChangeMask([CanBeNull] string newMask)
        {
            if (string.IsNullOrEmpty(newMask.Trim()))
            {
                Console.WriteLine("input new mask (regex):");
                newMask = Console.ReadLine();
                if (string.IsNullOrEmpty(newMask.Trim()))
                {
                    return;
                }

                Mask = newMask;
            } 
        }

        private void Clear()
        {
            IsShortPath = false;
            NeedPrintUngroup = false;
            UngroupType = null;
            UngroupInfoOutput = null;
        }
    }
} 