/*Logging Sicuro e Anonimizzazione Loggare solo eventi senza dati sensibili Non scrivere mai credenziali o dati decrittati. Anonimizzazione eventuale tramite hashing o masking. Invio log verso SIEM/ELK via syslog/tcp tls o API REST cifrata*/
void SafeLog(string message)
{
    string safeMsg = message.Replace(password, "***");
    File.AppendAllText(logPath, $"{DateTime.UtcNow:o} {safeMsg}\n");
}
/*Esempio di log sicuro*/
SafeLog("Payload downloaded successfully");
// Retry e logging senza password in chiaro

