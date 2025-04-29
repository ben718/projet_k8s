using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Identity.Api.Messaging.Consumers;
using Identity.Api.Models;
using Identity.Api.Services;
using MassTransit;
using MassTransit.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using GreenPipes;

namespace Identity.Api
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

            //By connecting here we are making sure that our service
            //cannot start until redis is ready. This might slow down startup,
            //but given that there is a delay on resolving the ip address
            //and then creating the connection it seems reasonable to move
            //that cost to startup instead of having the first request pay the
            //penalty.
            services.AddSingleton(sp =>
            {
                var configuration = new ConfigurationOptions { ResolveDns = true };
                
                // Amélioration pour Kubernetes: gestion plus robuste de la configuration Redis
                string redisHost = Configuration["RedisHost"] ?? "dotnetgigs-redis";
                string redisConnectionString = Configuration["ConnectionString"];
                
                // Si ConnectionString n'est pas définie ou est vide, construire à partir de RedisHost
                if (string.IsNullOrEmpty(redisConnectionString))
                {
                    // Par défaut, utiliser le port 6379 si non spécifié
                    if (!redisHost.Contains(":"))
                    {
                        redisHost += ":6379";
                    }
                    redisConnectionString = redisHost;
                }
                
                Console.WriteLine($"Connecting to Redis at: {redisConnectionString}");
                
                try 
                {
                    configuration.EndPoints.Add(redisConnectionString);
                    return ConnectionMultiplexer.Connect(configuration);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to connect to Redis: {ex.Message}");
                    // En cas d'échec, essayer de revenir à une configuration minimale
                    var fallbackConfig = new ConfigurationOptions { ResolveDns = true };
                    fallbackConfig.EndPoints.Add("localhost:6379");
                    Console.WriteLine("Falling back to localhost:6379");
                    return ConnectionMultiplexer.Connect(fallbackConfig);
                }
            });

            services.AddTransient<IIdentityRepository, IdentityRepository>();
            var builder = new ContainerBuilder();

            // register a specific consumer
            builder.RegisterType<ApplicantAppliedEventConsumer>();

            builder.Register(context =>
                {
                    var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        // Amélioration pour Kubernetes: gestion plus robuste de la connexion RabbitMQ
                        string rabbitmqHost = Configuration["EventBusConnection"] ?? "dotnetgigs-rabbitmq";
                        Console.WriteLine($"Connecting to RabbitMQ at: {rabbitmqHost}");
                        
                        var host = cfg.Host(new Uri($"rabbitmq://{rabbitmqHost}/"), h =>
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
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            try
            {
                // stash an applicant's user data in redis for test purposes...this would simulate establishing auth/session in the real world
                var identityRepository = serviceProvider.GetService<IIdentityRepository>();
                if (identityRepository != null)
                {
                    await identityRepository.UpdateUserAsync(new User { Id = "1", Email = "josh903902@gmail.com", Name = "Josh Dillinger" });
                }
                else
                {
                    Console.WriteLine("Warning: Could not resolve IIdentityRepository");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
            }

            try 
            {
                var bus = ApplicationContainer.Resolve<IBusControl>();
                var busHandle = TaskUtil.Await(() => bus.StartAsync());
                lifetime.ApplicationStopping.Register(() => busHandle.Stop());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting message bus: {ex.Message}");
            }
        }
    }
}
