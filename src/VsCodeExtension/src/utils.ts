import * as vscode from 'vscode';

export const ExtentionName = "TDL extension";

export function showMessage(text: string): void {
  console.log(text);
  vscode.window.showInformationMessage(text);
}

export function showError(text: string): void {
  console.error(text);
  vscode.window.showErrorMessage(text);
}
