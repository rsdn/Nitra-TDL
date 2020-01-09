import * as vscode from 'vscode';
import { TdlTaskProvider } from './tdlTaskProvider';
import { workspace, ExtensionContext, Position } from 'vscode';
import { LanguageClient, LanguageClientOptions,	ServerOptions } from 'vscode-languageclient';

let tdlTaskProvider: vscode.Disposable | undefined;
let client: LanguageClient;

export function activate(_context: vscode.ExtensionContext): void
 {
  tdlTaskProvider = vscode.tasks.registerTaskProvider(TdlTaskProvider.TdlType, new TdlTaskProvider());

  let serverOptions: ServerOptions = {
    command: "D:\\!\\RSDN\\nitra\\bin\\Debug\\Stage1\\Nitra.ClientServer.Server.exe",
    args: ["-lsp"],
    options: {
      stdio: "pipe"
    }
  };

  // Options to control the language client
  let clientOptions: LanguageClientOptions = {
    // Register the server for plain text documents
    documentSelector: [{ scheme: 'file', language: 'tdl' }],
    synchronize: {
      // Notify the server about file changes to '.clientrc files contained in the workspace
      fileEvents: workspace.createFileSystemWatcher('**/.clientrc')
    },
    initializationOptions: {
      FileExtension: ".tdl",
      Config: {
        ProjectSupport: {
          Caption: "TdlLang",
          TypeFullName: "Tdl.ProjectSupport",
          Path: "D:\\!\\RSDN\\TDL\\bin\\Debug\\Tdl.dll"
        },
        Languages: [{
          Name: "TdlLang",
          Path: "D:\\!\\RSDN\\TDL\\bin\\Debug\\Tdl.dll",
          DynamicExtensions: []
        }],
        References: []
      },
      References: [
        "FullName:mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
        "FullName:System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
      ]
    }
  };

  client = new LanguageClient(
    'tdl',
    'TDL Extension',
    serverOptions,
    clientOptions
  );

  client.start();
}


export function deactivate(): Thenable<void> | undefined
{
  if (tdlTaskProvider)
    tdlTaskProvider.dispose();

  if (client)
    return client.stop();
  else
    return undefined;
}
