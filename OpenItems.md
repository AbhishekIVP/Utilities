# Open Items

- [ ]    1. Dapr Traces
- [ ]    2. Dapr Metrics
- [ ]    3. Dapr Logs
- [x]    4. Options Pattern
- [ ]    5. Test Cases
- [x]    6. Add AWS/Azure Secrets
- [x]    7. Define proper Exception class
- [x]    8. Validations, [It should be based on AOP, but for now it is fine]

- [x] Dapr traces are not getting linked with Main service Call. [Redis Instance is needed at the startup because we need to bind Telemetry on it, so can not be traced as part of the service call]

- [x] Only 1 Redis connection can be traced, Either application or Locking or config

- [x] SecretsManager needs to be injected
  
- [x] Notification -> Pub/Sub
  
- [x] Add Logging all possible methods
- [ ] Add Multi tenancy, check TODO
- [ ] Add Database connectivity to locking mechanism [Should be optional]
- [ ] On failure of Notification Service, messages should be re-picked from the queue and processed, but it should have a timeout, after which the message should be dropped.
- [ ] attachment to the notification
- [ ] Send Notification to Browser using SignalR
- [ ] Add interfaces instead of classes in DI
- [x] Add Slack Integration
