using System;
using System.Globalization;
using System.Windows;

namespace TelerikWpfApp2
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow(string icphoneNumber, string haphoneNumber, string icphoneNumber2, string haphoneNumber2, int smsSendInterval, int refreshTime, int waitThresholdTime, int sendCleanUpTime,
            int recieveCleanUpTime,double delayThreshold,int notRecievedThreshold,bool atieActive,bool smartActive,
            bool jiringActive, bool rahyabActive, int chartsInterVals,bool isIcActive,bool isHamrahActive)
        {
            InitializeComponent();


            PhoneNumberBox.Text = icphoneNumber;
            HaPhoneNumberBox.Text = haphoneNumber;
            IranCell2TextBox.Text = icphoneNumber;
            HamrahAvval2TextBox.Text = haphoneNumber;
            
            SendSmsIntervalBox.Text = smsSendInterval.ToString(CultureInfo.InvariantCulture);
            RefreshTimeBox.Text = refreshTime.ToString(CultureInfo.InvariantCulture);
            WaitThresholdTimeBox.Text = waitThresholdTime.ToString(CultureInfo.InvariantCulture);
            SendCleanUpTimeBox.Text = sendCleanUpTime.ToString(CultureInfo.InvariantCulture);
            RecieveCleanUpTimeBox.Text = recieveCleanUpTime.ToString(CultureInfo.InvariantCulture);
            DelayThresholdBox.Text = delayThreshold.ToString(CultureInfo.InvariantCulture);
            NotRecievedThresholdBox.Text = notRecievedThreshold.ToString(CultureInfo.InvariantCulture);

            AtieCheckBox.IsChecked = atieActive;
            SmartCheckBox.IsChecked = smartActive;
            JiringCheckBox.IsChecked = jiringActive;
            RahyabCheckBox.IsChecked = rahyabActive;
            ActiveIrancell.IsChecked = isIcActive;
            ActiveHamrahAvval.IsChecked = isHamrahActive;


            ChartsIntervalsBox.Text = chartsInterVals.ToString(CultureInfo.InvariantCulture);



        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var icphoneNumber = PhoneNumberBox.Text;
                var haphoneNumber = HaPhoneNumberBox.Text;
                var icphoneNumber2 = IranCell2TextBox.Text;
                var haphoneNumber2 = HamrahAvval2TextBox.Text;

                var smsSendInterval = Convert.ToInt32(SendSmsIntervalBox.Text);
                var refreshTime = Convert.ToInt32(RefreshTimeBox.Text);
                var waitThresholdTime = Convert.ToInt32(WaitThresholdTimeBox.Text);
                var sendCleanUpTime = Convert.ToInt32(SendCleanUpTimeBox.Text);
                var recieveCleanUpTime = Convert.ToInt32(RecieveCleanUpTimeBox.Text);
                var delayThreshold = Convert.ToDouble(DelayThresholdBox.Text);
                var notRecievedThreshold = Convert.ToInt32(NotRecievedThresholdBox.Text);
                var chartsIntevals = Convert.ToInt32(ChartsIntervalsBox.Text);

                var atieCheckBox = AtieCheckBox.IsChecked != null && AtieCheckBox.IsChecked.Value;
                var smartCheckBox = SmartCheckBox.IsChecked != null && SmartCheckBox.IsChecked.Value;
                var jiringCheckBox = JiringCheckBox.IsChecked != null && JiringCheckBox.IsChecked.Value;
                var rahCheckBox = RahyabCheckBox.IsChecked != null && RahyabCheckBox.IsChecked.Value;
                var monitorIrancell = ActiveIrancell.IsChecked != null && ActiveIrancell.IsChecked.Value;
                var monitorHamrahAvval = ActiveHamrahAvval.IsChecked != null && ActiveHamrahAvval.IsChecked.Value;


                MainWindow.SetConfigs(icphoneNumber, haphoneNumber, icphoneNumber2, haphoneNumber2, smsSendInterval, refreshTime, waitThresholdTime, sendCleanUpTime, 
                    recieveCleanUpTime, delayThreshold, notRecievedThreshold, atieCheckBox,smartCheckBox,
                    jiringCheckBox, rahCheckBox, chartsIntevals,monitorIrancell,monitorHamrahAvval);
                Close();

            }
            catch (Exception  exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
