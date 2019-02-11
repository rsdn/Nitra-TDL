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

    protected override MessageImportance StandardErrorLoggingImportance
    {
        get { return MessageImportance.High; }
    }

    protected override MessageImportance StandardOutputLoggingImportance
    {
        get { return MessageImportance.Low; }
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
}
