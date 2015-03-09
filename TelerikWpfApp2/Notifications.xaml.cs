using System.Collections.Generic;
using System.Windows;

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
        }
    }
}
