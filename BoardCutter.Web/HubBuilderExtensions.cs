using System.Net;

using Microsoft.AspNetCore.SignalR.Client;

namespace BoardCutter.Web;

public static class HubBuilderExtensions
{
    public static HubConnection CreateHub(Uri hubUri, IRequestCookieCollection? cookies, Dictionary<string, string>? propCookies)
    {
        return new HubConnectionBuilder()
            .WithUrl(hubUri, options =>
            {
                if (propCookies == null)
                {
                    propCookies = new Dictionary<string, string>();

                    if (cookies?.Any() == true)
                    {
                        foreach (var cookie in cookies)
                        {
                            propCookies.Add(cookie.Key, cookie.Value);
                        }
                    }
                }

                options.UseDefaultCredentials = true;
                var cookieCount = propCookies.Count;
                var cookieContainer = new CookieContainer(cookieCount);
                foreach (var cookie in propCookies)
                    cookieContainer.Add(new Cookie(
                        cookie.Key,
                        WebUtility.UrlEncode(cookie.Value),
                        path: "/",
                        domain: hubUri.Host));
                options.Cookies = cookieContainer;

                foreach (var header in propCookies)
                    options.Headers.Add(header.Key, header.Value);

                options.HttpMessageHandlerFactory = _ =>
                {
                    var clientHandler = new HttpClientHandler
                    {
                        PreAuthenticate = true,
                        CookieContainer = cookieContainer,
                        UseCookies = true,
                        UseDefaultCredentials = true,
                    };
                    return clientHandler;
                };
            })
            .Build();
    }
}