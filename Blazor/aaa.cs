using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;

namespace Blazor
{
    public class AuthenticationService
    {
        public event Action<ClaimsPrincipal>? UserChanged;
        private ClaimsPrincipal? currentUser;

        public string? accesstoken { get; set; }

        public string? refreshtoken { get; set; }
        public ClaimsPrincipal CurrentUser
        {
            get { return currentUser ?? new(); }
            set
            {
                currentUser = value;

                if (UserChanged is not null)
                {
                    UserChanged(currentUser);
                }
            }
        }
    }

    public class CustomClaimsPrincipal  : ClaimsPrincipal
    {
        public CustomClaimsPrincipal(IIdentity identity) : base(identity)
        {
        }

    }


    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState authenticationState;

        public CustomAuthenticationStateProvider(AuthenticationService service)
        {
            authenticationState = new AuthenticationState(service.CurrentUser);

            service.UserChanged += (newUser) =>
            {
                authenticationState = new AuthenticationState(newUser);

                NotifyAuthenticationStateChanged(
                    Task.FromResult(new AuthenticationState(newUser)));
            };
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(authenticationState);
    }

    public class TokenProvider
    {
        public string? accesstoken { get; set; }

        public string? refreshtoken { get; set; }
    }

    public class UserProfile
    {
        public string? UserName { get; set; }
        //other claims....
    }

    public static class ApiAddress
    {
        public static Uri Uri => new Uri("https://localhost:32777/");
    }


    public interface IWeatherService
    {
        Task<WeatherForecast[]> GetWeather();
    }

    public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    public class AnotherClientWeatherService(HttpClient client, AuthenticationService authenticationService) : IWeatherService
    {
        public async Task<WeatherForecast[]> GetWeather()
        {
            if (authenticationService.accesstoken is null)
            {
                return [];
            }
            else
                try
                {
                    client.BaseAddress = ApiAddress.Uri;
                    var httpMessage = new HttpRequestMessage();
                    httpMessage.Method = HttpMethod.Get;
                    httpMessage.RequestUri = new Uri("weatherforecast", uriKind: UriKind.Relative);
                    httpMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationService.accesstoken);

                    var response = await client.SendAsync(httpMessage);
                    var content = response.Content;
                    var forecast = await content.ReadFromJsonAsync<WeatherForecast[]>();
                    return forecast ?? [];
                }
                catch (Exception e)
                {
                    return [];
                }
        }
    }
    public class ClientWeatherService(IHttpClientFactory factory, AuthenticationService authenticationService) : IWeatherService
    {
        public async Task<WeatherForecast[]> GetWeather()
        {
            if (authenticationService.accesstoken is null)
            {
                return [];
            }
            else
                try
                {
                    using var http = factory.CreateClient("std");
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationService.accesstoken);

                    return await http.GetFromJsonAsync<WeatherForecast[]>("weatherforecast") ?? [];
                }
                catch (Exception e)
                {
                    return [];
                }
        }
    }
}
