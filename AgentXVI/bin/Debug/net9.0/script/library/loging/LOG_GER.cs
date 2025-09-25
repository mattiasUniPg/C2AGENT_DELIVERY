/*Logging, Retry e Upload Sicuro Report via FTP*/

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

// Logger, retry e upload sicuro report via FTP
public static class LogGer
{
    // Logger semplice con scrittura su file e console
    public static void Log(string message, string logFile = "agent.log")
    {
        string entry = $"[{DateTime.Now}] {message}";
        Console.WriteLine(entry);
        File.AppendAllText(logFile, entry + Environment.NewLine);
    }

    // Retry automatico download/upload con max tentativi
    public static bool RetryAction(Action action, int maxRetries = 3, int delayMs = 2000)
    {
        int attempts = 0;
        while (attempts < maxRetries)
        {
            try
            {
                action();
                return true;
            }
            catch
            {
                attempts++;
                Thread.Sleep(delayMs);
            }
        }
        return false;
    }
    // Upload sicuro report via FTP con crittografia AES
    public static void UploadEncryptedReport(string localFilePath, string ftpUri, byte[] aesKey, byte[] aesIv)
    {
        byte[] reportData = File.ReadAllBytes(localFilePath);
        byte[] encryptedData = AesEncrypt(reportData, aesKey, aesIv);

        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUri);
        request.Method = WebRequestMethods.Ftp.UploadFile;
        request.Credentials = new NetworkCredential("ftpUser", "ftpPassword");
        request.EnableSsl = true;

        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(encryptedData, 0, encryptedData.Length);
        }

        using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
        Log($"Upload File Complete, status {response.StatusDescription}");
    }
    // Esempio di utilizzo
    public static void Main()
    {
        string localReportPath = "report.txt";
        string ftpUri = "ftp://server/upload/report.enc";
        byte[] aesKey = new byte[32]; // Replace with actual key
        byte[] aesIv = new byte[16];  // Replace with actual IV
        int maxRetries = 3;
        int retryDelayMs = 2000;

        while (true)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    UploadEncryptedReport(localReportPath, ftpUri, aesKey, aesIv);
                    Log("Report uploaded successfully");
                    break;
                }
                catch (Exception ex)
                {
                    Log($"Upload attempt {attempt + 1} failed: {ex.Message}");
                    Thread.Sleep(retryDelayMs);
                }
            }
            Thread.Sleep(60000); // Wait before next upload
        }
    }
    // AES Encrypt/Decryption
    public static byte[] AesEncrypt(byte[] data, byte[] key, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        using MemoryStream ms = new();
        using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cryptoStream.Write(data, 0, data.Length);
        }
        return ms.ToArray();
    }

    public static byte[] AesDecrypt(byte[] cipherText, byte[] key, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        using MemoryStream ms = new(cipherText);
        using CryptoStream cryptoStream = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using MemoryStream resultStream = new();
        cryptoStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
    // Retry e logging senza password in chiaro
    // Esempio di log sicuro
    public static void SafeLog(string message)
    {
        string logPath = "agent_log.txt";
        string password = "s3cr3t"; // Example sensitive data
        string safeMsg = message.Replace(password, "***");
        File.AppendAllText(logPath, $"{DateTime.UtcNow:o} {safeMsg}\n");
    }
    // Main loop
    public static async Task MainLoopAsync()
    {
        string localReportPath = "report.txt";
        string ftpUri = "ftp://server/upload/report.enc";
        byte[] aesKey = new byte[32]; // Replace with actual key
        byte[] aesIv = new byte[16];  // Replace with actual IV
        int maxRetries = 3;
        int retryDelayMs = 2000;

        while (true)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    UploadEncryptedReport(localReportPath, ftpUri, aesKey, aesIv);
                    SafeLog("Report uploaded successfully");
                    break;
                }
                catch (Exception ex)
                {
                    SafeLog($"Upload failed: {ex.Message}");
                    await Task.Delay(retryDelayMs);
                }
            }
            await Task.Delay(60000); // Wait before next upload
        }
    }
}
