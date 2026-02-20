var builder = DistributedApplication.CreateBuilder(args);

var sql = builder
    .AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = sql.AddDatabase("database");

builder.AddProject<Projects.UTB_Minute_DbManager>("utb-minute-dbmanager")
    .WithReference(database)
    .WithHttpCommand("reset-db", "Reset Database")
    .WaitFor(database);

//var postgres = builder.AddPostgres("postgres")
//    .WithPgAdmin()
//    .WithDataVolume()
//    .WithLifetime(ContainerLifetime.Persistent);

//var database = postgres.AddDatabase("database");

builder.Build().Run();