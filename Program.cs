using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static BioID.Services.BioIDWebService;

namespace BioID.BWS.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // App Configuration
            IConfiguration appConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(appConfiguration)
                .CreateLogger();
            try
            {
                Log.Information("Bws Webapp is starting up.");
                var builder = WebApplication.CreateBuilder(args);

                //Add support to logging with SERILOG
                builder.Services.AddSerilog();

                // Add gRPC bws client
                builder.Services.AddGrpcClient<BioIDWebServiceClient>(o =>
                {
                    // Configure grpc server endpoint from appsettings.json
                    o.Address = new Uri(appConfiguration["BwsGrpcApiSettings:Endpoint"]!);
                })
                    .AddCallCredentials((context, metadata, serviceProvider) =>
                    {
                        // Generate JWT token 
                        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(appConfiguration["BwsGrpcApiSettings:AccessKey"]!));
                        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
                        List<Claim> claims = [new Claim(JwtRegisteredClaimNames.Sub, appConfiguration["BwsGrpcApiSettings:ClientId"]!)];
                        var now = DateTime.UtcNow;
                        string token = new JwtSecurityTokenHandler().CreateEncodedJwt("demoWeApp", "bws", new ClaimsIdentity(claims), now, now.AddMinutes(10), now, credentials);
                        metadata.Add("Authorization", $"Bearer {token}");
                        return Task.CompletedTask;
                    });

                // Add services to the container.
                builder.Services.AddRazorPages();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                app.UseStaticFiles();

                app.UseRouting();

                app.MapRazorPages();

                app.Run();
            }
            catch (Exception ex )
            {
                Log.Fatal(ex, "The application failed to start.");
                Log.CloseAndFlush();
            }
            
        }
    }
}
