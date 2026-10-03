var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BadPetWeb>("padpetweb");

builder.Build().Run();
