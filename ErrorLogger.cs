using Newtonsoft.Json;
using System;
using System.IO;

namespace RandomLineExtractor;

/// <summary>
/// エラー情報をJSONファイルとして保存するクラス
/// デバッグ・トラブルシューティングを容易にするための詳細なエラー情報を記録
/// </summary>
public static class ErrorLogger
{
    /// <summary>
    /// エラー情報をJSONファイルに記録
    /// </summary>
    /// <param name="errorType">エラーの種類</param>
    /// <param name="exception">発生した例外</param>
    /// <param name="additionalData">追加のコンテキスト情報</param>
    public static void LogError(string errorType, Exception exception, object? additionalData = null)
    {
        try
        {
            var errorInfo = new
            {
                DateTime = DateTime.Now,
                ErrorType = errorType,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.Message,
                AdditionalData = additionalData
            };

            var json = JsonConvert.SerializeObject(errorInfo, Formatting.Indented);
            var fileName = $"error_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            
            File.WriteAllText(fileName, json);
        }
        catch
        {
            // エラーログの記録に失敗した場合は何もしない
            // 無限ループを防ぐため
        }
    }

    /// <summary>
    /// カスタムエラー情報をJSONファイルに記録
    /// </summary>
    /// <param name="errorType">エラーの種類</param>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="additionalData">追加のコンテキスト情報</param>
    public static void LogCustomError(string errorType, string message, object? additionalData = null)
    {
        try
        {
            var errorInfo = new
            {
                DateTime = DateTime.Now,
                ErrorType = errorType,
                Message = message,
                AdditionalData = additionalData
            };

            var json = JsonConvert.SerializeObject(errorInfo, Formatting.Indented);
            var fileName = $"error_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            
            File.WriteAllText(fileName, json);
        }
        catch
        {
            // エラーログの記録に失敗した場合は何もしない
        }
    }
}