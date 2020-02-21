import * as vscode from 'vscode';
import { TdlTaskProvider } from './tdlTaskProvider';
import { workspace, Range } from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions, NotificationType } from 'vscode-languageclient';
import * as fs from 'fs';
import * as path from 'path';
import { showMessage, showError, ExtentionName, log, HighlightingNotification, SpanClassInfoNotification } from './utils';
import { platform } from 'os';
import { SubscribeToSymbolHighlightNotyfications } from './symbol_highlight';

const langDllName = "Tdl.dll";
const lspServerName = "Nitra.ClientServer.Server.exe";

let tdlTaskProvider: vscode.Disposable | undefined;
let client: LanguageClient;

export function activate(context: vscode.ExtensionContext): void {
  log(`activate`);
  tdlTaskProvider = vscode.tasks.registerTaskProvider(TdlTaskProvider.TdlType, new TdlTaskProvider());
  activateLspServer(context);
}

export function deactivate(): Thenable<void> | undefined {
  if (tdlTaskProvider)
    tdlTaskProvider.dispose();

  if (client)
    return client.stop();
  else
    return undefined;
}

function activateLspServer(context: vscode.ExtensionContext): void {
  log(`activateLspServer`);
  const nitraPath = getNitraPath();

  showMessage(`nitraPath=${nitraPath}`);

  if (!nitraPath)
    return;

  const tdlPath = getTdlPath();

  showMessage(`tdlPath=${tdlPath}`);

  if (!tdlPath)
    return;

  const lspServerPath = path.join(nitraPath, lspServerName);
  const tdlDllPath = path.join(tdlPath, langDllName);

  showMessage(`lspServerPath=${lspServerPath}`);
  showMessage(`tdlDllPath=${tdlDllPath}`);

  let serverOptions: ServerOptions = platform() === 'win32'
    ? { command: lspServerPath, args: [               "-lsp"], options: { stdio: "pipe" } }
    : { command: "mono"       , args: [lspServerPath, "-lsp"], options: { stdio: "pipe" } };

  // Options to control the language client
  let clientOptions: LanguageClientOptions = {
    documentSelector: [
      { scheme: 'file'    , language: 'tdl' },
      { scheme: 'untitled', language: 'tdl' }
    ],
    synchronize:
    {

      // Notify the server about file changes to '.clientrc files contained in the workspace
      fileEvents: [
        workspace.createFileSystemWatcher('**/*.tdl'),
        workspace.createFileSystemWatcher('**/.clientrc')
      ]
    },
    initializationOptions: {
      FileExtension: ".tdl",
      Config: {
        ProjectSupport: {
          Caption: "TdlLang",
          TypeFullName: "Tdl.ProjectSupport",
          Path: tdlDllPath
        },
        Languages: [{
          Name: "TdlLang",
          Path: tdlDllPath,
          DynamicExtensions: []
        }],
        References: [
          "FullName:mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
          "FullName:System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ]
      },
      References: []
    }
  };

  client = new LanguageClient(
    'tdl',
    'TDL Extension',
    serverOptions,
    clientOptions
  );

  SubscribeToSymbolHighlightNotyfications(client);
 
  log(`--> client.start();`);
  client.start();

  log(`<-- client.start();`);
}

let _nitraPath: string | undefined;

function getNitraPath(): string | undefined {
  if (_nitraPath)
    return _nitraPath;

  let nitraBinPath: string | undefined = process.env.NitraBinPath;

  if (!nitraBinPath || !fs.existsSync(nitraBinPath)) {
    const ext = vscode.extensions.getExtension('VladislavChistyakov.tdl')!;
    const binPath = path.join(ext.extensionPath!, "bin");
    if (!fs.existsSync(binPath)) {
      showError(`The TDL extension cannot find a path to Nitra. Reinstall ${ExtentionName} or specify path to Nitra in NitraBinPath environment variable. NitraBinPath="${nitraBinPath}".`);
      return undefined;
    }

    nitraBinPath = binPath;
  }

  const lspServerPath = path.join(nitraBinPath, lspServerName);

  if (!fs.existsSync(nitraBinPath)) {
    showError(`The "${nitraBinPath}" directory does not contain ${lspServerName} LSP Server. Reinstall the ${ExtentionName} or place the Nitra binary in the specified directory.`);
    return undefined;
  }

  return _nitraPath = nitraBinPath;
}

let _tdlPath: string | undefined;

function getTdlPath(): string | undefined {
  if (_tdlPath)
    return _tdlPath;

  const nitraPath = getNitraPath();

  if (nitraPath) {
    const langDllPath = path.join(nitraPath, langDllName);

    if (fs.existsSync(langDllPath))
      return _tdlPath = nitraPath;
  }

  const langPath: string | undefined = process.env.TDL;

  if (langPath && fs.existsSync(langPath)) {
    const langDllPath = path.join(langPath, langDllName);
    if (fs.existsSync(langDllPath))
      return _tdlPath = langPath;

    showError(`The ${langDllName} not found in "${langPath}". Reinstall the ${ExtentionName} or place ${langDllName} into the specified path.`);
    return undefined;
  }

  showError(`The ${langDllName} not found. Reinstall the ${ExtentionName} or set "TDL" environment variable to ${langDllName} location.`);
  return undefined;
}
