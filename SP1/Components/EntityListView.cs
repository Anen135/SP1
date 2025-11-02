using System.Reflection;

namespace SP1.Components;

public class EntityListView<T> : ContentView where T : new()
{
    public CollectionView CV;
    public EntityListView()
    {
        var Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(250),
                new ColumnDefinition { Width = GridLength.Auto },
            }
        };
        var SL = new StackLayout { Margin = new Thickness(16, 0, 0, 0) };
        var openBtn = new Button { Text = "Open" };

        openBtn.Clicked += (s, _) => Open(s);

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
            SL.Add(label);
        }

        grid.Add(SL, 0, 0);
        grid.Add(openBtn, 1, 0);
        Content = grid;
    }

    public async void Open(object s)
    {
        var btn = (Button)s;
        var entity = (T)btn.BindingContext;
        await Navigation.PushAsync(new Pages.EntityDetailPage<T>(entity));
    }
}
