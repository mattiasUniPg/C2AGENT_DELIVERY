using System;
using System.Net;

/*client FTP in C# che può caricare file su un server FTP. Per uso avanzato si può estendere con autenticazione, gestione directory, download file*/
/*permette di caricare (upload) file di tutte le estensioni al server*/

class SimpleFtpClient
{
    static void Main()
    {
        string ftpServer = "ftp://127.0.0.1"; // Cambiare con IP del server
        string username = "anonymous"; // Se autenticazione richiesta
        string password = "anonymous";

        string localFilePath = @"C:\path\to\file.exe"; // File locale da caricare
        string uploadFileName = "uploaded_file.exe"; // Nome con cui sarà salvato sul server

        UploadFile(ftpServer, username, password, localFilePath, uploadFileName);
    }

    static void UploadFile(string ftpServer, string username, string password, string localFile, string uploadFileName)
    {
        try
        {
            string uri = $"{ftpServer}/{uploadFileName}";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(username, password);

            byte[] fileContents = System.IO.File.ReadAllBytes(localFile);
            request.ContentLength = fileContents.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            using (var response = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during FTP upload: " + ex.Message);
        }
    }
}
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
// Retry e logging senza password in chiar


