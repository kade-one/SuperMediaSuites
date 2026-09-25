namespace SuperMediaSuites.Services;

/// <summary>
/// Tiny static service container. Avoids pulling in a full DI framework for an app this size;
/// the services here are process-singletons shared by every tab.
/// </summary>
public static class ServiceLocator
{
    public static AppSettings Settings { get; }

    static ServiceLocator()
    {
        Settings = AppSettings.Load();
    }
}
