import * as vscode from 'vscode';

export const ExtentionName = "TDL extension";

export function showMessage(text: string): void {
  log(text);
  vscode.window.showInformationMessage(text);
}

export function showError(text: string): void {
  error(text);
  vscode.window.showErrorMessage(text);
}

export function log(text: string): void {
  console.log(date.toISOString() + " " + text);
}

export function error(text: string): void {
  console.error(date.toISOString() + " " + text);
}

const date = new Date();
