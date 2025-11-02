using System.Reflection;

namespace SP1.Pages;

public partial class EntityListPage<T> : ContentPage where T : new()
{
    private CollectionView CV;
    private PropertyInfo[] Props;
    public EntityListPage()
    {
        Title = $"{typeof(T).Name}List";
        var Items = DatabaseService.Connection.Table<T>().ToList();
        Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Content = BuildView(Items);
    }

    private View BuildView(List<T> Items)
    {
        var grid = new Grid
        {
            RowDefinitions = {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };

        CV = new CollectionView
        {
            ItemsSource = Items,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 8
            },
            ItemTemplate = new DataTemplate(() =>
            {
                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(250),
                        new ColumnDefinition { Width =  GridLength.Auto },
                    }
                };
                var SL = new StackLayout { Margin = new Thickness(16, 0, 0, 0) };
                var openBtn = new Button { Text = "Open" };
                openBtn.Clicked += (s, _) => Open(s);
                openBtn.IsEnabled = AccessControl.CanOpen<T>();
                foreach (var prop in Props)
                {
                    var label = new Label();
                    if (prop.GetCustomAttribute<Attributes.ForeignEntityAttribute>() is not null and var foreignattr)
                    {
                        label.BindingContextChanged += (_, __) =>
                        {
                            if (label.BindingContext is not T ctx)
                            {
                                label.Text = $"{prop.Name}: --";
                                return;
                            }
                            var val = prop.GetValue(ctx);
                            if (val is null)
                            {
                                label.Text = $"{prop.Name}: --";
                            } else if (!DatabaseService.foreignMap.TryGetValue(foreignattr.Foreign, out var getForeignList))
                            {
                                label.Text = $"{prop.Name}: --";
                            } else
                            {
                                var entity = getForeignList().FirstOrDefault(x => x.Id == (int)val);
                                label.Text = $"{prop.Name}: {entity?.Name ?? "--"}";
                            }
                        };
                    }
                    else
                    {
                        label.SetBinding(Label.TextProperty, new Binding(prop.Name, stringFormat: $"{prop.Name} : {{0}}"));
                    }
                    SL.Add(label);
                }
                grid.Add(SL, 0, 0);
                grid.Add(openBtn, 1, 0);
                return grid;
            })
        };

        var RefreshBtn = new Button { Text = "Refresh", };
        var ClearAllBtn = new Button { Text = "Clear All", BackgroundColor = Colors.Red, };
        var AddBtn = new Button { Text = "Add", };

        RefreshBtn.Clicked += (_, __) => Refresh();
        AddBtn.Clicked += (_, __) => Add();
        ClearAllBtn.Clicked += (_, __) => ClearAll();

        ClearAllBtn.IsEnabled = AccessControl.CanDelete<T>();
        AddBtn.IsEnabled = AccessControl.CanCreate<T>();

        grid.Add(new HorizontalStackLayout { Children = { RefreshBtn, AddBtn, ClearAllBtn, } }, 0, 0);
        grid.Add(CV, 0, 1);
        return grid;
    }

    private async void Open(object s)
    {
        var btn = (Button)s;
        var entity = (T)btn.BindingContext;
        await Navigation.PushAsync(new EntityDetailPage<T>(entity));
    }

    private void Refresh()
    {
        CV.ItemsSource = DatabaseService.Connection.Table<T>().ToList();
    }

    private void Add()
    {
        DatabaseService.Connection.Insert(new T());
        Refresh();
    }

    private async void ClearAll()
    {
        var answer = await App.Current.Windows[0].Page.DisplayAlert("Delete", "Are u sure?", "Yes", "Nooooo");
        if (answer)
        {
            DatabaseService.Connection.DeleteAll<T>();
            Refresh();
            await App.Current.Windows[0].Page.DisplayAlert("Delete", "Delete completed", "OK");
        }
    }

}
