/*Generazione Dinamica Chiavi AES Uso di RNG Crypto sicuro e gestione chiavi in memoria volatile:*/
byte[] key = new byte[32];
byte[] iv = new byte[16];
using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(key);
    rng.GetBytes(iv);
}
/*Per scambio sicuro usare Diffie-Hellman o ECDH (preparazione in C#): 
Generare coppie chiavi pubbliche/private Scambiare pubbliche tramite handshake HTTPS Derivare chiave segreta condivisa per AES*/