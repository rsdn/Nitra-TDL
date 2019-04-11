param(
{{Parameters}}
)
function TdlMakeLineDirective { 
    "#line $($MyInvocation.ScriptLineNumber + 1) ""$($MyInvocation.ScriptName)"""
}
{{Prologue}}

$TdlDeploymentSource = @"
$(TdlMakeLineDirective)
{{Source}}
#line default
"@

$TdlDeploymentName = @'
{{Name}}
'@

$TdlTempFile = New-TemporaryFile

Set-Content -Path $TdlTempFile -Encoding UTF8 -Value $TdlDeploymentSource

{{Tool}} $TdlDeploymentName $TdlTempFile

$TdlExitCode = $LastExitCode

Remove-Item -Path $TdlTempFile

exit $TdlExitCode
