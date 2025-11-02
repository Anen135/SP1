using Microsoft.Extensions.Logging;
using SP1.Models;
using SQLite;
namespace SP1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        DatabaseService.Init(Path.Combine(FileSystem.AppDataDirectory, "lolkek"));
        return builder.Build();
    }
}

public static class DatabaseService
{
    public static SQLiteConnection Connection;
    public static Models.Manager CurrentUser = null;
    public static Dictionary<Type, Func<List<EntityBase>>> foreignMap = new();
    public static void Init(string ConnectionString)
    {
        Connection = new SQLiteConnection(ConnectionString);
        Connection.CreateTables<Models.Manager, Models.Client, Models.Trade>();
        foreignMap[typeof(Models.Trade)] = () => Connection.Table<Models.Trade>().Cast<Models.EntityBase>().ToList();
        foreignMap[typeof(Models.Manager)] = () => Connection.Table<Models.Manager>().Cast<Models.EntityBase>().ToList();
        foreignMap[typeof(Models.Client)] = () => Connection.Table<Models.Client>().Cast<Models.EntityBase>().ToList();
    }
}

public static class AccessControl
{
    public static Models.ManagerRole? Role = null;
    public static bool CanCreate<T>()
    {
        if (Role == null) return false;
        if (typeof(T) == typeof(Models.Manager)) return Role == Models.ManagerRole.Admin;
        return Role == Models.ManagerRole.Admin || Role == Models.ManagerRole.Manager;
    }
    public static bool CanDelete<T>()
    {
        if (Role == null) return false;
        if (typeof(T) == typeof(Models.Manager)) return Role == Models.ManagerRole.Admin;
        return Role == Models.ManagerRole.Admin || Role == Models.ManagerRole.Manager;
    }
    public static bool CanEdit<T>()
    {
        if (Role == null) return false;
        if (typeof(T) == typeof(Models.Manager)) return Role == Models.ManagerRole.Admin;
        return Role == Models.ManagerRole.Admin || Role == Models.ManagerRole.Manager;
    }
    public static bool CanView<T>()
    {
        if (Role == null) return false;
        return true;
    }
    public static bool CanOpen<T>()
    {
        if (Role == null) return false;
        if (typeof(T) == typeof(Models.Manager)) return Role == Models.ManagerRole.Admin;
        return Role == Models.ManagerRole.Admin || Role == Models.ManagerRole.Manager;
    }
}