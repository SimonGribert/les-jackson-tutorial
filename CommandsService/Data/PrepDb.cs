using System;
using System.Collections.Generic;
using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.Data
{
    public class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

                var platforms = grpcClient.ReturnAllPlatforms();

                SeedData(
                    serviceScope.ServiceProvider.GetService<ICommandRepo>(), 
                    serviceScope.ServiceProvider.GetService<AppDbContext>(),
                    platforms,
                    isProd);
            }
        }

        private static void SeedData(ICommandRepo repo, AppDbContext context, IEnumerable<Platform> platforms, bool isProd)
        {
            if (isProd)
            {
                Console.WriteLine("--> Attempting to apply migrations...");

                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }

            Console.WriteLine("Seeding new platforms...");
            
            foreach(var plat in platforms)
            {
                if (!repo.ExternalPlatformExist(plat.ExternalID))
                {
                    repo.CreatePlatform(plat);
                }

                repo.SaveChanges();
            }

        }
    }
}