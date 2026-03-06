using Tva.Application;
using Tva.Modules;
using Tva.Theme;
using Tva.Tui.Rendering;
using Tva.Tui.Runtime;

var modules = ModuleBootstrap.CreateModules();
var registry = new ModuleRegistry(modules);
var clock = new SystemClock();
var shellExecutor = new ProcessShellExecutor();
var terminalCommands = new TerminalCommandService(shellExecutor);
var updates = new LiveUpdateService(clock);
var app = new WorkspaceApp(registry, terminalCommands, updates, TvaThemes.AmberCrt);
var renderer = new SpectreWorkspaceRenderer();
var runtime = new TuiRuntime(app, renderer);

await runtime.RunAsync();
