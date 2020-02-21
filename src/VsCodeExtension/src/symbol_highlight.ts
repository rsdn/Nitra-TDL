import * as vscode from 'vscode';
import { LanguageClient, NotificationType } from 'vscode-languageclient';
import { HighlightingNotification, SpanClassInfoNotification } from './utils';
import { Range } from 'vscode';


const KeywordHightightNotificationType = new NotificationType<HighlightingNotification , void>('$/keywordHighlight');
const SymbolHightightNotificationType  = new NotificationType<HighlightingNotification , void>('$/symbolHighlight' );
const LanguageLoadedNotificationType   = new NotificationType<SpanClassInfoNotification, void>('$/languageLoaded'  );

var SpanClassInfos = new Map<number, { decor: vscode.TextEditorDecorationType, color: string }>();

export function SubscribeToSymbolHighlightNotyfications(client: LanguageClient) {
  client.onReady().then(() => {
    client.onNotification(KeywordHightightNotificationType, x => { ApplySpanInfos(x); });
    client.onNotification(SymbolHightightNotificationType , x => { ApplySpanInfos(x); });
    client.onNotification(LanguageLoadedNotificationType  , x => {
      x.SpanClassInfo.reduce((k, v) => {
        let col = v.ForegroundColor + 16777216;
        let forecolor = '#' + ('00000' + (col | 0).toString(16)).substr(-6);
        let decor = vscode.window.createTextEditorDecorationType({
          isWholeLine: false
          , color: forecolor
        });
        SpanClassInfos.set(v.Id, { decor: decor, color: forecolor });
        return k;
      });
    });

    vscode.window.onDidChangeActiveTextEditor((e) => {
      client.sendNotification("$/fileActivatedNotification", { uri: e!.document.fileName, version: e!.document.version });
    });
  });
}

function ApplySpanInfos(note: HighlightingNotification): void {

  let editor = vscode.window.activeTextEditor!;
  let doc = editor.document;
  if (!doc) return;

  if (doc.fileName !== note.uri) return;

  let ranges = new Map<number, Range[]>();

  note.spanInfos.forEach((v, i) => {
    let start = doc!.positionAt(v.Span.StartPos);
    let end = doc!.positionAt(v.Span.EndPos);
    var range = new Range(start, end);

    if (ranges.has(v.SpanClassId)) {
      ranges.get(v.SpanClassId)!.push(range);
    }
    else {
      ranges.set(v.SpanClassId, [range]);
    }
  });

  for (let key of ranges) {
    let decor = SpanClassInfos.get(key[0])!;
    if (!decor) {
      let a = 0; a;
    }
    editor.setDecorations(decor.decor, []);
    editor.setDecorations(decor.decor, key[1]);
  }
}