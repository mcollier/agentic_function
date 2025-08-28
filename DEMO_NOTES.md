# My Demo Notes

## Aspire Demo
1. Expand AppHost project
2. Show AppHost.cs
3. Show appsettings.Development.json
4. Azure Function project - no local.settings.json
5. API project - no appsettings.json
6. Aspire handles spinning up:
    1. Azurite storage emulator
    2. Durable Task Scheduler emulator
    3. ASP.NET API project
    4. Azure Function project
7. Aspire uses enviornment variable convention to pass references to my Function project

## Full Workflow Demo

1. Open Function project
2. Walk through the ClaimOrchestrator class
   1. Show branching logic
3. Start Aspire AppHost - RUN

### Simple claim

1. Show sample policies (samples/policies)
   1. P-12345
2. Sample 1 from .http file
   1. Simple example showing all required fields, no claim history, and covered vehicle.
3. Show Aspire dashboard traces
4. Show email received

### Uncovered vehicle

1. Again showing P-12345
2. Sample 2 from .http file

### Invalid sample - missing required fields

1. Sample 3 from .http
2. Show file in blob storage

### High fraud probability sample

1. Show 4 from .http 
2. Open Durable Task Scheduler
3. Show workflow is paused by timmer
4. Raise Event - `FraudReviewCompleted` w/ `true`
5. Show file in blob storage
6. Show email received

