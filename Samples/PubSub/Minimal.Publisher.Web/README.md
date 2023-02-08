# Publisher Configurations

## Add to the appSettings

~~~config
"Application": {
    "DefaultSecretStore": "local",
    "IsMultiTenant": true
}
~~~

## Run as a package

~~~cmd
dapr run --app-id publisher -- dotnet run
~~~

> OR

## Run as sidecar

### Add the following in launchsettings

~~~conf
"environmentVariables": {
    "DAPR_GRPC_PORT": "63841"
}
~~~

~~~cmd
dapr run --app-id publisher-sidecar-app --dapr-grpc-port 63841 --log-as-json --enable-api-logging --log-level error
~~~

~~~cmd
dotnet run
~~~
