using System.Windows;
using System.Windows.Threading;

namespace v2rayN.Views;

public partial class TimedNoticeDialog
{
    private DispatcherTimer? _timer;
    private int _remaining;

    public TimedNoticeDialog() : this(string.Empty, string.Empty, 10)
    {
    }

    public TimedNoticeDialog(string title, string message, int autoCloseSeconds)
    {
        InitializeComponent();

        Title = title;
        txtTitle.Text = title;
        txtMessage.Text = message;
        _remaining = autoCloseSeconds;
        txtCountdown.Text = string.Format(ResUI.TbAutoCloseIn, _remaining);
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        timer.Tick += (_, _) =>
        {
            _remaining--;
            if (_remaining <= 0)
            {
                timer.Stop();
                Close();
                return;
            }
            txtCountdown.Text = string.Format(ResUI.TbAutoCloseIn, _remaining);
        };
        timer.Start();
        WindowsUtils.SetDarkBorder(this, AppManager.Instance.Config.UiItem.CurrentTheme);
    }
}
