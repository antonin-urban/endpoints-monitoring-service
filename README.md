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

## API Endpoints
Note: `id` on last position means an actual actual id
* `GET: api/MonitoredEndpoint`
* `GET: api/MonitoredEndpoint/id` 
* `POST: api/MonitoredEndpoint`
* `PUT: api/MonitoredEndpoint/id` {only for updates}
* `DELETE: api/MonitoredEndpoint/id`


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
* .NET Core
* MySQL Server

## How to run
* Pull the solution, go to folder with project `./EndpointsMonitoringService/EndpointsMonitoringService.csproj`, run `dotnet run`. The project should build and run.
* OR open the solution `./EndpointsMonitoringService.sln` with Visual Studio.


* Service is logging into terminal (Serilog logger). 
* Logger filters and connection string can be edited in `appsettings.json` and `appsettings.Development.json`.






