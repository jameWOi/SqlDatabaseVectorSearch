using System.Reflection.Metadata;

var builder = DistributedApplication.CreateBuilder(args);

//var sql = builder.AddSqlServer("Sql")
    //.WithLifetime(ContainerLifetime.Persistent);

//var db = sql.AddDatabase("database");

var password = builder.AddParameter("password", "Password123", secret: true);

var sql = builder.AddSqlServer("sql", password)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDockerfile(@".\", "sql2025.docker")
    .WithHostPort(14333);

var db = sql
    .WithDataVolume()
    .AddDatabase("Db");

builder.AddProject<Projects.SqlDatabaseVectorSearch>("sqldatabasevectorsearch")
    .WithReference(db)
    .WithEnvironment("ConnectionStrings__SqlConnection", db)
    .WaitFor(db);

builder.Build().Run();
