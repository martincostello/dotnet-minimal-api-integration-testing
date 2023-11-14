namespace TodoApp;

public sealed class BrowserStackLocalOptions
{
    //// See https://www.browserstack.com/local-testing/binary-params

    public string? LocalIdentifier { get; set; }

    public string? ProxyHostName { get; set; }

    public string? ProxyPassword { get; set; }

    public int? ProxyPort { get; set; }

    public string? ProxyUserName { get; set; }

    internal static IList<string> BuildCommandLine(string apiKey, BrowserStackLocalOptions? options)
    {
        ArgumentNullException.ThrowIfNull(apiKey);

        List<string> arguments =
        [
            "--key",
            apiKey,
            "--only-automate"
        ];

        if (!string.IsNullOrWhiteSpace(options?.LocalIdentifier))
        {
            arguments.Add("--local-identifier");
            arguments.Add(options.LocalIdentifier);
        }

        if (!string.IsNullOrWhiteSpace(options?.ProxyHostName))
        {
            if (!options.ProxyPort.HasValue)
            {
                throw new ArgumentException("No proxy port number specified.", nameof(options));
            }

            arguments.Add("--proxy-host");
            arguments.Add( options.ProxyHostName);
            arguments.Add("--proxy-port");
            arguments.Add(options.ProxyPort.Value.ToString(CultureInfo.InvariantCulture));

            if (!string.IsNullOrWhiteSpace(options.ProxyUserName))
            {
                if (string.IsNullOrWhiteSpace(options.ProxyPassword))
                {
                    throw new ArgumentException("No proxy password specified.", nameof(options));
                }

                arguments.Add("--proxy-user");
                arguments.Add(options.ProxyUserName);
                arguments.Add("--proxy-pass");
                arguments.Add(options.ProxyPassword);
            }
        }

        return arguments;
    }
}
