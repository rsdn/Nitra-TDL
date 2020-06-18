param(
{{Parameters}}
)

$TdlDeploymentName = @'
{{Name}}
'@

function TdlMakeLineDirective { 
    "#line $($MyInvocation.ScriptLineNumber + 1) ""$($MyInvocation.ScriptName)"""
}

function TdlWriteLog {
    param ([string]$Text)
    Write-Output "$('{0:yyyy-MM-dd} {0:HH:mm:ss}' -f (Get-Date))|TDL|$Text"
}

TdlWriteLog "Preparing deployment $TdlDeploymentName"

{{Prologue}}

$TdlDeploymentSource = @"
$(TdlMakeLineDirective)
{{Source}}
#line default
"@

TdlWriteLog "Saving deployment source"

$TdlTempFile = [System.IO.Path]::GetTempFileName()

Set-Content -Path $TdlTempFile -Encoding UTF8 -Value $TdlDeploymentSource

TdlWriteLog "Running deployment tool"

{{Tool}} $TdlDeploymentName $TdlTempFile

$TdlExitCode = $LastExitCode

Remove-Item -Path $TdlTempFile

TdlWriteLog "Deployment complete"

exit $TdlExitCode
