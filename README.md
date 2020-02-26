# TDL
Test Definition Language (powered by Nitra)

Real world example which demonstrates how to build a language using Nitra framework.

The TDL describes test scenarios that can then be executed by some runtime execution system. As a result of building a TDL project, a TDL compiler is created – tdlc.exe, a Nuget-package allowing you to build projects that include .tdl-files and Visual Studio extension which adds highlighting and IntelliSense support for tdl-files.

TDL defines a set of testing entities: product, platform, deployment, scenario and suite. As well as their types, groups (lists) and generators (something similar to templates in other programming languages).

tdlc.exe receives .tdl-files as input, references to .net assemblies (optional) and other optional parameters. The result of translation is an output .json-file containing description of entities described in TDL which is suitable for machine processing. Before translation, the code is checked for correctness (syntax check, link check, business rule execution, etc.).

To build a project you need: VS 2017 installed. Latest [Nemerle](https://github.com/rsdn/nemerle/wiki/Nemerle-build-process-(for-Nemerle-developers)) and [Nitra](https://github.com/rsdn/nitra/wiki/Nitra-Build-process) built from source code. They should be placed in the same level folder as TDL folder. For example:
```
C:\RSDN\Nemerle
C:\RSDN\Nitra
C:\RSDN\TDL
```
To build the project, run src\Build.cmd.

Examples of usage located in [Tests/Tdl](Tests/Tdl) folder. TDL tests generate a .json file and compare it to a reference .json file from the [Tests/Samples](Tests/Samples) folder.
