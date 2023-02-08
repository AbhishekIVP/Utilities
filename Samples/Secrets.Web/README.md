# Dapr Secrets

## Current Support

1. Local File
2. Env

## Usage

Run

~~~cmd
dapr run --app-id secret-sidecar-app --log-as-json --enable-api-logging --log-level error
~~~

OR

~~~cmd
dapr run --app-id secret-sidecar-app --dapr-grpc-port 63842 --log-as-json --enable-api-logging --log-level error
~~~

Configure application's appsettings

> Development

~~~json
 "Application": {
    "DefaultSecretStore": "local",
    "IsMultiTenant": true
  }
~~~

> Production

~~~json
 "Application": {
    "DefaultSecretStore": "aws",
    "IsMultiTenant": true
  }
~~~
