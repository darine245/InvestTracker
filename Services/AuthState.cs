using InvestTracker.Models;

namespace InvestTracker.Services;

public class AuthState
{
    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    // Notifie le NavMenu quand l'état change (login / logout)
    public event Action? OnChange;


    public void Login(User user)
    {
        CurrentUser = user;
        OnChange?.Invoke();

    }

    public void Logout()
    {
        CurrentUser = null;
        OnChange?.Invoke();

    }
}