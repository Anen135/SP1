using System.Reflection;

namespace SP1.Components;

public class BaseListView<T> : ContentView where T : new()
{
    protected readonly CollectionView CV;
    protected readonly PropertyInfo[] Props;

    public BaseListView(List<T> items)
    {
        Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        CV = new CollectionView
        {
            ItemsSource = items,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 8
            },
            ItemTemplate = new DataTemplate(() => CreateItemTemplate())
        };

        Content = BuildView();
    }

    /// <summary>
    /// Строит базовую структуру (кнопки + контент + коллекция)
    /// Можно переопределять в наследниках, вызывая base.BuildView()
    /// </summary>
    protected virtual View BuildView()
    {
        var refreshBtn = new Button { Text = "Refresh" };
        var addBtn = new Button { Text = "Add" };
        var clearBtn = new Button { Text = "Clear All", BackgroundColor = Colors.Red };

        refreshBtn.Clicked += (_, __) => Refresh();
        addBtn.Clicked += (_, __) => Add();
        clearBtn.Clicked += async (_, __) => await ClearAll();

        clearBtn.IsEnabled = AccessControl.CanDelete<T>();
        addBtn.IsEnabled = AccessControl.CanCreate<T>();

        var buttonBar = new HorizontalStackLayout
        {
            Children = { refreshBtn, addBtn, clearBtn }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // Buttons
                new RowDefinition { Height = GridLength.Auto }, // Custom content (вставка)
                new RowDefinition { Height = GridLength.Star }  // CollectionView
            }
        };

        // Добавляем кнопки
        grid.Add(buttonBar, 0, 0);

        // Вставочное место — можно переопределить InsertCustomContent
        var customContent = InsertCustomContent();
        if (customContent != null)
            grid.Add(customContent, 0, 1);

        // Добавляем CollectionView
        grid.Add(CV, 0, 2);

        return grid;
    }

    /// <summary>
    /// Можно переопределить, чтобы вставить свой контент между кнопками и коллекцией.
    /// </summary>
    protected virtual View? InsertCustomContent() => null;

    /// <summary>
    /// Шаблон одного элемента списка.
    /// Можно переопределить в наследниках для кастомизации.
    /// </summary>
    protected virtual View CreateItemTemplate()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(250),
                new ColumnDefinition { Width = GridLength.Auto },
            }
        };

        var stack = new StackLayout { Margin = new Thickness(16, 0, 0, 0) };
        var openBtn = new Button { Text = "Open" };

        openBtn.Clicked += async (s, _) =>
        {
            if (s is Button btn && btn.BindingContext is T entity)
            {
                await Navigation.PushAsync(new Pages.EntityDetailPage<T>(entity));
            }
        };

        openBtn.IsEnabled = AccessControl.CanOpen<T>();

        foreach (var prop in Props)
        {
            var label = new Label();

            if (prop.GetCustomAttribute<Attributes.ForeignEntityAttribute>() is Attributes.ForeignEntityAttribute foreignAttr)
            {
                label.BindingContextChanged += (_, __) =>
                {
                    if (label.BindingContext is not T ctx)
                    {
                        label.Text = $"{prop.Name}: --";
                        return;
                    }
                    var val = prop.GetValue(ctx);
                    if (val == null)
                    {
                        label.Text = $"{prop.Name}: --";
                        return;
                    }

                    if (!DatabaseService.foreignMap.TryGetValue(foreignAttr.Foreign, out var getForeignList))
                    {
                        label.Text = $"{prop.Name}: --";
                        return;
                    }

                    var entity = getForeignList().FirstOrDefault(x => x.Id == (int)val);
                    label.Text = $"{prop.Name}: {entity?.Name ?? "--"}";
                };
            }
            else
            {
                label.SetBinding(Label.TextProperty, new Binding(prop.Name, stringFormat: $"{prop.Name} : {{0}}"));
            }
            stack.Add(label);
        }

        grid.Add(stack, 0, 0);
        grid.Add(openBtn, 1, 0);
        return grid;
    }

    protected void Refresh()
    {
        CV.ItemsSource = DatabaseService.Connection.Table<T>().ToList();
    }

    protected void Add()
    {
        DatabaseService.Connection.Insert(new T());
        Refresh();
    }

    protected async Task ClearAll()
    {
        var answer = await App.Current.Windows[0].Page.DisplayAlert("Delete", "Are you sure?", "Yes", "No");
        if (answer)
        {
            DatabaseService.Connection.DeleteAll<T>();
            Refresh();
            await App.Current.Windows[0].Page.DisplayAlert("Delete", "Delete completed", "OK");
        }
    }
}
