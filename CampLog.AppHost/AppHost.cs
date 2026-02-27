var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();
var db = postgres.AddDatabase("camplogdb");

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithDataVolume()
    .WithRealmImport("keycloak");

var api = builder.AddProject<Projects.CampLog_Api>("api")
    .WithReference(db).WaitFor(db)
    .WithReference(keycloak).WaitFor(keycloak);

builder.AddProject<Projects.CampLog_Web>("web")
    .WithReference(api).WaitFor(api)
    .WithReference(keycloak).WaitFor(keycloak);

builder.Build().Run();
