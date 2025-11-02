using SP1.Models;

namespace SP1.Pages;

public class LoginPage : ContentPage
{
    Entry loginEntry;
    Entry passwordEntry;
    Label messageLabel;

    public LoginPage()
    {
        Title = "Login";

        loginEntry = new Entry { Placeholder = "Login", ReturnType = ReturnType.Next, Text = "Manager"};
        passwordEntry = new Entry { Placeholder = "Password", IsPassword = true, ReturnType = ReturnType.Go, Text = "Manager" };
        messageLabel = new Label { Text = "Write your cock" };
        loginEntry.Completed += (s, e) => passwordEntry.Focus();
        passwordEntry.Completed += OnLoginCompleted;

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Children =
                {
                    new Label { Text = "Login" },
                    loginEntry,
                    new Label { Text = "Password" },
                    passwordEntry,
                    messageLabel
                }
            }
        };
    }

    private void OnLoginCompleted(object sender, EventArgs e)
    {
        var user = DatabaseService.Connection.Find<Manager>( m => m.Name == loginEntry.Text && m.Password == passwordEntry.Text);
        if (user != null)
        {
            DatabaseService.CurrentUser = user;
            AccessControl.Role = user.Role;
            messageLabel.Text = "Correct!";
            messageLabel.TextColor = Colors.Green;
            ((AppShell)App.Current.Windows[0].Page).showControls();
        }
        else
        {
            messageLabel.Text = "Wrong!";
            messageLabel.TextColor = Colors.Red;
        }
    }
}
