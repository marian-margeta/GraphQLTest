using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphQLWeb
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // In memory SQLite
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            services.AddPooledDbContextFactory<MyDbContext>(x => x.UseSqlite(connection));

            // AutoMapper
            services.AddAutoMapper(Assembly.GetEntryAssembly());

            // GraphQL
            services.AddGraphQLServer()
                .AddQueryType<QueryType>()
                .AddType<FolderType>()
                .SetPagingOptions(new PagingOptions()
                {
                    IncludeTotalCount = true
                })
                .AddProjections()
                .AddFiltering()
                .AddSorting();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
            });
        }
    }

    public class FolderProfile : Profile
    {
        public FolderProfile()
        {
            CreateMap<File, FileDto>();
            CreateMap<Folder, FolderDto>();
        }
    }

    public class FolderType : ObjectType<Folder>
    {
        protected override void Configure(IObjectTypeDescriptor<Folder> descriptor)
        {
            descriptor.Field(x => x.Files)
                //.UseOffsetPaging()
                .UseFiltering()
                .UseSorting();
        }
    }

    public class Query
    {
        public IQueryable<Folder> GetFolders([ScopedService] MyDbContext myDbContext)
        {
            return myDbContext.Folder;
        }
    }

    public class QueryType : ObjectType<Query>
    {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor.Field(x => x.GetFolders(default))
                .UseDbContext<MyDbContext>()
                //.UseOffsetPaging()
                .UseProjection()
                .UseFiltering()
                .UseSorting();
        }
    }

    public class MyDbContext : DbContext
    {
        public DbSet<Folder> Folder { get; set; }
        public DbSet<File> File { get; set; }

        public MyDbContext(DbContextOptions options)
            : base(options)
        {

        }
    }

    public class Folder
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<File> Files { get; set; }
    }

    public class FolderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<FileDto> Files { get; set; }
    }

    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public Folder Folder { get; set; }
    }

    public class FileDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public FolderDto Folder { get; set; }
    }
}
