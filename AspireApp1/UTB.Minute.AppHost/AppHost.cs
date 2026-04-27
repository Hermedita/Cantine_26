var builder = DistributedApplication.CreateBuilder(args);

var sql = builder
    .AddSqlServer("sql-server");

var database = sql.AddDatabase("database");

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

builder.Build().Run();