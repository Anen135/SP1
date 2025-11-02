using SP1.Models;
using SP1.Components;

namespace SP1.Pages;

public class Dashboard : ContentPage
{
    private Manager Entity;
    private CollectionView TradeCV;
    private Picker StatusPicker;

    public Dashboard()
    {
        Title = "Dashboard";
        Entity = DatabaseService.Connection.Find<Manager>(DatabaseService.CurrentUser.Id);
        if (Entity == null)
        {
            Content = new Label
            {
                Text = "Manager not found.",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            return;
        }
        var trades = DatabaseService.Connection.Table<Trade>()
            .Where(t => t.Manager == Entity.Id)
            .ToList();
        TradeCV = new CollectionView
        {
            ItemsSource = trades,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 8
            },
            ItemTemplate = new DataTemplate(() => new EntityListView<Trade>())
        };

        var refreshButton = new Button { Text = "Refresh" };
        var addButton = new Button { Text = "Add" };
        var clearButton = new Button { Text = "Clear All", BackgroundColor = Colors.Red };

        refreshButton.Clicked += (_, __) => RefreshTrades();
        addButton.Clicked += (_, __) => AddTrade();
        clearButton.Clicked += async (_, __) => await ClearTrades();

        clearButton.IsEnabled = AccessControl.CanDelete<Trade>();
        addButton.IsEnabled = AccessControl.CanCreate<Trade>();

        StatusPicker = new Picker
        {
            Title = "Filter by Trade Status",
            ItemsSource = Enum.GetNames(typeof(TradeStatus)).ToList(),
            SelectedIndex = 0
        };
        StatusPicker.SelectedIndexChanged += (_, __) => RefreshTrades();
        var userInfo = new VerticalStackLayout
        {
            Padding = new Thickness(10),
            Children =
            {
                new Label { Text = $"Welcome, {Entity.Name}!", FontSize = 20, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center },
                new Label { Text = $"Role: {Entity.Role}" },
                new Label { Text = $"ID: {Entity.Id}" },
                new Label { Text = $"Password: {Entity.Password}" },
                new Label { Text = $"Description: {Entity.Description}" }
            }
        };

        var filterPanel = new HorizontalStackLayout
        {
            Padding = new Thickness(10, 0),
            Children =
            {
                new Label { Text = "Trade status:", VerticalOptions = LayoutOptions.Center },
                StatusPicker
            }
        };

        var controlPanel = new HorizontalStackLayout
        {
            Padding = new Thickness(10, 5),
            Spacing = 10,
            Children = { refreshButton, addButton, clearButton }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },  
                new RowDefinition { Height = GridLength.Auto },  
                new RowDefinition { Height = GridLength.Auto },  
                new RowDefinition { Height = GridLength.Star }   
            }
        };

        grid.Add(userInfo, 0, 0);
        grid.Add(filterPanel, 0, 1);
        grid.Add(controlPanel, 0, 2);
        grid.Add(TradeCV, 0, 3);

        Content = grid;
    }

    private void RefreshTrades()
    {
        if (Entity == null) return;

        var selectedStatus = TradeStatus.New;
        if (StatusPicker.SelectedIndex >= 0)
        {
            var name = StatusPicker.Items[StatusPicker.SelectedIndex];
            selectedStatus = Enum.Parse<TradeStatus>(name);
        }

        var trades = DatabaseService.Connection.Table<Trade>()
            .Where(t => t.Manager == Entity.Id && t.Status == selectedStatus)
            .ToList();

        TradeCV.ItemsSource = trades;
    }

    private void AddTrade()
    {
        if (Entity == null) return;
        DatabaseService.Connection.Insert(new Trade { Manager = Entity.Id });
        RefreshTrades();
    }

    private async Task ClearTrades()
    {
        if (Entity == null) return;

        var answer = await DisplayAlert("Delete", "Are you sure?", "Yes", "No");
        if (!answer) return;

        var trades = DatabaseService.Connection.Table<Trade>()
            .Where(t => t.Manager == Entity.Id)
            .ToList();

        foreach (var td in trades)
            DatabaseService.Connection.Delete(td);

        RefreshTrades();
        await DisplayAlert("Delete", "Delete completed", "OK");
    }
}
