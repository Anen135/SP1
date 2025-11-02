namespace SP1;

public class AppShell : Shell
{
    private TabBar tabbar;
    public AppShell()
    {
        tabbar = new TabBar();
        var managerTab = new Tab { Title = "Managers" };
        managerTab.Items.Add(new ShellContent
        {
            Title = "ManagersList",
            Route = "ManagersListRoute",
            ContentTemplate = new DataTemplate(() => new Pages.EntityListPage<Models.Manager>())
        });
        var clientTab = new Tab { Title = "Clients" };
        clientTab.Items.Add(new ShellContent
        {
            Title = "ClientsList",
            Route = "ClientsListRoute",
            ContentTemplate = new DataTemplate(() => new Pages.EntityListPage<Models.Client>())
        });
        var tradeTab = new Tab { Title = "Trades" };
        tradeTab.Items.Add(new ShellContent
        {
            Title = "TradeList",
            Route = "TradeListRoute",
            ContentTemplate = new DataTemplate(() => new Pages.EntityListPage<Models.Trade>())
        });
        var dashboardTab = new Tab { Title = "Dashboard" };
        dashboardTab.Items.Add(new ShellContent
        {
            Title = "Dashboard",
            Route = "DashboardRoute",
            ContentTemplate = new DataTemplate(() => new Pages.Dashboard())
        });
        tabbar.Items.Add(managerTab);
        tabbar.Items.Add(clientTab);
        tabbar.Items.Add(tradeTab);
        tabbar.Items.Add(dashboardTab);
        showLogin();
    }

    public void showLogin()
    {
        Items.Clear();
        Items.Add(new ShellContent
        {
            Title = "Login",
            Route = "LoginRoute",
            ContentTemplate = new DataTemplate(() => new Pages.LoginPage())
        });
    }

    public void showControls()
    {
        Items.Clear();
        Items.Add(tabbar);
    }
}
