import * as path from 'path';
import * as fs from 'fs';
import * as vscode from 'vscode';

export class TdlTaskProvider implements vscode.TaskProvider {
  static TdlType: string = 'tdl';

  constructor() {
  }

  public provideTasks(): Thenable<vscode.Task[]> | undefined {
    tryCreateTasks();
    return undefined;
  }

  public resolveTask(_task: vscode.Task): vscode.Task | undefined {
    return undefined;
  }
}

let tdl2JsonPath : string | undefined = undefined;

function tryCreateTasks() : void
{
  console.log("tryCreateTasks()");
  const workspaceFolders = vscode.workspace.workspaceFolders;
  if (!workspaceFolders)
    return;

  for (let workspaceFolder of workspaceFolders)
    tryCreateTask(workspaceFolder);
}

function getTdl2JsonPath() : string
{
  //const ext = vscode.extensions.getExtension('VladislavChistyakov.tdl')!;
  //const extensionPath = ext.extensionPath!;
  //const tdl2JsonPath = path.join(extensionPath, 'Tdl2Json.exe');
  //return tdl2JsonPath;
  return '${env:TDL}Tdl2Json.exe';
}

function showMessage(text : string) : void
{
  console.log(text);
  vscode.window.showInformationMessage(text);
}

function tryCreateTask(workspaceFolder : vscode.WorkspaceFolder) : void
{
  const workFolderRoot = workspaceFolder.uri.fsPath;
  if (!workFolderRoot)
    return;

  const dotVsCodePath = path.join(workFolderRoot, ".vscode");

  if (!fs.existsSync(dotVsCodePath))
    fs.mkdirSync(dotVsCodePath);

  const tasksJsonPath = path.join(dotVsCodePath, 'tasks.json');
  if (fs.existsSync(tasksJsonPath))
    return;

  if (!fs.readdirSync(workFolderRoot).some(file => /\.tdl$/i.test(file))) // file.endsWith('.tdl')
    return;

  if (!tdl2JsonPath)
    tdl2JsonPath = getTdl2JsonPath();

  const content = `{
  // See https://go.microsoft.com/fwlink/?LinkId=733558
  // for the documentation about the tasks.json format
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "type": "shell",
      "windows": {
        "command": "${tdl2JsonPath}",
        "args": [
          "*.tdl",
          "-out:\${workspaceFolder}.json"
        ],
      },
      "linux": {
        "command": "mono",
        "args": [
          "${tdl2JsonPath}",
          "*.tdl",
          "-out:\${workspaceFolder}.json"
        ],
      },
      "osx": {
        "command": "mono",
        "args": [
          "${tdl2JsonPath}",
          "*.tdl",
          "-out:\${workspaceFolder}.json"
        ],
      },
      "group": "build",
      "presentation": {
        // Reveal the output only if unrecognized errors occur.
        "reveal": "always",
        "panel": "shared"
      },
      "problemMatcher": ["$tdl"]
    }
  ]
}
`.replace(/\\/g, '\\\\');
  try {
    fs.writeFileSync(tasksJsonPath, content);
    showMessage("TDL build task was created.");
    showMessage("Set path to Tdl2Json.exe into 'TDL' environment variable!");
  } catch (error) {
    console.error(error);
    vscode.window.showErrorMessage("Failed to create TDL build task!");
  }
}
