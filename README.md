# Endpoints monitoring service

REST API microservice for monitoring http/https URLs, written in .NET Core.


## Description
Authorized user can create `MonitoredEndpoint` via JSON POST request. Service will be sending GET requests to the `Url` in interval (s) defined by `MonitoringResult`. Responses are saved into object `MonitoringResult` (http status code, payload).
User can view one particular result, last 10 results or last 10 results by particular `MonitoredEndpoint`.

`MonitoredEndpoint`can be viewed, created, updated, deleted by its owner.
`MonitoringResult` can be viewed by its owner. 

Authentication: via Bearer token in header (`Authorization: Bearer <token>`).


Data: JSON body.


Monitored interval value is in seconds.

## API endpoints
Note: `id` on last position means an actual actual id
* `GET:     api/MonitoredEndpoint`
* `GET:     api/MonitoredEndpoint/id` 
* `POST:    api/MonitoredEndpoint`
* `PUT:     api/MonitoredEndpoint/id` {only for updates}
* `DELETE:  api/MonitoredEndpoint/id`


* `GET: api/MonitoredResult` {last 10 results}
* `GET: api/MonitoredResult/id`
* `GET: api/MonitoredResult/ForEndpoind/id_endpoint` {last 10 results for endpoint}



## API examples
`MonitoredEndpoint` POST example:
```json
{
    "name": "my monitored url",
    "url": "www.url.com",
    "monitoredInterval": 100
}
```
`MonitoringResult` example:
```json
{
    "id": 563,
    "dateOfCheck": "2020-09-01T00:20:24",
    "returnedHttpStatusCode": 0,
    "returnedPayload":"Some payload...",
    "monitoredEndpointForeignKey": 1
}
```

## Prerequisites
* [.NET Core](https://dotnet.microsoft.com/download)
* [MySQL Server](https://www.mysql.com)

## How to run
* Clone the solution.
```console
git clone https://github.com/antonin-urban/endpoints-monitoring-service.git
````
* Go to service project folder.
```console
cd ./endpoints-monitoring-service/EndpointsMonitoringService
```
* Run with dotnet cmd. The project should build and run.
```console
dotnet run
```

* OR open the solution `./EndpointsMonitoringService.sln` with Visual Studio and run it in the IDE.
### Notes:
* Service root URL can be changed in file `./Properties/launchSettings.json`. Default is `http://localhost:5000`.
* Service is logging into terminal ([Serilog logger](https://serilog.net)). You should see output like this:
```Console
[2020-09-01 08:29:13.189][INF][EndpointsMonitoringService.Program] APP STARTED 
[08:29:14.102][INF][EndpointsMonitoringService.Program] Database EnsureCreated done
[08:29:14.349][INF][EndpointsMonitoringService.Services.MonitoringWorker] MonitoringWorker started
[08:29:19.352][INF][EndpointsMonitoringService.Services.MonitoringWorker] MonitoringWorker waking up...
...
```
* Logger filters and connection string can be edited in `appsettings.json` (Publish builds) and `appsettings.Development.json` (Debug, Release builds).
