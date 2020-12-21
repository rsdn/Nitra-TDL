﻿using Microsoft.Build.Framework;
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

    public string DeploymentScriptHeader
    {
        get;
        set;
    }

    public string BooleanMarshalMode
    {
        get;
        set;
    }

    public string DiffFile
    {
        get;
        set;
    }

    public string RepositoryPath
    {
        get;
        set;
    }

    public bool ExcludeTriggerPathScenarios
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

    protected override string GenerateResponseFileCommands()
    {
        var buffer = new List<string>();
        buffer.Add("-log-level:short");
        foreach (var item in Sources)
        {
            buffer.Add(EscapeFilePath(item.GetMetadata("FullPath")));
        }
        foreach (var item in References)
        {
            buffer.Add(EscapeFilePath(item.GetMetadata("FullPath")));
        }
        if (!string.IsNullOrEmpty(DeploymentScriptHeader))
        {
            buffer.Add("-deployment-header:" + EscapeFilePath(DeploymentScriptHeader));
        }
        if (!string.IsNullOrEmpty(BooleanMarshalMode))
        {
            buffer.Add("-bool-marshal-mode:" + EscapeFilePath(BooleanMarshalMode));
        }
        if (!string.IsNullOrEmpty(DiffFile))
        {
            buffer.Add("-diff-file:" + EscapeFilePath(DiffFile));
        }
        if (!string.IsNullOrEmpty(RepositoryPath))
        {
            buffer.Add("-repo-directory:" + EscapeFilePath(RepositoryPath));
        }
        if (ExcludeTriggerPathScenarios)
        {
            buffer.Add("-exclude-triggerpath-scenarios");
        }
        buffer.Add("-out:" + EscapeFilePath(OutputFile));
        return string.Join(Environment.NewLine, buffer);
    }

    private static string EscapeFilePath(string filePath)
    {
        return filePath.IndexOf(' ') < 0 ? filePath : ("\"" + filePath + "\"");
    }
}
