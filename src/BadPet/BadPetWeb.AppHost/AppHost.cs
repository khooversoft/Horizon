var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BadPetWeb>("padpetweb");

builder.AddProject<Projects.CivillyInsaneWeb>("civillyinsaneweb");

builder.Build().Run();
