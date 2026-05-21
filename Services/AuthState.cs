using InvestTracker.Models;

namespace InvestTracker.Services;

public class AuthState
{
    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    // Pas d'événement OnChange → on n'en a plus besoin
    public void Login(User user)
    {
        CurrentUser = user;
    }

    public void Logout()
    {
        CurrentUser = null;
    }
}