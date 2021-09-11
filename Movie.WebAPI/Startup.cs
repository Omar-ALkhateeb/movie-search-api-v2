using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Movie.Base;
using Movie.CronJobScheduler;
using Movie.DB;
using Movie.ElasticSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public async void ConfigureServices(IServiceCollection services)
        {
            // elastic search setup
            var esClient = new ES().Get();

            // litedb setup
            DBC db = new DBC();
            var col = db.Create();

            // seeding on start
            if (MovieEntity.IsEmpty(col))
            {
                Console.WriteLine("seeding...");
                MovieEntity.SeedDB(col);
                MovieEntity.SeedSearch(esClient, col);
            }

            // cronjob setup
            CronJob sched = new CronJob();
            await sched.StartSchedulerAsync();
            await sched.CreateJob(esClient, col);

            // cors
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("*");
                    });
            });

            services.AddSingleton(esClient);
            services.AddSingleton(col);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Movie.WebAPI", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Movie.WebAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
