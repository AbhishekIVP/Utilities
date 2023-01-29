# Dapr Secrets

## Current Support

1. Local File
2. Env

## Usage

Run 
~~~cmd
dapr run --app-id secret-sidecar-app
~~~
OR
~~~cmd
dapr run --app-id secret-sidecar-app --dapr-grpc-port 63842
~~~

Configure application's appsettings

> Development

~~~json
 "Dapr": {
    "DefaultSecretStore": "local",
    "SideCarPort": 63842
  }
~~~

> Production

~~~json
 "Dapr": {
    "DefaultSecretStore": "aws"
  }
~~~
