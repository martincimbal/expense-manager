namespace expense_manager_30.Services;

public static class Session
{
    public static int CurrentUserId { get; set; }
    public static string Username { get; set; } = string.Empty;

    public static void SetUser(int id, string username)
    {
        CurrentUserId = id;
        Username = username;
    }

    public static void Clear()
    {
        CurrentUserId = 0;
        Username = string.Empty;
    }

    public static bool IsLoggedIn => CurrentUserId != 0;
}
