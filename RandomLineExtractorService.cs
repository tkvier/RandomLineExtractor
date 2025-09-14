using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomLineExtractor;

/// <summary>
/// テキストファイルからランダムに行を抽出し、別ファイルに出力するサービス
/// </summary>
public class RandomLineExtractorService
{
    /// <summary>
    /// ランダム行抽出処理の実行
    /// </summary>
    /// <param name="inputPath">読み込み元ファイルパス</param>
    /// <param name="outputPath">出力先ファイルパス</param>
    /// <param name="count">抽出する行数</param>
    /// <param name="includeDateTime">日時を含めるかのフラグ</param>
    /// <param name="statusCallback">ステータス更新コールバック</param>
    public async Task<bool> ExtractRandomLinesAsync(
        string inputPath, 
        string outputPath, 
        int count, 
        bool includeDateTime,
        Action<string>? statusCallback = null)
    {
        try
        {
            // 入力ファイルの検証
            statusCallback?.Invoke("入力ファイルの検証中...");
            if (!ValidateInputFile(inputPath))
            {
                return false;
            }

            // ファイルの読み込み
            statusCallback?.Invoke("ファイルを読み込み中...");
            var lines = await ReadFileAsync(inputPath);
            
            if (lines.Count == 0)
            {
                ErrorLogger.LogCustomError("ファイル内容エラー", "読み込んだファイルに行が存在しません", 
                    new { InputPath = inputPath });
                statusCallback?.Invoke("エラー: ファイルに行が存在しません");
                return false;
            }

            // 抽出行数の検証
            if (count > lines.Count)
            {
                ErrorLogger.LogCustomError("抽出行数エラー", "指定された抽出行数がファイルの行数を超えています", 
                    new { RequestedCount = count, AvailableLines = lines.Count, InputPath = inputPath });
                statusCallback?.Invoke($"エラー: 抽出行数({count})がファイルの行数({lines.Count})を超えています");
                return false;
            }

            // ランダム行の抽出
            statusCallback?.Invoke($"{count}行をランダム抽出中...");
            var randomLines = ExtractRandomLines(lines, count);

            // 出力ディレクトリの作成
            statusCallback?.Invoke("出力ディレクトリの準備中...");
            await CreateOutputDirectoryAsync(outputPath);

            // ファイルへの書き込み
            statusCallback?.Invoke("結果をファイルに書き込み中...");
            await WriteOutputFileAsync(outputPath, randomLines, includeDateTime);

            statusCallback?.Invoke($"完了: {count}行を正常に抽出しました");
            return true;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("ランダム行抽出処理エラー", ex, 
                new { InputPath = inputPath, OutputPath = outputPath, Count = count, IncludeDateTime = includeDateTime });
            statusCallback?.Invoke($"エラー: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 入力ファイルの検証
    /// </summary>
    private bool ValidateInputFile(string inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            ErrorLogger.LogCustomError("入力パス検証エラー", "入力ファイルパスが指定されていません", 
                new { InputPath = inputPath });
            return false;
        }

        if (!File.Exists(inputPath))
        {
            ErrorLogger.LogCustomError("入力ファイル存在エラー", "指定された入力ファイルが存在しません", 
                new { InputPath = inputPath });
            return false;
        }

        return true;
    }

    /// <summary>
    /// ファイルの非同期読み込み
    /// </summary>
    private async Task<List<string>> ReadFileAsync(string filePath)
    {
        try
        {
            var lines = new List<string>();
            using var reader = new StreamReader(filePath, Encoding.UTF8);
            
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lines.Add(line);
            }

            return lines;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("ファイル読み込みエラー", ex, new { FilePath = filePath });
            throw;
        }
    }

    /// <summary>
    /// リストからランダムに指定数の要素を抽出
    /// </summary>
    private List<string> ExtractRandomLines(List<string> lines, int count)
    {
        try
        {
            var random = new Random();
            return lines.OrderBy(x => random.Next()).Take(count).ToList();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("ランダム抽出エラー", ex, 
                new { TotalLines = lines.Count, RequestedCount = count });
            throw;
        }
    }

    /// <summary>
    /// 出力ディレクトリの作成
    /// </summary>
    private async Task CreateOutputDirectoryAsync(string outputPath)
    {
        try
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await Task.CompletedTask; // 非同期処理の一貫性のため
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("出力ディレクトリ作成エラー", ex, new { OutputPath = outputPath });
            throw;
        }
    }

    /// <summary>
    /// 結果をファイルに書き込み
    /// </summary>
    private async Task WriteOutputFileAsync(string outputPath, List<string> lines, bool includeDateTime)
    {
        try
        {
            using var writer = new StreamWriter(outputPath, false, Encoding.UTF8);
            
            if (includeDateTime)
            {
                await writer.WriteLineAsync($"# ランダム抽出結果 - {DateTime.Now:yyyy/MM/dd HH:mm:ss}");
                await writer.WriteLineAsync($"# 抽出行数: {lines.Count}");
                await writer.WriteLineAsync();
            }

            foreach (var line in lines)
            {
                await writer.WriteLineAsync(line);
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError("ファイル書き込みエラー", ex, 
                new { OutputPath = outputPath, LineCount = lines.Count, IncludeDateTime = includeDateTime });
            throw;
        }
    }
}