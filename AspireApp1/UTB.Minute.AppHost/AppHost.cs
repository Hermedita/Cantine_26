var builder = DistributedApplication.CreateBuilder(args);

var sql = builder
    .AddSqlServer("sql-server");

var database = sql.AddDatabase("database");

var keycloak = builder.AddKeycloak("keycloak", 8080)
                      .WithRealmImport("import")
                      .WithContainerName("utb-minute-keycloak")
                      .WithDataVolume("utb-minute-keycloak-data")
                      .WithLifetime(ContainerLifetime.Persistent);


builder.AddProject<Projects.UTB_Minute_DbManager>("utb-minute-dbmanager")
    .WithReference(database)
    .WithHttpCommand("reset-db", "Reset Database")
    .WaitFor(database);

var api = builder.AddProject<Projects.UTB_Minute_WebApi>("web-api")
    .WithReference(database)
    .WithReference(keycloak)
    .WaitFor(database)
    .WaitFor(keycloak);

builder.AddProject<Projects.UTB_Minute_AdminClient>("admin-client")
    .WithReference(api) 
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WaitFor(api);

builder.AddProject<Projects.UTB_Minute_CanteenClient>("canteen-client")
    .WithReference(api)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WaitFor(api);

builder.Build().Run();