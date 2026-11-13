using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

namespace VetControl.Hosting;

public static class DeploymentSetup
{
    public static void AddDeploymentDefaults(this WebApplicationBuilder builder, string applicationName)
    {
        var identity = applicationName == "VetControl.Identity";
        if (!builder.Environment.IsDevelopment())
        {
            foreach (var key in identity
                ? new[] { "Frontend:Origin", "OpenId:Issuer", "OpenId:RedirectUri", "OpenId:PostLogoutRedirectUri" }
                : new[] { "Authentication:FrontendOrigin", "Authentication:Authority" })
            {
                if (!Uri.TryCreate(builder.Configuration[key], UriKind.Absolute, out var uri) ||
                    uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo))
                    throw new InvalidOperationException($"Configure {key} com uma URL HTTPS válida para produção.");
            }
            var secretKey = identity ? "OpenId:ClientSecret" : "Authentication:ClientSecret";
            var secret = builder.Configuration[secretKey];
            if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32 || secret.Contains("development", StringComparison.OrdinalIgnoreCase) ||
                secret.StartsWith("REPLACE_", StringComparison.Ordinal))
                throw new InvalidOperationException($"Configure {secretKey} com um segredo exclusivo de produção de pelo menos 32 caracteres.");
        }

        var directory = builder.Configuration["DataProtection:KeysPath"];
        if (string.IsNullOrWhiteSpace(directory)) directory = Path.Combine(builder.Environment.ContentRootPath, "data-protection-keys");
        var protection = builder.Services.AddDataProtection().SetApplicationName(applicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(directory)));
        if (!builder.Environment.IsDevelopment())
        {
            var certificate = builder.Configuration["DataProtection:CertificatePath"];
            if (string.IsNullOrWhiteSpace(certificate))
                throw new InvalidOperationException("Configure DataProtection:CertificatePath para proteger as chaves persistidas em produção.");
            var loaded = X509CertificateLoader.LoadPkcs12FromFile(certificate, builder.Configuration["DataProtection:CertificatePassword"], X509KeyStorageFlags.EphemeralKeySet);
            if (!loaded.HasPrivateKey) throw new InvalidOperationException("O certificado de Data Protection precisa conter a chave privada.");
            protection.ProtectKeysWithCertificate(loaded);
        }

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.ForwardLimit = 1;
            foreach (var value in builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
            {
                if (!IPAddress.TryParse(value, out var address)) throw new InvalidOperationException("Endereço de proxy inválido.");
                options.KnownProxies.Add(address);
            }
        });
        builder.Services.AddHttpsRedirection(options => options.HttpsPort = 443);
    }

    public static void UseDeploymentDefaults(this WebApplication app)
    {
        app.UseForwardedHeaders();
        if (!app.Environment.IsDevelopment()) { app.UseHsts(); app.UseHttpsRedirection(); }
    }
}
