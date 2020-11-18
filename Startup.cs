using System;
using System.IO;
using System.Reflection;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Xchangez.Clases;
using Xchangez.Interfaces;

namespace Xchangez
{
    public class Startup
    {
        /// <summary>
        /// Esto funciona para ver los comandos que se realizan a SQL en la consola (solo en desarrollo)
        /// </summary>
        public static readonly ILoggerFactory Logger = LoggerFactory.Create(configure =>
        {
            configure.AddFilter((category, level) =>
                category == DbLoggerCategory.Database.Name
                && level == LogLevel.Information
            ).AddConsole();
        });

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // --------------------------------------------------
            // configuración inicial autómatica (configuración por defecto ya incluida al crear un proyecto)
            // --------------------------------------------------
            services.AddControllers();

            // --------------------------------------------------
            // configurando sql con ef core (el dbcontext es para definir las tablas de la base de datos y ejecutar métodos CRUD)
            // --------------------------------------------------
            services.AddDbContext<XchangezContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString(Constantes.STR_CONNECTION_NAME));
                options.EnableSensitiveDataLogging(); // comandos que se ejecutan
                options.UseLoggerFactory(Logger); // para ver dichos comandos en la consola
            });

            // --------------------------------------------------
            // agregando swagger (nos sirve para ver todos los métodos de los controladores en una interfaz gráfica y poder consumirlos)
            // --------------------------------------------------
            AgregarSwagger(services);

            // --------------------------------------------------
            // implementando mapper https://gavilanch.wordpress.com/2019/03/18/asp-net-core-2-2-objetos-de-transferencia-de-datos-y-automapper/
            // Ver la clase Helpers/AutoMapperProfiles.cs, aqui se indican los mapeados de las clases abstractas a los DTO
            // --------------------------------------------------
            services.AddAutoMapper(typeof(Startup));

            // --------------------------------------------------
            // implementando repositorio generico (para obtener datos)
            // --------------------------------------------------
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

            // --------------------------------------------------
            // configurando cors
            // --------------------------------------------------
            services.AddCors(o =>
            {
                o.AddPolicy("AllowAllHeaders", b =>
                {
                    b.AllowAnyOrigin();
                    b.AllowAnyHeader();
                    b.AllowAnyMethod();
                });
            });

            // --------------------------------------------------
            // implementando clase para guardado de archivos
            // --------------------------------------------------
            services.AddTransient<IFile, SaveFile>();
            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // configurando swagger
            ConfigurarSwagger(app);

            // configurando cors
            app.UseCors("AllowAllHeaders");

            // --------------------------------------------------
            // configuración inicial autómatica (* se agrego manualmente)
            // --------------------------------------------------
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(); // * para poder acceder a las imagenes del servidor local

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthentication(); // * habilitamos la autenticacion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void AgregarSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc(Constantes.SWAGGER_VERSION, new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Version = Constantes.SWAGGER_VERSION,
                    Title = Constantes.SWAGGER_TITLE,
                    Description = Constantes.SWAGGER_DESCRIPTION
                });

                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                o.IncludeXmlComments(xmlPath);

                // se agrega autenticacion por tokens
                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Autorización encabezado usando el esquema Bearer. \r\n\r\n Ingresa 'Bearer' [space] y luego el token generado " +
                    "(el token se genera al logearse)." +
                    "\r\n\r\nEjemplo: \"Bearer aqui_va_el_token\"",
                });

                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // --------------------------------------------------
            // configurando tokens para autenticacion
            // --------------------------------------------------
            services.AddAuthentication(n =>
            {
                n.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                n.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(n =>
            {
                n.RequireHttpsMetadata = false;
                n.SaveToken = true;
                n.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration[Constantes.JWT_SECRETKEY_NAME])),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        private void ConfigurarSwagger(IApplicationBuilder app)
        {
            app.UseSwagger();

            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint(Constantes.SWAGGER_ENDP_URL_DEFAULT, Constantes.SWAGGER_ENDP_NAME_DEFAULT);
            });
        }
    }
}
