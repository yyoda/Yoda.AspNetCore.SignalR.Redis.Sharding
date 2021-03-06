﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Yoda.AspNetCore.SignalR.Redis.Sharding;

namespace SignalR.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR()
                .UseShardingRedis(options =>
                {
                    foreach (var configuration in RedisServer.Instance.Configurations)
                    {
                        options.Add(configuration.Options, configuration.IsDedicatedForAllChannel);
                    }
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSignalR(routes =>
            {
                routes.MapHub<EchoHub>("/echoHub");
            });
            app.Map("/healthcheck", current =>
            {
                current.Use((context, next) => context.Response.WriteAsync("OK"));
            });
            app.UseMvc();
        }
    }
}
