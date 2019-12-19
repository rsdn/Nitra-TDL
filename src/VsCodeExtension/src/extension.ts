import * as vscode from 'vscode';
import { TdlTaskProvider } from './tdlTaskProvider';

let rakeTaskProvider: vscode.Disposable | undefined;
let customTaskProvider: vscode.Disposable | undefined;

export function activate(_context: vscode.ExtensionContext): void {
  rakeTaskProvider = vscode.tasks.registerTaskProvider(TdlTaskProvider.TdlType, new TdlTaskProvider());
}


export function deactivate(): void {
  if (rakeTaskProvider)
    rakeTaskProvider.dispose();

  if (customTaskProvider)
    customTaskProvider.dispose();
}
