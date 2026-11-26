using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace v2rayN.Views;

public partial class UrlDetectedDialog
{
    public int Choice { get; private set; } = -1;

    public UrlDetectedDialog() : this(string.Empty)
    {
    }

    public UrlDetectedDialog(string url)
    {
        InitializeComponent();

        Title = "url detected";
        txtMessage.Text = $"{url}\nHow to add?";
        var items = new ObservableCollection<string>
        {
            "As Subscription (add to this group)",
            "As Subscription (create new group)",
            "As link",
        };
        lstOptions.ItemsSource = items;
        lstOptions.SelectedIndex = 0;
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        };
        WindowsUtils.SetDarkBorder(this, AppManager.Instance.Config.UiItem.CurrentTheme);
    }

    private void LstOptions_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return && lstOptions.SelectedIndex >= 0)
        {
            Confirm();
            e.Handled = true;
        }
    }

    private void LstOptions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (lstOptions.SelectedIndex >= 0)
        {
            Confirm();
        }
    }

    private void Confirm()
    {
        Choice = lstOptions.SelectedIndex;
        DialogResult = true;
    }
}
