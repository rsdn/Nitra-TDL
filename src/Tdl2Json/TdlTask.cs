using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TdlTask : ToolTask
{
    protected override string ToolName
    {
        get { return "Tdl2Json.exe"; }
    }

    public string WorkingDirectory
    {
        get;
        set;
    }

    [Required]
    public ITaskItem[] Sources
    {
        get;
        set;
    }

    [Required]
    public ITaskItem[] References
    {
        get;
        set;
    }

    [Required, Output]
    public string OutputFile
    {
        get;
        set;
    }

    protected override string GenerateFullPathToTool()
    {
        return null;
    }

    protected override string GetWorkingDirectory()
    {
        return WorkingDirectory;
    }

    protected override string GetResponseFileSwitch(string responseFilePath)
    {
        return "\"" + "-from-file:" + responseFilePath + "\"";
    }

    protected override string GenerateResponseFileCommands()
    {
        var buffer = new List<string>();
        buffer.Add("");
        foreach (var item in Sources)
        {
            buffer.Add(item.GetMetadata("FullPath"));
        }
        foreach (var item in References)
        {
            buffer.Add(item.GetMetadata("FullPath"));
        }
        buffer.Add("-out:" + OutputFile);
        return string.Join(Environment.NewLine, buffer);
    }

    private static readonly Regex OutputRegex = new Regex(@"(.*?)\((\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)\:\s*(\w*)\:\s*(.*)");

    protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
    {
        var match = OutputRegex.Match(singleLine);
        if (match.Success)
        {
            switch (match.Groups[6].Value)
            {
                case "error":
                    Log.LogError("", "", "", match.Groups[1].Value,
                        int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value),
                        int.Parse(match.Groups[4].Value), int.Parse(match.Groups[5].Value),
                        match.Groups[7].Value);
                    break;
                case "warning":
                    Log.LogWarning("", "", "", match.Groups[1].Value,
                        int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value),
                        int.Parse(match.Groups[4].Value), int.Parse(match.Groups[5].Value),
                        match.Groups[7].Value);
                    break;
                default:
                    Log.LogMessage("", "", "", match.Groups[1].Value,
                        int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value),
                        int.Parse(match.Groups[4].Value), int.Parse(match.Groups[5].Value),
                        match.Groups[7].Value);
                    break;
            }
        }
        else
        {
            base.LogEventsFromTextOutput(singleLine, messageImportance);
        }
    }
}
