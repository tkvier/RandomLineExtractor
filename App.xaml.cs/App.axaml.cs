using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;

namespace RandomLineExtractor;

/// <summary>
/// Avaloniaアプリケーションのメインクラス
/// アプリケーションの起動とライフサイクル管理を担当
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// アプリケーションの初期化処理
    /// XAMLリソースの読み込みを行う
    /// </summary>
    public override void Initialize()
    {
        try
        {
            AvaloniaXamlLoader.Load(this);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("アプリケーション初期化エラー", ex);
            throw;
        }
    }

    /// <summary>
    /// アプリケーション起動完了時の処理
    /// メインウィンドウの作成と表示を行う
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // メインウィンドウの作成と設定
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("フレームワーク初期化エラー", ex);
            throw;
        }
    }
}

/// <summary>
/// アプリケーションのエントリーポイント
/// </summary>
public class Program
{
    // メインエントリーポイント
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            // 最上位レベルでの例外処理
            ErrorLogger.LogError("アプリケーション起動エラー", ex, new { Args = args });
            throw;
        }
    }

    /// <summary>
    /// Avaloniaアプリケーションビルダーの構成
    /// フォント、テーマ、ロギング設定を含む
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}