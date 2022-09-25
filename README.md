# Encrypt Microservice (Docker for C# .NET 6.0)

## Cómo clonar este repositorio¿?
  * Agregue su id_pulbic (ssh) a la configuración de su cuenta en [gitlab/github](https://docs.gitlab.com/ee/ssh/).
  * Clone el repositorio con el proyecto: `git clone git@github.com:yanpierchirino/encrypt.git`

## Configuración de su entorno de desarrollo
Independientemente de la plataforma, debe tener acceso una copia desprotegida del código.

## Endpoint
```
import requests
import json

url = "http://localhost:5001/api/v1/encrypt"

payload = json.dumps({
  "datosJSONtxt": "{'merchantId': '1234567','name': 'barnoteuser','password': 'banortepassword','mode': 'AUT', 'controlNumber': 'REF1', 'terminalId': '12345678', 'amount': '100', 'merchantName': 'My Company Name', 'merchantCity': 'Mazatlan', 'lang':'ES'}",
  "rsaPublicKey": "MIIGfDCCBWSgAwIBAgITFwAAzM6wEEhjctuWpAABAADMzjANBgkqhkiG9w0BAQsF..."
})

headers = {
  'Content-Type': 'application/json'
}

response = requests.request("POST", url, headers=headers, data=payload)

print(response.text)
```
