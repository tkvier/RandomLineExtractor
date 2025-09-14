using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RandomLineExtractor;

/// <summary>
/// メインウィンドウクラス
/// ユーザーインターフェースの制御とデータバインディングを担当
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly AppConfig? _config;
    private readonly RandomLineExtractorService? _extractorService;

    private string _inputFilePath = "";
    private string _outputFilePath = "";
    private int _randomCount = 10;
    private bool _includeDateTime = true;

    public new event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>読み込みファイルパス</summary>
    public string InputFilePath
    {
        get => _inputFilePath;
        set
        {
            if (_inputFilePath != value)
            {
                _inputFilePath = value;
                OnPropertyChanged();
                if (_config != null)
                {
                    _config.InputFilePath = value;
                    _config.Save();
                }
            }
        }
    }

    /// <summary>出力ファイルパス</summary>
    public string OutputFilePath
    {
        get => _outputFilePath;
        set
        {
            if (_outputFilePath != value)
            {
                _outputFilePath = value;
                OnPropertyChanged();
                if (_config != null)
                {
                    _config.OutputFilePath = value;
                    _config.Save();
                }
            }
        }
    }

    /// <summary>ランダム抽出数</summary>
    public int RandomCount
    {
        get => _randomCount;
        set
        {
            if (_randomCount != value)
            {
                _randomCount = value;
                OnPropertyChanged();
                if (_config != null)
                {
                    _config.RandomCount = value;
                    _config.Save();
                }
            }
        }
    }

    /// <summary>日時含める設定</summary>
    public bool IncludeDateTime
    {
        get => _includeDateTime;
        set
        {
            if (_includeDateTime != value)
            {
                _includeDateTime = value;
                OnPropertyChanged();
                if (_config != null)
                {
                    _config.IncludeDateTime = value;
                    _config.Save();
                }
            }
        }
    }

    public MainWindow()
    {
        try
        {
            InitializeComponent();
            
            // サービスの初期化
            _extractorService = new RandomLineExtractorService();
            
            // 設定の読み込み
            _config = AppConfig.Load();
            LoadConfigToUI();
            
            // データバインディングの設定
            DataContext = this;
            
            UpdateStatus("アプリケーションが正常に起動しました");
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("アプリケーション初期化エラー", ex);
            UpdateStatus($"初期化エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 設定をUIに反映
    /// </summary>
    private void LoadConfigToUI()
    {
        try
        {
            if (_config != null)
            {
                _inputFilePath = _config.InputFilePath;
                _outputFilePath = _config.OutputFilePath;
                _randomCount = _config.RandomCount;
                _includeDateTime = _config.IncludeDateTime;
            }
            
            // プロパティ変更通知（UIに反映）
            OnPropertyChanged(nameof(InputFilePath));
            OnPropertyChanged(nameof(OutputFilePath));
            OnPropertyChanged(nameof(RandomCount));
            OnPropertyChanged(nameof(IncludeDateTime));
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("設定UI反映エラー", ex);
        }
    }

    /// <summary>
    /// 読み込みファイル参照ボタンのクリックイベント
    /// </summary>
    private async void OnInputFileBrowseClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatus("読み込みファイルを選択中...");
            
            var options = new FilePickerOpenOptions
            {
                Title = "読み込みファイルを選択",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("テキストファイル") { Patterns = new[] { "*.txt" } },
                    new FilePickerFileType("すべてのファイル") { Patterns = new[] { "*.*" } }
                },
                AllowMultiple = false
            };

            var files = await StorageProvider.OpenFilePickerAsync(options);
            
            if (files?.Count > 0)
            {
                InputFilePath = files[0].Path.LocalPath;
                UpdateStatus("読み込みファイルが選択されました");
            }
            else
            {
                UpdateStatus("ファイル選択がキャンセルされました");
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("読み込みファイル選択エラー", ex);
            UpdateStatus($"ファイル選択エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 出力ファイル参照ボタンのクリックイベント
    /// </summary>
    private async void OnOutputFileBrowseClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatus("出力ファイルを選択中...");
            
            var options = new FilePickerSaveOptions
            {
                Title = "出力ファイルを指定",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("テキストファイル") { Patterns = new[] { "*.txt" } }
                },
                DefaultExtension = "txt",
                SuggestedFileName = "抽出結果.txt"
            };

            var file = await StorageProvider.SaveFilePickerAsync(options);
            
            if (file != null)
            {
                OutputFilePath = file.Path.LocalPath;
                UpdateStatus("出力ファイルが指定されました");
            }
            else
            {
                UpdateStatus("ファイル選択がキャンセルされました");
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("出力ファイル選択エラー", ex);
            UpdateStatus($"ファイル選択エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 実行ボタンのクリックイベント
    /// </summary>
    private async void OnExecuteClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            // UIの無効化（重複実行防止）
            if (sender is Button button)
            {
                button.IsEnabled = false;
            }

            UpdateStatus("ランダム抽出処理を開始します...");

            // 入力値の検証
            if (!ValidateInputs())
            {
                return;
            }

            // ランダム抽出処理の実行
            if (_extractorService != null)
            {
                var success = await _extractorService.ExtractRandomLinesAsync(
                    InputFilePath,
                    OutputFilePath,
                    RandomCount,
                    IncludeDateTime,
                    UpdateStatus
                );

                if (success)
                {
                    UpdateStatus($"処理完了: {RandomCount}行を正常に抽出しました");
                }
            }
            else
            {
                UpdateStatus("エラー: サービスが初期化されていません");
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("実行ボタン処理エラー", ex, new 
            { 
                InputPath = InputFilePath, 
                OutputPath = OutputFilePath, 
                Count = RandomCount 
            });
            UpdateStatus($"処理エラー: {ex.Message}");
        }
        finally
        {
            // UIの有効化
            if (sender is Button button)
            {
                button.IsEnabled = true;
            }
        }
    }

    /// <summary>
    /// 入力値の検証
    /// </summary>
    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(InputFilePath))
        {
            UpdateStatus("エラー: 読み込みファイルを選択してください");
            return false;
        }

        if (string.IsNullOrWhiteSpace(OutputFilePath))
        {
            UpdateStatus("エラー: 出力ファイルを指定してください");
            return false;
        }

        if (RandomCount <= 0)
        {
            UpdateStatus("エラー: 抽出数は1以上を指定してください");
            return false;
        }

        return true;
    }

    /// <summary>
    /// ステータステキストの更新
    /// </summary>
    private void UpdateStatus(string message)
    {
        try
        {
            if (this.FindControl<TextBlock>("StatusTextBlock") is TextBlock statusTextBlock)
            {
                statusTextBlock.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("ステータス更新エラー", ex, new { Message = message });
        }
    }

    /// <summary>
    /// プロパティ変更通知
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}