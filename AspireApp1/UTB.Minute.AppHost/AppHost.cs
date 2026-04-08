var builder = DistributedApplication.CreateBuilder(args);

var sql = builder
    .AddSqlServer("sql-server");
    //.WithDataVolume()
    //.WithEnvironment("MSSQL_SA_PASSWORD", "Str0ngP@ssw0rd!")
    //.WithLifetime(ContainerLifetime.Persistent);

var database = sql.AddDatabase("database");

builder.AddProject<Projects.UTB_Minute_DbManager>("utb-minute-dbmanager")
    .WithReference(database)
    .WithHttpCommand("reset-db", "Reset Database")
    .WaitFor(database);

builder.AddProject<Projects.UTB_Minute_WebApi>("web-api").WithReference(database);

//var postgres = builder.AddPostgres("postgres")
//    .WithPgAdmin()
//    .WithDataVolume()
//    .WithLifetime(ContainerLifetime.Persistent);

//var database = postgres.AddDatabase("database");

builder.Build().Run();