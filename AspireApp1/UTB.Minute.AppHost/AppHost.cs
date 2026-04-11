var builder = DistributedApplication.CreateBuilder(args);

var sql = builder
    .AddSqlServer("sql-server");

var database = sql.AddDatabase("database");

builder.AddProject<Projects.UTB_Minute_DbManager>("utb-minute-dbmanager")
    .WithReference(database)
    .WithHttpCommand("reset-db", "Reset Database")
    .WaitFor(database);

builder.AddProject<Projects.UTB_Minute_WebApi>("web-api").WithReference(database);


builder.Build().Run();