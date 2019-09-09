param(
{{Parameters}}
)

$TdlDeploymentName = @'
{{Name}}
'@

function TdlMakeLineDirective { 
    "#line $($MyInvocation.ScriptLineNumber + 1) ""$($MyInvocation.ScriptName)"""
}

{{Prologue}}

Write-Output $TdlDeploymentName

$TdlDeploymentSource = @"
$(TdlMakeLineDirective)
{{Source}}
#line default
"@

$TdlTempFile = [System.IO.Path]::GetTempFileName()

Set-Content -Path $TdlTempFile -Encoding UTF8 -Value $TdlDeploymentSource

{{Tool}} $TdlDeploymentName $TdlTempFile

$TdlExitCode = $LastExitCode

Remove-Item -Path $TdlTempFile

exit $TdlExitCode
