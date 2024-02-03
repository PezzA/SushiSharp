using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace SushiSharp.Web.Hubs;

public class IdentityService : IDisposable
{
    private AuthenticationStateProvider _authenticationStateProvider;

    public event EventHandler<IdentityChangedEventArgs>? IdentityChanged;

    public IdentityService(AuthenticationStateProvider authenticationStateProvider)
    {
        // The dependancy container will inject it's AuthenticationStateProvider instance here
        _authenticationStateProvider = authenticationStateProvider;
        // register for the state changed event
        _authenticationStateProvider.AuthenticationStateChanged += NotifyIdentityChanged;
    }

    // Method to get the current Identity
    public async ValueTask<ClaimsPrincipal> GetCurrentIdentity()
    {
        AuthenticationState state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return state.User;
    }

    // Fire and forget async event handler that waits on the Task completion before invoking the Event
    private async void NotifyIdentityChanged(Task<AuthenticationState> newAuthStateTask)
    {
        AuthenticationState state = await newAuthStateTask;
        this.IdentityChanged?.Invoke(null, new() { Identity = state.User});
    }

    // de-register for the state changed event
    public void Dispose()
        => _authenticationStateProvider.AuthenticationStateChanged -= NotifyIdentityChanged;
}

// Custom Event Args class to pass the new identity
public class IdentityChangedEventArgs : EventArgs
{
    public ClaimsPrincipal Identity { get; init; } = new ClaimsPrincipal();
}