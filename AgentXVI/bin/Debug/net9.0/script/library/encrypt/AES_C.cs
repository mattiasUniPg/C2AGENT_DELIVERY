using System.Security.Cryptography;

/*AES simmetrico per crittografia più solida*/
/*CODIFICA*/

public static byte[] AesEncrypt(byte[] data, byte[] key, byte[] iv)
{
    using (Aes aes = Aes.Create())
    {
        aes.Key = key;
        aes.IV = iv;
        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }
}

public static byte[] AesDecrypt(byte[] data, byte[] key, byte[] iv)
{
    using (Aes aes = Aes.Create())
    {
        aes.Key = key;
        aes.IV = iv;
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }
}
// Funzione per caricare un file cifrato via FTP
public static void UploadEncryptedReport(string localFilePath, string ftpUri, byte[] aesKey, byte[] aesIV)
{
    byte[] plainData = File.ReadAllBytes(localFilePath);
    byte[] encryptedData = AesEncrypt(plainData, aesKey, aesIV);

    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUri);
    request.Method = WebRequestMethods.Ftp.UploadFile;
    // You need to define 'username' and 'password' variables or pass them as parameters
    // Example:
    // request.Credentials = new NetworkCredential("yourUsername", "yourPassword");
    request.ContentLength = encryptedData.Length;

    using (Stream requestStream = request.GetRequestStream())
    {
        requestStream.Write(encryptedData, 0, encryptedData.Length);
    }
    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
    {
        Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
    }
}
// Esempio di server FTP molto semplice (solo per scopi dimostrativi)
public class SimpleFtpServer
{
    private readonly int port;
    public SimpleFtpServer(int port)
    {
        this.port = port;
    }

    public void Start()
    {
        TcpListener listener = new TcpListener(System.Net.IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"FTP Server started on port {port}");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected");
            HandleClient(client);
        }
    }

    private void HandleClient(TcpClient client)
    {
        using NetworkStream stream = client.GetStream();
        // Qui si dovrebbe implementare il protocollo FTP completo
        // Per semplicità, supponiamo che il client invii direttamente un file
        string filePath = "received_file.dat";
        using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        byte[] buffer = new byte[4096];
        int bytesRead;

        // Semplice ricezione dati, può essere migliorata per protocollo FTP completo
        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            fileStream.Write(buffer, 0, bytesRead);
            if (stream.DataAvailable == false)
                break; // fine ricezione
        }
        Console.WriteLine($"File saved: {filePath}");
    }

    static void Main()
    {
        var server = new SimpleFtpServer(21);
        server.Start();
    }
}
