# Hybrid RESTful and gRPC service using .NET 10

This document outlines how to get started with a hybrid REST and gRPC service using ASP.NET Core on .NET 10.

## Motivation

While looking at migrating existing APIs from REST to gRPC, I struggled to find a working C# example, where I could run a hybrid between the two. I did not want to convert an existing service strictly to gRPC and throw away the REST implementation since many legacy services may still depend on it. Instead, I wondered if it could be possible to add the gRPC component on top of an existing REST service and expose separate ports to run HTTP/1.x and HTTP/2 connections. For a relatively simple idea, I had hoped their would-be various documentation for how this could be achieved, but after a bit of researching, I decided to create my own example.

## Build and run the sample

You can import the code straight into your preferred IDE (i.e. Visual Studio) or run the sample using the `dotnet` CLI commands (in the root project folder).

```pwsh
>  dotnet build
>  dotnet .\aspnetapp\bin\Debug\net10.0\aspnetapp.dll
```

After the application runs, navigate to http://localhost:4999/scalar in your web browser to access the Scalar API reference (the OpenAPI document itself is served at http://localhost:4999/openapi/v1.json). Enter a value in the name field and it should return something similar like this below:

```json
{
  "message": "Hello Ben"
}
```

For the gRPC piece, you can install a gRPC client (i.e. [BloomRPC](https://github.com/uw-labs/bloomrpc)). Import the `greet.proto` file to the client and enter the server address as `localhost:5000`.

![alt text](bloomrpc.png?raw=true "BloomRPC Example")

## Build and run the sample with Docker

You can build and run the sample in Docker using the following commands. Navigate to the folder where the Dockerfile lies.

```pwsh
>  docker build -t aspnetapp-k8s .
>  docker run -it --rm -p 9000:4999 -p 9001:5000 --name aspnetcore-sample aspnetapp-k8s
```

After the application starts, navigate to http://localhost:9000/scalar in your web browser.

> Note: The run command `-p` argument maps ports 9000 and 9001 on the local machine to ports 4999 and 5000 in the container (the form of the port mapping is `host:container`).

## Build and run the sample with Minikube

If you want to run Kubernetes locally, you can spin up a whole cluster manually. Another solution is to use [minikube](https://kubernetes.io/docs/setup/minikube/). 

You can build and run the sample locally with [minikube](https://kubernetes.io/docs/setup/minikube/). This document won't show you how to install [minikube](https://kubernetes.io/docs/setup/minikube/) or the command line tool [kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/).

If this is your first time setting up `minikube`, the Docker daemon in `minikube` may not initally know about the Docker daemon in your host. You can share the context by running the following commmands:
```pwsh
>  minikube docker-env
>  SET DOCKER_TLS_VERIFY=1
>  SET DOCKER_HOST=tcp://172.17.13.216:2376
>  SET DOCKER_CERT_PATH=C:\Users\[USERNAME]\.minikube\certs
>  REM Run this command to configure your shell:
>  REM @FOR /f "tokens=*" %i IN ('minikube docker-env') DO @%i
```

`minikube` also provides a port range between 30000–32767 for services, so when a new service gets created a random port gets choosen. You can specify the nodePort range by running a similar command like this one below:
```pwsh
>  minikube start --extra-config=apiserver.service-node-port-range=80-30000
```

> Note: To stop the local `minikube` cluster, run the command: `minikube stop`

Next, build the docker image:
```pwsh
>  docker build -t aspnetapp-k8s .
```

Create a service and a deployment:
```pwsh
>  cd aspnetapp
>  kubectl create -f service.yaml
>  kubectl create -f deployment.yaml
```

> Note: To delete a service and deployment, you can run the following commands: `kubectl delete service aspnetapp-k8s` and `kubectl delete deployment aspnetapp-k8s`.

Now check if the deployment succeeded:
```pwsh
>  kubectl get deployments
```

You can also check the statuses of your pods:
```pwsh
>  kubectl get pods
```

To find out which IP and ports have been exposed, use this command (this is different when using managed k8s):
```pwsh
>  minikube service aspnetapp-k8s --url
```

You'll get an output similar to this:
```pwsh
http://192.168.99.100:4999
http://192.168.99.100:5000
```

Navigate to http://192.168.99.100:4999/scalar in your web browser to test the REST component.

And again for the gRPC piece, you can use a gRPC client (i.e. [BloomRPC](https://github.com/uw-labs/bloomrpc)) to connect to `192.168.99.100:5000`.