using System.IO;
using System.Windows;
using System.Windows.Threading;
using ToolBoxApp.Models;
using ToolBoxApp.Services;

namespace ToolBoxApp;

/// <summary>
/// 程序入口。支持两种启动模式：
///   1. 正常启动：显示主窗口
///   2. --sync-autostart：后台静默运行一次自启动清单同步后立即退出（供计划任务调用）
/// 全局异常会被捕获并写入日志文件，避免闷声崩溃。
/// </summary>
public partial class App : Application
{
    private static readonly string LogPath = Path.Combine(ConfigService.RootDir, "crash.log");

    protected override async void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (_, args) =>
        {
            LogException(args.Exception, "UI线程未处理异常");
            MessageBox.Show($"程序遇到错误：\n{args.Exception.Message}\n\n详细信息已记录到：\n{LogPath}",
                "出错了", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            LogException(args.ExceptionObject as Exception, "AppDomain未处理异常");
        };
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            LogException(args.Exception, "异步任务未观察异常");
            args.SetObserved();
        };

        base.OnStartup(e);

        if (e.Args.Contains("--sync-autostart"))
        {
            await RunSilentSyncAsync();
            Shutdown();
            return;
        }

        var mainWindow = new MainWindow();
        mainWindow.Show();
    }

    private static void LogException(Exception? ex, string context)
    {
        try
        {
            var text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}\n{ex}\n\n";
            File.AppendAllText(LogPath, text);
        }
        catch
        {
            // 日志写入失败也不能再抛异常
        }
    }

    private static async Task RunSilentSyncAsync()
    {
        try
        {
            var config = ConfigService.Load();
            var syncService = new AutoStartSyncService();
            await syncService.SyncAllAsync(config.AutoStartItems);
            ConfigService.Save(config);
        }
        catch (Exception ex)
        {
            LogException(ex, "静默同步失败");
        }
    }
}

