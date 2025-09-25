#!/bin/bash

PORT=8080
FILE_TO_SEND="payload.exe"

# Serve il file quando richiesto, risposta HTTP minimale
while true; do
  # attende la connessione e legge la richiesta
  REQUEST=$(nc -l -p $PORT -q 1)
  
  # Controlla se la richiesta contiene GET per il file
  if echo "$REQUEST" | grep -q "GET /$FILE_TO_SEND"; then
    echo -e "HTTP/1.1 200 OK\nContent-Type: application/octet-stream\n\n$(cat $FILE_TO_SEND)" | nc -l -p $PORT -q 1
  else
    # Risposta 404 per richieste diverse
    echo -e "HTTP/1.1 404 Not Found\n\nFile not found" | nc -l -p $PORT -q 1
  fi
done
# Nota: Questo è un esempio molto semplice e non gestisce molte situazioni reali.
# Per un server HTTP completo, considera l'uso di strumenti come Python's http.server o simili.
# Assicurati di avere i permessi necessari per eseguire questo script e che la porta 8080 sia libera.
# Esegui con: bash HTTP_NSF.sh
# Per testare, puoi usare: curl http://localhost:8080/payload.exe --output downloaded_payload.exe
# Nota: Questo script è inteso per scopi educativi e di test in ambienti controllati.
# Non utilizzare in ambienti di produzione senza adeguate misure di sicurezza.
# Assicurati di avere netcat (nc) installato sul tuo sistema.
#  