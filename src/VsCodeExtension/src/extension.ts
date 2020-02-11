import * as vscode from 'vscode';
import { TdlTaskProvider } from './tdlTaskProvider';
import { workspace, ExtensionContext, Position, Range } from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions, NotificationType } from 'vscode-languageclient';
import * as fs from 'fs';
import * as path from 'path';
import { showMessage, showError, ExtentionName, log, error } from './utils';
import { platform } from 'os';
import { KeywordsHighlightingCreated_AsyncServerMessage, SymbolsHighlightingCreated_AsyncServerMessage, LanguageLoaded_AsyncServerMessage, SpanInfo } from './NitraMessages';

const langDllName = "Tdl.dll";
const lspServerName = "Nitra.ClientServer.Server.exe";

let tdlTaskProvider: vscode.Disposable | undefined;
let client: LanguageClient;


const KeywordHightightNotificationType = new NotificationType<KeywordsHighlightingCreated_AsyncServerMessage, void>('notification/keywordHighlight');
const SymbolHightightNotificationType = new NotificationType<SymbolsHighlightingCreated_AsyncServerMessage, void>('notification/symbolHighlight');
const LanguageLoadedNotificationType = new NotificationType<LanguageLoaded_AsyncServerMessage, void>('notification/languageLoaded');

var SpanClassInfos = new Map<number, { decor: vscode.TextEditorDecorationType, color: string }>();

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


function ApplySpanInfos(spanInfos: SpanInfo[]) : void {
	
	let editor = vscode.window.activeTextEditor!;
	let doc = editor.document;
	if(!doc) return;

	let ranges = new Map<number, Range[]>();

	spanInfos.forEach((v, i) => {
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

	// for (const key of ranges.keys()) {
	// 	let rng = ranges.get(key).map(a => `[${a.start.line} ${a.start.character} - ${a.end.line} ${a.end.character}]`).join(", ");
	// 	let spi = SpanClassInfos.get(key);
	// 	console.log(`${rng}`, `color:${spi.color};`);
	// }

	for (let key of ranges) {
		let decor = SpanClassInfos.get(key[0])!;
		if (!decor) {
			let a = 0; a;
		}
		editor.setDecorations(decor.decor, []);
		editor.setDecorations(decor.decor, key[1]);
	}
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
		? { command: lspServerPath, args: ["-lsp"], options: { stdio: "pipe" } }
		: { command: "mono", args: [lspServerPath, "-lsp"], options: { stdio: "pipe" } };

	// Options to control the language client
	let clientOptions: LanguageClientOptions = {
		documentSelector: [
			{ scheme: 'file', language: 'tdl' },
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

	client.onReady().then(() => {
		client.onNotification(KeywordHightightNotificationType, x => {
			showMessage("KeywordHightightNotificationType");

			ApplySpanInfos(x.spanInfos);

		});
		client.onNotification(SymbolHightightNotificationType, x => {
			showMessage("SymbolHightightNotificationType");
			ApplySpanInfos(x.spanInfos);
		});

		client.onNotification(LanguageLoadedNotificationType, x => {
			showMessage("LanguageLoadedNotificationType");

			x.spanClassInfos.reduce((k, v) => {
				let col = v.ForegroundColor + 16777216;
				let forecolor = '#' + ('00000' + (col | 0).toString(16)).substr(-6);
				let decor = vscode.window.createTextEditorDecorationType({
					isWholeLine: false
					, color: forecolor
					//, backgroundColor: '#eeeeee'
				});
				SpanClassInfos.set(v.Id, { decor: decor, color: forecolor });
				//l.push(`spanclass ${v.Id}, color ${forecolor}, decor: ${decor.key}`);
				return k;
			});

		});
	});


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
