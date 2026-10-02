var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.HorizonWeb>("horizonweb");

builder.Build().Run();
