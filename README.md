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

## Build and run the sample with Kubernetes (kind)

Plain Docker Engine does not ship with Kubernetes, and Docker Desktop's bundled cluster is opt-in (Settings → Kubernetes), so this sample uses [kind](https://kind.sigs.k8s.io/) (Kubernetes in Docker) to run a local cluster inside Docker containers. This document won't show you how to install [kind](https://kind.sigs.k8s.io/docs/user/quick-start/#installation) or the command line tool [kubectl](https://kubernetes.io/docs/tasks/tools/).

Create the cluster using the provided `config.yaml` (in the repository root). The config raises the API server's `service-node-port-range`, since the service pins nodePorts 4999 and 5000 below the default 30000–32767 range, and maps those ports to `localhost`:
```pwsh
>  kind create cluster --config config.yaml
```

Next, build the docker image and load it into the cluster. The deployment uses `imagePullPolicy: Never`, so the image must be preloaded onto the cluster nodes:
```pwsh
>  docker build -t aspnetapp-k8s .
>  kind load docker-image aspnetapp-k8s
```

Create the deployment and service:
```pwsh
>  cd aspnetapp
>  kubectl apply -f deployment.yaml -f service.yaml
```

Now check if the deployment succeeded:
```pwsh
>  kubectl get deployments
```

You can also check the statuses of your pods:
```pwsh
>  kubectl get pods
```

> Note: The service is of type `LoadBalancer`, so its `EXTERNAL-IP` stays `<pending>` in a local kind cluster — that's expected. Traffic reaches the pods through the nodePorts mapped by `config.yaml`.

Navigate to http://localhost:4999/scalar in your web browser to test the REST component.

And again for the gRPC piece, you can use a gRPC client (i.e. [grpcurl](https://github.com/fullstorydev/grpcurl)) to connect to `localhost:5000`.

To tear everything down, delete the resources and the cluster:
```pwsh
>  kubectl delete -f deployment.yaml -f service.yaml
>  kind delete cluster
```
