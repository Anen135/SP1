using SP1.Components;

namespace SP1.Pages;

public partial class EntityListPage<T> : ContentPage where T : new()
{
    public EntityListPage()
    {
        Title = $"{typeof(T).Name} List";
        var items = DatabaseService.Connection.Table<T>().ToList();
        Content = new EntityListView<T>(items);
    }
}
