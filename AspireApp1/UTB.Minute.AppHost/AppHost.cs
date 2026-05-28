var builder = DistributedApplication.CreateBuilder(args);

var sql = builder
    .AddSqlServer("sql-server");

var database = sql.AddDatabase("database");

<<<<<<< Updated upstream
=======
var keycloak = builder.AddKeycloak("keycloak", 8080)
                      .WithContainerName("utb-minute-keycloak")
                      .WithDataVolume("utb-minute-keycloak-data")
                      .WithRealmImport("import")
                      .WithLifetime(ContainerLifetime.Persistent);

>>>>>>> Stashed changes

builder.AddProject<Projects.UTB_Minute_DbManager>("utb-minute-dbmanager")
    .WithReference(database)
    .WithHttpCommand("reset-db", "Reset Database")
    .WaitFor(database);

var api = builder.AddProject<Projects.UTB_Minute_WebApi>("web-api")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.UTB_Minute_AdminClient>("admin-client")
    .WithReference(api) 
    .WaitFor(api);

builder.AddProject<Projects.UTB_Minute_CanteenClient>("canteen-client")
    .WithReference(api) 
    .WaitFor(api);

builder.Build().Run();