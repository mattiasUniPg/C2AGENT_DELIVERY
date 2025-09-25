/*Trasferimento Batch Comprimente e Cifrato Comprime report con GZip Cifra con AES Invia tramite HTTPS POST multiparte*/
byte[] Compress(byte[] data)
{
    using (var ms = new MemoryStream())
    using (var gzip = new GZipStream(ms, CompressionLevel.Optimal))
    {
        gzip.Write(data, 0, data.Length);
        gzip.Close();
        return ms.ToArray();
    }
}
