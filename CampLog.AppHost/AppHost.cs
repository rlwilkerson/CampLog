var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();
var db = postgres.AddDatabase("camplogdb");

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithRealmImport("keycloak");

var api = builder.AddProject<Projects.CampLog_Api>("api")
    .WithReference(db).WaitFor(db)
    .WithReference(keycloak).WaitFor(keycloak);

builder.AddProject<Projects.CampLog_Web>("web")
    .WithReference(api).WaitFor(api)
    .WithReference(keycloak).WaitFor(keycloak);

builder.AddProject<Projects.CampLog_Web2>("web2")
    .WithReference(api).WaitFor(api)
    .WithReference(keycloak).WaitFor(keycloak);

builder.AddViteApp("web3", "../CampLog.Web3")
    .WithReference(api).WaitFor(api)
    .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("https"))
    .WithEnvironment("VITE_KEYCLOAK_URL", keycloak.GetEndpoint("http"))
    .PublishAsDockerFile();

builder.Build().Run();
