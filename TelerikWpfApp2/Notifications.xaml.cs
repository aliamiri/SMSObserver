using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace TelerikWpfApp2
{
    /// <summary>
    /// Interaction logic for Notifications.xaml
    /// </summary>
    public partial class Notifications : Window
    {
        public Notifications(List<NotificationsClass> notificationsClasses )
        {
            InitializeComponent();

            RadNoficationGridView.ItemsSource = notificationsClasses;
            PreviewKeyDown += HandleEsc;
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
