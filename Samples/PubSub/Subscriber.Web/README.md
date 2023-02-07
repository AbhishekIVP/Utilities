# Subscriber Configurations

## Add to the appSettings

~~~config
"Application": {
    "DefaultSecretStore": "local",
    "IsMultiTenant": true
}
~~~

## Run as a package

~~~cmd
dapr run --app-id subscriber --app-port 5001  -- dotnet run --urls="http://+:5001"
~~~

> OR

## Run as sidecar

### Add the following in launchsettings

~~~conf
"environmentVariables": {
    "DAPR_GRPC_PORT": "63840"
}
~~~

~~~cmd
dapr run --app-id subscriber-sidecar-app --app-port 5127 --dapr-grpc-port 63840 --log-as-json --enable-api-logging --log-level error
~~~

~~~cmd
dotnet run --urls="http://+:5127"
~~~
