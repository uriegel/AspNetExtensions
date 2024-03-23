using System.Security.Cryptography.X509Certificates;
using CsTools.Extensions;

using static CsTools.WithLogging;
using static CsTools.Functional.Memoization;
using Microsoft.AspNetCore.Server.Kestrel.Https;

namespace AspNetExtensions;

public static class LetsEncrypt
{
    public static WebApplicationWithHost UseLetsEncryptValidation(this WebApplicationWithHost app)
        => app.SideEffect(_ => app.WithMapGet("/.well-known/acme-challenge/{secret}", 
                                                (string secret) => GetFileContent($"{secret}")));

    public const string LETS_ENCRYPT_DIR = "LETS_ENCRYPT_DIR";

    /// <summary>
    /// Resetter has to be existent, otherwise MemoizeMaybe will be called with null!
    /// </summary>
    static Resetter Resetter { get; } = new Resetter();

    public static Func<X509Certificate2?> Get { get; } = MemoizeMaybe(InitCertificate, Resetter);

    public static void Use(HttpsConnectionAdapterOptions options)
        => options.ServerCertificateSelector = (_, __) => Get();

    static X509Certificate2? InitCertificate()
        => GetEnvironmentVariable(LETS_ENCRYPT_DIR)
            ?.AppendPath("certificate.pfx")
            ?.ReadCertificate()
            ?.SideEffect(_ => StartCertificateTimer());

    static string InitGetPfxPassword()
        => (OperatingSystem.IsLinux()
            ? "/etc"
            : Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))
            ?.AppendPath("letsencrypt-uweb")
            ?.ReadAllTextFromFilePath()
            ?.Trim() 
            ?? "".SideEffect(_ => Console.WriteLine("!!!NO PASSWORD!!"));

    /// <summary>
    /// Reads Pfx password from local file
    /// </summary>
    public static Func<string> GetPfxPassword { get; } = Memoize(InitGetPfxPassword);

    static X509Certificate2 ReadCertificate(this string fileName)
        => new(fileName, GetPfxPassword());

    static readonly Func<string, string?> GetFileContent = name =>
        name == "check"
        ? "checked"
        : GetEnvironmentVariable(LETS_ENCRYPT_DIR)
            ?.AppendPath(name)
            ?.ReadAllTextFromFilePath();

    static void StartCertificateTimer()
        => certificateResetter ??= new(_ => Resetter.Reset(),
                null,
                TimeSpan.FromDays(1),
                TimeSpan.FromDays(1));
        
    static Timer? certificateResetter;
}

