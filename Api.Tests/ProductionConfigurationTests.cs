extern alias VetApi;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VetControl.Testing;
using Xunit;
using DeploymentSetup = VetApi::VetControl.Hosting.DeploymentSetup;

namespace Api.Tests;

public sealed class ProductionConfigurationTests
{
    [Fact]
    public async Task HttpsProxyTemplateAcceptsBothDomainsAndCertificates()
    {
        using var directory = new TemporaryKeyDirectory();
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=clinic.example.test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        foreach (var name in new[] { "app", "auth" })
        {
            await File.WriteAllTextAsync(Path.Combine(directory.Path, name + ".crt"), certificate.ExportCertificatePem());
            await File.WriteAllTextAsync(Path.Combine(directory.Path, name + ".key"), rsa.ExportPkcs8PrivateKeyPem());
        }
        var root = BackupTool.FindRoot();
        var result = await BackupTool.Run("docker", ["run", "--rm", "--add-host", "api:127.0.0.1", "--add-host", "auth:127.0.0.1",
            "-e", "APP_DOMAIN=clinic.example.test", "-e", "AUTH_DOMAIN=identity.example.test",
            "--mount", $"type=bind,source={Path.Combine(root, "deployment", "nginx")},target=/etc/nginx/templates,readonly",
            "--mount", $"type=bind,source={directory.Path},target=/etc/nginx/certificates,readonly",
            "nginx:1.28-alpine", "nginx", "-t"], root);
        Assert.True(result.Exit == 0, result.Output);
        Assert.Contains("test is successful", result.Output);
    }

    [Theory]
    [InlineData("Authentication:Authority", "http://identity.example.test")]
    [InlineData("Authentication:FrontendOrigin", "http://clinic.example.test")]
    [InlineData("Authentication:ClientSecret", "vet-control-development-secret-change-in-production")]
    [InlineData("Authentication:ClientSecret", "short")]
    [InlineData("Authentication:ClientSecret", "REPLACE_WITH_A_PRODUCTION_SECRET_OF_32_CHARACTERS")]
    public void ProductionRejectsUnsafeDefaults(string key, string value)
    {
        using var directory = new TemporaryKeyDirectory();
        var builder = Builder(directory.Path);
        builder.Configuration[key] = value;
        Assert.Throws<InvalidOperationException>(() => DeploymentSetup.AddDeploymentDefaults(builder, "VetControl"));
    }

    [Fact]
    public void ProductionRequiresCertificateAndEncryptsPersistedKeys()
    {
        using var directory = new TemporaryKeyDirectory();
        var missing = Builder(directory.Path);
        Assert.Throws<InvalidOperationException>(() => DeploymentSetup.AddDeploymentDefaults(missing, "VetControl"));
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=VetControl integration test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        var path = Path.Combine(directory.Path, "test-only.pfx");
        File.WriteAllBytes(path, certificate.Export(X509ContentType.Pfx, "test-password"));
        var builder = Builder(directory.Path);
        builder.Configuration["DataProtection:CertificatePath"] = path;
        builder.Configuration["DataProtection:CertificatePassword"] = "test-password";
        DeploymentSetup.AddDeploymentDefaults(builder, "VetControl");
        using var services = builder.Services.BuildServiceProvider();
        var protector = services.GetRequiredService<IDataProtectionProvider>().CreateProtector("Production test");
        var protectedValue = protector.Protect("test-sensitive-content");
        Assert.Equal("test-sensitive-content", protector.Unprotect(protectedValue));
        var key = Assert.Single(Directory.GetFiles(directory.Path, "key-*.xml"));
        var xml = File.ReadAllText(key);
        Assert.Contains("encryptedSecret", xml);
        Assert.DoesNotContain("<masterKey", xml);
        var restarted = Builder(directory.Path);
        restarted.Configuration["DataProtection:CertificatePath"] = path;
        restarted.Configuration["DataProtection:CertificatePassword"] = "test-password";
        DeploymentSetup.AddDeploymentDefaults(restarted, "VetControl");
        using var restartedServices = restarted.Services.BuildServiceProvider();
        Assert.Equal("test-sensitive-content", restartedServices.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("Production test").Unprotect(protectedValue));
    }

    private static WebApplicationBuilder Builder(string directory)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        { EnvironmentName = "Production", ContentRootPath = directory });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:Authority"] = "https://identity.example.test/",
            ["Authentication:FrontendOrigin"] = "https://clinic.example.test",
            ["Authentication:ClientSecret"] = "test-only-random-like-secret-at-least-32-characters",
            ["DataProtection:KeysPath"] = directory
        });
        return builder;
    }
}
