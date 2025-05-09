using System;
using Applicants.Api.Messaging.Consumers;
using MassTransit.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Applicants.Api.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MassTransit;
using GreenPipes;

namespace Applicants.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IContainer ApplicationContainer { get; private set; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddScoped<IApplicantRepository>(c => new ApplicantRepository(Configuration["ConnectionString"]));
            //"Server=sql.data;User=sa;Password=Pass@word;Database=dotnetgigs.applicants"

            var builder = new ContainerBuilder();

            // register a specific consumer
            builder.RegisterType<ApplicantAppliedEventConsumer>();

            builder.Register(context =>
                {
                    var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        var host = cfg.Host(new Uri($"rabbitmq://{Configuration["EventBusConnection"]}/"), h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.UseRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1)));
                        cfg.ReceiveEndpoint("dotnetgigs" + Guid.NewGuid().ToString(), e =>
                        {
                            e.LoadFrom(context);
                        });
                    });

                    return busControl;
                })
                .SingleInstance()
                .As<IBusControl>()
                .As<IBus>();

            builder.Populate(services);
            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            var bus = ApplicationContainer.Resolve<IBusControl>();
            var busHandle = TaskUtil.Await(() => bus.StartAsync());
            lifetime.ApplicationStopping.Register(() => busHandle.Stop());
        }
    }
}
