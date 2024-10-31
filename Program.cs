using BioID.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add gRPC bws client
builder.Services.AddGrpcClient<BioIDWebService.BioIDWebServiceClient>(o =>
{
    // Configure grpc server endpoint from appsettings.json
    var gprcEndpoint = builder.Configuration.GetSection("BwsGrpcApiSettings")["Endpoint"] ?? throw new InvalidOperationException("The gRPC endpoint is not specified or is incorrect.");
    o.Address = new Uri(gprcEndpoint);
})
    .AddCallCredentials((context, metadata, serviceProvider) =>
    {
        // Generate JWT token 
        var key = builder.Configuration.GetSection("BwsGrpcApiSettings")["AccessKey"] ?? throw new InvalidOperationException("The grpc access key could not be found.");
        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
        var clientID = builder.Configuration.GetSection("BwsGrpcApiSettings")["ClientId"] ?? throw new InvalidOperationException("The grpc clientId could not be found.");
        List<Claim> claims = [new Claim(JwtRegisteredClaimNames.Sub, clientID)];
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




