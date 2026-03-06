using Tva.Application;
using Tva.Contracts;
using Tva.Modules;
using Tva.Theme;
using Tva.Tui.Rendering;
using Tva.Tui.Runtime;

var modules = ModuleBootstrap.CreateModules();
IModuleCatalog moduleCatalog = new ModuleCatalog(modules);
ISessionState sessionState = new AppSessionState();
INavigationService navigation = new NavigationService(sessionState);
INotificationService notifications = new NotificationService(sessionState);
var clock = new SystemClock();
var shellExecutor = new ProcessShellExecutor();
var terminalCommands = new TerminalCommandService(shellExecutor);
var updates = new LiveUpdateService(clock);
var app = new WorkspaceApp(moduleCatalog, navigation, sessionState, notifications, terminalCommands, updates, TvaThemes.AmberCrt);
var renderer = new SpectreWorkspaceRenderer();
var runtime = new TuiRuntime(app, renderer);

await runtime.RunAsync();
