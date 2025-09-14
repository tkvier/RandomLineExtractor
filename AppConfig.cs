using Newtonsoft.Json;
using System;
using System.IO;

namespace RandomLineExtractor;

/// <summary>
/// アプリケーションの設定を管理するクラス
/// 設定の保存・読み込み機能を提供
/// </summary>
public class AppConfig
{
    private const string CONFIG_FILE = "config.json";

    /// <summary>読み込み対象のtxtファイルパス</summary>
    public string InputFilePath { get; set; } = "";

    /// <summary>出力先のtxtファイルパス</summary>
    public string OutputFilePath { get; set; } = "";

    /// <summary>抽出するランダム行数</summary>
    public int RandomCount { get; set; } = 10;

    /// <summary>出力時に日時を含めるかのフラグ</summary>
    public bool IncludeDateTime { get; set; } = true;

    /// <summary>
    /// 設定をJSONファイルに保存
    /// </summary>
    public void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(CONFIG_FILE, json);
        }
        catch (Exception ex)
        {
            // 設定保存エラーは致命的でないため、エラーログのみ出力
            ErrorLogger.LogError("設定保存エラー", ex, new { ConfigFile = CONFIG_FILE });
        }
    }

    /// <summary>
    /// JSONファイルから設定を読み込み
    /// </summary>
    /// <returns>読み込まれた設定オブジェクト</returns>
    public static AppConfig Load()
    {
        try
        {
            if (!File.Exists(CONFIG_FILE))
            {
                // 設定ファイルが存在しない場合はデフォルト設定を返す
                return new AppConfig();
            }

            var json = File.ReadAllText(CONFIG_FILE);
            var config = JsonConvert.DeserializeObject<AppConfig>(json);
            return config ?? new AppConfig();
        }
        catch (Exception ex)
        {
            // 設定読み込みエラー時はデフォルト設定を返し、エラーログを記録
            ErrorLogger.LogError("設定読み込みエラー", ex, new { ConfigFile = CONFIG_FILE });
            return new AppConfig();
        }
    }
}