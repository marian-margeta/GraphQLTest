using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace GraphQLWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            InitializeDatabase(host);
            host.Run();
        }

        private static void InitializeDatabase(IHost host)
        {
            using var sc = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var sp = sc.ServiceProvider;
            var ctx = sp.GetRequiredService<IDbContextFactory<MyDbContext>>().CreateDbContext();
            ctx.Database.Migrate();

            ctx.Folder.Add(new Folder()
            {
                Name = "xxx",
                Description = "descr",
                Files = new List<File> {
                    new File() {
                        Name = "zzz",
                        Text = "asdasd asdasd"
                    }
                }
            });
            ctx.SaveChanges();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
