namespace SP1.Pages;

public class Dashboard : ContentPage
{
    public Dashboard()
    {
        Title = "Dashboard";
        var Entity = DatabaseService.Connection.Find<Models.Manager>(DatabaseService.CurrentUser.Id);
        var Trades = DatabaseService.Connection.Table<Models.Trade>().Where(t => t.Manager == Entity.Id).ToList();
        if (Entity != null)
        {
            Content = new HorizontalStackLayout
            {
                Spacing = 9,
                Children =
                    {
                        new Components.EntityListView<Models.Trade>(Trades),
                        new VerticalStackLayout
                        {
                            new Label
                            {
                                Text = $"Welcome, {Entity.Name}!",
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                Text = $"Your role: {Entity.Role}",
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                Text = $"Your ID: {Entity.Id}",
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                Text = $"Your Password: {Entity.Password}",
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                Text = $"Your Description: {Entity.Description}",
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            },
                        },
                    }
            };
        }
    }
}
