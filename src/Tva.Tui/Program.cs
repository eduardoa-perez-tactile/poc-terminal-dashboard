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
var workTracker = new PrototypeWorkTrackerDataSource();
var codingActivity = new PrototypeCodingActivityDataSource();
var changeDelivery = new PrototypeChangeDeliveryDataSource();
var reviewQueue = new PrototypeReviewQueueDataSource();
var communications = new PrototypeCommunicationsDataSource();
var workLog = new PrototypeWorkLogDataSource();
var readingList = new PrototypeReadingListDataSource();
var updates = new LiveUpdateService(clock, workTracker, codingActivity, changeDelivery, reviewQueue, communications, workLog, readingList);
var app = new WorkspaceApp(moduleCatalog, navigation, sessionState, notifications, terminalCommands, updates, TvaThemes.AmberCrt);
var renderer = new SpectreWorkspaceRenderer();
var runtime = new TuiRuntime(app, renderer);

await runtime.RunAsync();
