using System.Globalization;

public class CultureHttpMessageHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var culture =
            CultureInfo.CurrentUICulture ??
            CultureInfo.DefaultThreadCurrentUICulture ??
            CultureInfo.CurrentCulture ??
            new CultureInfo("en");

        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.ParseAdd(culture.Name);

        Console.WriteLine($"[HttpHandler] Forwarding Accept-Language: {culture.Name} (UI={CultureInfo.CurrentUICulture?.Name}, Thread={CultureInfo.DefaultThreadCurrentUICulture?.Name})");

        return base.SendAsync(request, cancellationToken);
    }
}
