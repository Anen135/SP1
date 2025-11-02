using SP1.Attributes;
using SQLite;
using System.Reflection;

namespace SP1.Pages;

public class EntityDetailPage<T> : ContentPage where T : new()
{
    private T Entity;
    private PropertyInfo[] properties;
    private Dictionary<PropertyInfo, Entry> entries = new();
    private Dictionary<PropertyInfo, Picker> pickers = new();
    private Dictionary<PropertyInfo, Picker> foreigns = new();

    public EntityDetailPage(T entity)
    {
        Entity = entity;
        properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Title = $"{typeof(T).Name} Detail";
        BuildView();
    }

    private void BuildView()
    {
        var VSL = new VerticalStackLayout();
        foreach (var prop in properties)
        {
            if (!prop.CanWrite) continue;
            var val = prop.GetValue(Entity);
            if (prop.GetCustomAttribute<PrimaryKeyAttribute>() != null) { // Label 
                if (val == null) throw new Exception(Title="val is null");
                if (val.ToString() == null) throw new Exception(Title = "cant convert to string");
                VSL.Add(new Label { Text = $"Number: {val.ToString()}" });
            } else if (prop.PropertyType == typeof(string)) { // Entry
                if (val == null) throw new Exception(Title="val is null");
                if (val.ToString() == null) throw new Exception(Title = "cant convert to string");
                var entry = new Entry { Text = val.ToString()};
                VSL.Add(new Label { Text = $"{prop.Name}"});
                VSL.Add(entry);
                entries[prop] = entry;
            } else if (prop.PropertyType.IsEnum) { // Picker
                var picker = new Picker
                {
                    Title = $"Select {prop.Name}",
                    ItemsSource = Enum.GetValues(prop.PropertyType),
                    ItemDisplayBinding = new Binding(".")
                };
                if (val != null) picker.SelectedItem = val;
                VSL.Add(picker);
                pickers[prop] = picker;
            } else if (prop.GetCustomAttribute<ForeignEntityAttribute>() is not null and var foreignattr) {
                var items = DatabaseService.foreignMap[foreignattr.Foreign]();
                var picker = new Picker
                {
                    Title = $"{prop.Name}",
                    ItemsSource = items,
                    ItemDisplayBinding = new Binding("Name")
                };
                if (val is int Id)
                    picker.SelectedItem = items.FirstOrDefault(x => x.Id == Id);
                VSL.Add(picker);
                foreigns[prop] = picker;
            }
            else { // Unexpected
                VSL.Add(new Label { Text = $"{prop.Name} : {val?.ToString() ?? "Unexpected"}" });
            }
        }
        var saveBtn = new Button { Text = "Save" };
        var deleteBtn= new Button { Text = "Delete"};
        saveBtn.Clicked += (_, __) => Save();
        deleteBtn.Clicked += (_, __) => Delete();
        VSL.Add(saveBtn);
        VSL.Add(deleteBtn);
        Content = new ScrollView { Content = VSL };
    }

    private async void Save()
    {
        try
        {
            foreach (var (key, val) in entries)
            {
                key.SetValue(Entity, val.Text);
            }
            foreach (var (key, val) in pickers)
            {
                key.SetValue(Entity, val.SelectedItem);
            }
            foreach (var (key, val) in foreigns)
            {
                if (val.SelectedItem is Models.EntityBase entity)
                    key.SetValue(Entity, entity.Id);
            }

            DatabaseService.Connection.Update(Entity);
            await App.Current.Windows[0].Page.DisplayAlert("Save", "Saved successfully", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await App.Current.Windows[0].Page.DisplayAlert("Error", $"Error on save: {ex.Message}", "OK");
        }
    }

    private async void Delete()
    {
        var answer = await App.Current.Windows[0].Page.DisplayAlert("Delete", "Are you sure?", "Yes", "Noooo");
        if (!answer) return;
        DatabaseService.Connection.Delete(Entity);
        await Navigation.PopAsync();
    }
}
