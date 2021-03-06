﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using NLog;
using SMSapplication;
using SMSDLL;
using Telerik.Windows.Controls.Charting;

namespace TelerikWpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        private Logger logger = LogManager.GetLogger("MainWindow");

        

        #region fields

        private static string IC_PhoneNumber;
        private static string HA_PhoneNumber;
        private static string IC_PhoneNumber2;
        private static string HA_PhoneNumber2;

        private static int SendSmsInterval;
        private static int RefreshTime;
        private static int WaitThresholdTime;
        private static int SendCleanUpTime;
        private static int RecieveCleanUpTime;

        private static long DelayThreshold;
        private static int NotRecievedThreshold;
        private static int ChartsInterVals;

        private static bool ActiveADP = true;
        private static bool ActiveSmart = true;
        private static bool ActiveRahnama = true;
        private static bool ActiveJiring = true;



        private static int IC_SerialPortSpeed = 115200;
        private static int HA_SerialPortSpeed = 115200;
        private static int IC_SerialPortSpeed2 = 115200;
        private static int HA_SerialPortSpeed2 = 115200;

        private static string IC_PortNo = "Com4";
        private static string HA_PortNo = "Com4";
        private static string IC_PortNo2 = "Com4";
        private static string HA_PortNo2 = "Com4";

        private List<NotificationsClass> _notificationsClasses = new List<NotificationsClass>();
        private List<SendFailNotifications> _channelProblemClasses = new List<SendFailNotifications>();

        private readonly List<SendSmsWrapper> _sendSmses = new List<SendSmsWrapper>();

        private ShortMessageCollection _objShortMessageCollection = new ShortMessageCollection();

        private readonly List<RecievedSmsWrapper> _smsClasses = new List<RecievedSmsWrapper>();

        private static int _counter = 0;

        private SerialPort ic_port = new SerialPort();
        private SerialPort ha_port = new SerialPort();
        private SerialPort ic_port2 = new SerialPort();
        private SerialPort ha_port2 = new SerialPort();

        private clsSMS _objclsSms = new clsSMS();

        private ObservableCollection<MyChartObject> _latancyList = new ObservableCollection<MyChartObject>();
        private ObservableCollection<MyChartObject> _NotRecievedList = new ObservableCollection<MyChartObject>();


        private static bool isICEnabaled;
        private static bool isHaenabaled = true;
        private static bool isICEnabaled2;
        private static bool isHaenabaled2;


        private bool _isIcAtieNotRecieved;
        private bool _isIcRahyabNotRecieved;
        private bool _isIcSmartNotRecieved;

        private bool _ishaAtieNotRecieved;
        private bool _ishaRahyabNotRecieved;
        private bool _ishaSmartNotRecieved;
        private bool _isJiringNotRecieved;

        private bool _isIcAtieDelay;
        private bool _isIcRahyabDelay;
        private bool _isIcSmartDelay;

        private bool _ishaAtieDelay;
        private bool _ishaRahyabDelay;
        private bool _ishaSmartDelay;
        private bool _isJiringDelay;

        private int icAtieStatus = 0;
        private int icSmartStatus = 0;
        private int icRahyabStatus = 0;

        private int haAtieStatus = 0;
        private int haSmartStatus = 0;
        private int haRahyabStatus = 0;
        private int jiringStatus = 0;
        
        private BitmapSource redSource;
        private BitmapSource yellowSource;
        private BitmapSource greenSource;
        
        #endregion



        public MainWindow()
        {
            logger.Debug("program starts ... ");
            InitializeComponent();
            SetDefaultValues();
            OpenPorts();
            InitSeries();
            redSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    Properties.Resources.red_light.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(50, 50));
            yellowSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    Properties.Resources.yellow_light.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(50, 50));
            greenSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    Properties.Resources.green_light.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(50, 50));
        }

      

        public async void AsyncTasks()
        {
            try
            {
                var sendAsync = Task<string>.Factory.StartNew(SendSms);
                var readAsync1 = Task<string>.Factory.StartNew(ReadIrancell1Sms);
                var readAsync2 = Task<string>.Factory.StartNew(ReadIrancell2Sms);
                var readAsync3 = Task<string>.Factory.StartNew(ReadHamrahAvval1Sms);
                var readAsync4 = Task<string>.Factory.StartNew(ReadHamrahAvval2Sms);
                await UpdateCharts();
                await readAsync1;
                await readAsync2;
                await readAsync3;
                await readAsync4;
                await sendAsync;
            }
            catch (Exception e)
            {
                logger.Error("AsyncTasks : " + e.Message);
            }
        }

        #region initialization

        private void OpenPorts()
        {
            try
            {
                if (isICEnabaled)
                {
                    ic_port = _objclsSms.OpenPort(IC_PortNo, IC_SerialPortSpeed, 8, 5000, 5000);
                }
                if (isICEnabaled2)
                {
                    ic_port2 = _objclsSms.OpenPort(IC_PortNo2, IC_SerialPortSpeed2, 8, 5000, 5000);
                }
                if (isHaenabaled)
                {
                    ha_port = _objclsSms.OpenPort(HA_PortNo, HA_SerialPortSpeed, 8, 5000, 5000);
                }
                if (isHaenabaled2)
                {
                    ha_port2 = _objclsSms.OpenPort(HA_PortNo2, HA_SerialPortSpeed2, 8, 5000, 5000);
                }
            }
            catch (Exception ex)
            {
                logger.Error("MainWindow " + ex.Message);
            }
        }


        private static void SetDefaultValues()
        {
            try
            {
                IC_PhoneNumber = ConfigurationManager.AppSettings["IC_PhoneNumber"];
                HA_PhoneNumber = ConfigurationManager.AppSettings["HA_PhoneNumber"];
                IC_PhoneNumber2 = ConfigurationManager.AppSettings["IC_PhoneNumber2"];
                HA_PhoneNumber2 = ConfigurationManager.AppSettings["HA_PhoneNumber2"];

                SendSmsInterval = Convert.ToInt32(ConfigurationManager.AppSettings["SendSmsInterval"] );
                RefreshTime = Convert.ToInt32(ConfigurationManager.AppSettings["RefreshTime"] );
                WaitThresholdTime = Convert.ToInt32(ConfigurationManager.AppSettings["WaitThresholdTime"] );
                SendCleanUpTime = Convert.ToInt32(ConfigurationManager.AppSettings["SendCleanUpTime"] );
                RecieveCleanUpTime =
                    Convert.ToInt32(ConfigurationManager.AppSettings["RecieveCleanUpTime"] );

                ChartsInterVals = Convert.ToInt32(ConfigurationManager.AppSettings["ChartsIntervals"] );


                IC_SerialPortSpeed = Convert.ToInt32(ConfigurationManager.AppSettings["ICSpeed"] );

                HA_SerialPortSpeed = Convert.ToInt32(ConfigurationManager.AppSettings["HASpeed"] );

                IC_SerialPortSpeed2 = Convert.ToInt32(ConfigurationManager.AppSettings["ICSpeed2"] );

                HA_SerialPortSpeed2 = Convert.ToInt32(ConfigurationManager.AppSettings["HASpeed2"] );


                IC_PortNo = ConfigurationManager.AppSettings["ICComPort"] ;

                HA_PortNo = ConfigurationManager.AppSettings["HAComPort"] ;

                IC_PortNo2 = ConfigurationManager.AppSettings["ICComPort2"] ;

                HA_PortNo2 = ConfigurationManager.AppSettings["HAComPort2"] ;

                SendSmsWrapper.WaitTime = SendCleanUpTime*1000;
                RecievedSmsWrapper.WaitTime = RecieveCleanUpTime*1000;

                DelayThreshold = Convert.ToInt64(ConfigurationManager.AppSettings["DelayThreshold"] );
                NotRecievedThreshold = Convert.ToInt32(ConfigurationManager.AppSettings["NotRecievedThreshold"] );
            }
            catch (Exception  e)
            {
                //logger.Error("setDefs : " + e.Message);
            }
        }

        private void InitSeries()
        {
            const string category = "StartTime";


            var yValue = "IcApdVal";
            var lable = "آتیه - ایرانسل";
           
            var icadpSeriesMapping = InitiSerie(lable, category, yValue);
            SmsRadChart.SeriesMappings.Add(icadpSeriesMapping);

            var icadpSeriesMapping2 = InitiSerie(lable, category, yValue);
            SmsRadChart2.SeriesMappings.Add(icadpSeriesMapping2);


            yValue = "HaApdVal";
            lable = "آتیه - همراه";

            var adpSeriesMapping = InitiSerie(lable, category, yValue);
            SmsRadChart.SeriesMappings.Add(adpSeriesMapping);

            var adpSeriesMapping2 = InitiSerie(lable, category, yValue);
            SmsRadChart2.SeriesMappings.Add(adpSeriesMapping2);


             yValue = "IcSmartVal";
            lable = "اسمارت-ایرانسل";
            var icsmartSeriesMapping = InitiSerie(lable, category, yValue);
            SmsRadChart.SeriesMappings.Add(icsmartSeriesMapping);

            var icsmartSeriesMapping2= InitiSerie(lable, category, yValue);
            SmsRadChart2.SeriesMappings.Add(icsmartSeriesMapping2);


             yValue = "HaSmartVal";
            lable =  "اسمارت-همراه";
            var smartSeriesMapping = InitiSerie(lable, category, yValue);
            SmsRadChart.SeriesMappings.Add(smartSeriesMapping);

            var smartSeriesMapping2 = InitiSerie(lable, category, yValue);
            SmsRadChart2.SeriesMappings.Add(smartSeriesMapping2);


             yValue = "IcRahVal";
            lable ="رهیاب-ایرانسل";
            var icrahSeriesMapping = InitiSerie(lable, category, yValue);
            SmsRadChart.SeriesMappings.Add(icrahSeriesMapping);

            var icrahSeriesMapping2= InitiSerie(lable, category, yValue);
            SmsRadChart2.SeriesMappings.Add(icrahSeriesMapping2);


             yValue = "HaRahVal";
            lable = "رهیاب-همراه";
            var rahSeriesMapping = InitiSerie(lable, category, yValue);
            SmsRadChart.SeriesMappings.Add(rahSeriesMapping);

            var rahSeriesMapping2 = InitiSerie(lable, category, yValue);
            SmsRadChart2.SeriesMappings.Add(rahSeriesMapping2);

             yValue = "JiringVal";
            lable = "جیرینگ";
            var jiringSeriesMapping = InitiSerie(lable, category, yValue);
            SmsRadChart.SeriesMappings.Add(jiringSeriesMapping);

            var jiringSeriesMapping2 = InitiSerie(lable, category, yValue);
            SmsRadChart2.SeriesMappings.Add(jiringSeriesMapping2);

            SmsRadChart.DefaultView.ChartArea.EnableAnimations = false;
            SmsRadChart2.DefaultView.ChartArea.EnableAnimations = false;
        }

        private static SeriesMapping InitiSerie(string lable, string category, string yValue)
        {
            var icadpSeriesMapping = new SeriesMapping
            {
                LegendLabel = lable,
                SeriesDefinition = new LineSeriesDefinition()
            };
            icadpSeriesMapping.ItemMappings.Add(new ItemMapping(category, DataPointMember.XCategory));
            icadpSeriesMapping.ItemMappings.Add(new ItemMapping(yValue, DataPointMember.YValue));
            icadpSeriesMapping.SeriesDefinition.ShowItemLabels = false;
            return icadpSeriesMapping;
        }

        #endregion

        #region charts

        private async Task<string> UpdateCharts()
        {
            while (runningState)
            {
                await Task.Delay(1000*RefreshTime);
                try
                {
                    CreateDataForDelay();
                    SmsRadChart.ItemsSource = _latancyList;
                    CreateDataForNotRecieved();
                    SmsRadChart2.ItemsSource = _NotRecievedList;

                    RadOperatorProblemGridView.ItemsSource = null;
                    RadOperatorProblemGridView.ItemsSource = _channelProblemClasses;

                    RadNoficationGridView.ItemsSource = null;
                    RadNoficationGridView.ItemsSource = _notificationsClasses;

                    ChangeStatusColor();
                }
                catch (Exception e)
                {
                    logger.Error("update : " + e.Message);
                }
            }
            return null;
        }

        private void ChangeStatusColor()
        {
            AtieImage.Source = icAtieStatus > 2 ? redSource : icAtieStatus > 0 ? yellowSource : greenSource;
            SmartImage.Source = icSmartStatus > 2 ? redSource : icSmartStatus > 0 ? yellowSource : greenSource;
            RahyabImage.Source = icRahyabStatus > 2 ? redSource : icRahyabStatus > 0 ? yellowSource : greenSource;

            AtieImageHa.Source = haAtieStatus> 2 ? redSource : haAtieStatus> 0 ? yellowSource : greenSource;
            SmartImageHa.Source = haSmartStatus> 2 ? redSource : haSmartStatus> 0 ? yellowSource : greenSource;
            RahyabImageHa.Source = haRahyabStatus> 2 ? redSource : haRahyabStatus> 0 ? yellowSource : greenSource;
            JiringImage.Source = jiringStatus> 2 ? redSource : jiringStatus> 0 ? yellowSource : greenSource;
        }

        private void CreateDataForDelay()
        {
            try
            {

                var dateTime = DateTime.Now;
                var adpTotalDelay = GetDelay(0);
                
                var haadpTotalDelay = GetDelay(4);
                var smartTotalDelay = GetDelay(1);
                var haSmartTotalDelay = GetDelay(5);
                var rahTotalDelay = GetDelay(2);
                var haRahTotalDelay = GetDelay(6);
                var jirTotalDelay = GetDelay(3);

                SetDelayStatus(adpTotalDelay, smartTotalDelay, rahTotalDelay, haadpTotalDelay, haSmartTotalDelay, haRahTotalDelay, jirTotalDelay);

                UpdateDelayNotifications(adpTotalDelay, dateTime, haadpTotalDelay, smartTotalDelay, haSmartTotalDelay,
                    rahTotalDelay, haRahTotalDelay, jirTotalDelay);

                while (_latancyList.Count > ChartsInterVals)
                    _latancyList.RemoveAt(0);

                var o = new MyChartObject(adpTotalDelay, smartTotalDelay, rahTotalDelay, jirTotalDelay, haadpTotalDelay,
                    haSmartTotalDelay, haRahTotalDelay, DateTime.Now);

                _latancyList.Add(o);
            }
            catch (Exception ex)
            {
                logger.Error("CreateDataForDelay" + ex.Message);
            }
        }

        private void SetDelayStatus(long adpTotalDelay, long smartTotalDelay, long rahTotalDelay, long haadpTotalDelay,
            long haSmartTotalDelay, long haRahTotalDelay, long jirTotalDelay)
        {
            var twoThird = 2*DelayThreshold/3;
            var half = DelayThreshold/2;

            icAtieStatus = adpTotalDelay > DelayThreshold ? 3 : adpTotalDelay > twoThird ? 2 : adpTotalDelay > half ? 1 : 0;

            icSmartStatus = smartTotalDelay > DelayThreshold
                ? 3
                : smartTotalDelay > twoThird ? 2 : smartTotalDelay > half ? 1 : 0;

            icRahyabStatus = rahTotalDelay > DelayThreshold ? 3 : rahTotalDelay > twoThird ? 2 : rahTotalDelay > half ? 1 : 0;

            haAtieStatus = haadpTotalDelay > DelayThreshold
                ? 3
                : haadpTotalDelay > twoThird ? 2 : haadpTotalDelay > half ? 1 : 0;

            haSmartStatus = haSmartTotalDelay > DelayThreshold
                ? 3
                : haSmartTotalDelay > twoThird ? 2 : haSmartTotalDelay > half ? 1 : 0;

            haRahyabStatus = haRahTotalDelay > DelayThreshold
                ? 3
                : haRahTotalDelay > twoThird ? 2 : haRahTotalDelay > half ? 1 : 0;

            jiringStatus = jirTotalDelay > DelayThreshold ? 3 : jirTotalDelay > twoThird ? 2 : jirTotalDelay > half ? 1 : 0;
        }

        private long GetDelay(int type)
        {
            var mySmSs = _smsClasses.FindAll(item => item.RevcieveSms.Type == type);
            var currCount = 0;
            long totalDelay = 0;
            foreach (var sms in mySmSs)
            {
                currCount++;
                totalDelay += sms.RevcieveSms.Delay;
            }

            if (currCount != 0)
                totalDelay /= currCount;
            return totalDelay;
        }

        public void CreateDataForNotRecieved()
        {

            var dateTime = DateTime.Now;

            var allAdp =
                Convert.ToDouble(
                    _sendSmses.FindAll(
                        item => item.sms.SendTime.AddSeconds(WaitThresholdTime) < dateTime && item.sms.Type == 0)
                        .Count);

            var allhaadp =
                Convert.ToDouble(
                    _sendSmses.FindAll(
                        item => item.sms.SendTime.AddSeconds(WaitThresholdTime) < dateTime && item.sms.Type == 4)
                        .Count);

            var allSmart =
                Convert.ToDouble(
                    _sendSmses.FindAll(
                        item => item.sms.SendTime.AddSeconds(WaitThresholdTime) < dateTime && item.sms.Type == 1)
                        .Count);

            var allHaSmart =
                Convert.ToDouble(
                    _sendSmses.FindAll(
                        item => item.sms.SendTime.AddSeconds(WaitThresholdTime) < dateTime && item.sms.Type == 5)
                        .Count);

            var allRah =
                Convert.ToDouble(
                    _sendSmses.FindAll(
                        item => item.sms.SendTime.AddSeconds(WaitThresholdTime) < dateTime && item.sms.Type == 2)
                        .Count);

            var allHaRah =
                Convert.ToDouble(
                    _sendSmses.FindAll(
                        item => item.sms.SendTime.AddSeconds(WaitThresholdTime) < dateTime && item.sms.Type == 6)
                        .Count);

            var allJir =
                Convert.ToDouble(
                    _sendSmses.FindAll(
                        item => item.sms.SendTime.AddSeconds(WaitThresholdTime) < dateTime && item.sms.Type == 3)
                        .Count);

            SetNotRecievedStatus(allAdp, allSmart, allRah, allHaSmart, allHaRah, allhaadp, allJir);

            UpdateNotRecievedNotifications(allAdp, dateTime, allhaadp, allSmart, allHaSmart, allRah, allHaRah, allJir);

            while (_NotRecievedList.Count > ChartsInterVals)
                _NotRecievedList.RemoveAt(0);


            var o = new MyChartObject(allAdp, allSmart, allRah, allJir, allhaadp, allHaSmart, allHaRah, dateTime);

            _NotRecievedList.Add(o);
        }

        private void SetNotRecievedStatus(double allAdp, double allSmart, double allRah, double allHaSmart, double allHaRah,
            double allhaadp, double allJir)
        {
            var half = NotRecievedThreshold/2;
            var twoThird = 2*NotRecievedThreshold/3;

            icAtieStatus += allAdp >= NotRecievedThreshold ? 3 : allAdp > twoThird ? 2 : allAdp > half ? 1 : 0;
            icSmartStatus += allSmart >= NotRecievedThreshold ? 3 : allSmart > twoThird ? 2 : allSmart > half ? 1 : 0;
            icRahyabStatus += allRah >= NotRecievedThreshold ? 3 : allRah > twoThird ? 2 : allRah > half ? 1 : 0;
            haSmartStatus += allHaSmart >= NotRecievedThreshold ? 3 : allHaSmart > twoThird ? 2 : allHaSmart > half ? 1 : 0;
            haRahyabStatus += allHaRah >= NotRecievedThreshold ? 3 : allHaRah > twoThird ? 2 : allHaRah > half ? 1 : 0;
            haAtieStatus += allhaadp >= NotRecievedThreshold ? 3 : allhaadp > twoThird ? 2 : allhaadp > half ? 1 : 0;
            jiringStatus += allJir >= NotRecievedThreshold ? 3 : allJir > twoThird ? 2 : allJir > half ? 1 : 0;
        }

        #endregion

        #region notification region

        private void UpdateDelayNotifications(double adpIcDelay, DateTime dateTime, double adpHaDelay,
            double smartIcDelay,
            double smartHaDelay, double rahIcDelay, double rahHaDelay, double allJir)
        {
            var type = 1;
            var msg = "تاخیر در دریافت";
            var sender = "IC_ADP";
            if (adpIcDelay > DelayThreshold)
            {
                if (!_isIcAtieDelay)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _isIcAtieDelay = true;
                }
            }
            else if (_isIcAtieDelay)
            {
                UpdateNotification(type, sender);
                _isIcAtieDelay = false;
            }

            sender = "HA_ADP";
            if (adpHaDelay > DelayThreshold)
            {
                if (!_ishaAtieDelay)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _ishaAtieDelay = true;
                }
            }
            else if (_ishaAtieDelay)
            {
                UpdateNotification(type, sender);
                _ishaAtieDelay = false;
            }
            sender = "IC_Smart";
            if (smartIcDelay > DelayThreshold)
            {
                if (!_isIcSmartDelay)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _isIcSmartDelay = true;
                }
            }
            else if (_isIcSmartDelay)
            {
                UpdateNotification(type, sender);
                _isIcSmartDelay = false;
            }
            sender = "HA_Smart";
            if (smartHaDelay > DelayThreshold)
            {
                if (!_ishaSmartDelay)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _ishaSmartDelay = true;
                }
            }
            else if (_ishaSmartDelay)
            {
                UpdateNotification(type, sender);
                _ishaSmartDelay = false;
            }
            sender = "IC_Rah";
            if (rahIcDelay > DelayThreshold)
            {
                if (!_isIcRahyabDelay)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _isIcRahyabDelay = true;
                }
            }
            else if (_isIcRahyabDelay)
            {
                UpdateNotification(type, sender);
                _isIcRahyabDelay = false;
            }
            sender = "HA_Rah";
            if (rahHaDelay > DelayThreshold)
            {
                if (!_ishaRahyabDelay)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _ishaRahyabDelay = true;
                }
            }
            else if (_ishaRahyabDelay)
            {
                UpdateNotification(type, sender);
                _ishaRahyabDelay = false;
            }

            sender = "Jiring";

            if (allJir > DelayThreshold)
            {
                if (!_isJiringDelay)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _isJiringDelay = true;
                }
            }
            else if (_isJiringDelay)
            {
                UpdateNotification(type, sender);
                _isJiringDelay = false;
            }
        }

        private void UpdateNotRecievedNotifications(double allAdp, DateTime dateTime, double allhaadp, double allSmart,
            double allHaSmart, double allRah, double allHaRah, double allJir)
        {
            var type = 0;
            var msg = "نرسیده ها";
            var sender = "IC_ADP";
            if (allAdp > NotRecievedThreshold)
            {
                if (!_isIcAtieNotRecieved)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _isIcAtieNotRecieved = true;
                }
            }
            else if (_isIcAtieNotRecieved)
            {
                UpdateNotification(type, sender);
                _isIcAtieNotRecieved = false;
            }

            sender = "HA_ADP";
            if (allhaadp > NotRecievedThreshold)
            {
                if (!_ishaAtieNotRecieved)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _ishaAtieNotRecieved = true;
                }
            }
            else if (_ishaAtieNotRecieved)
            {
                UpdateNotification(type, sender);
                _isIcAtieNotRecieved = false;
            }
            sender = "IC_Smart";
            if (allSmart > NotRecievedThreshold)
            {
                if (!_isIcSmartNotRecieved)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _isIcSmartNotRecieved = true;
                }
            }
            else if (_isIcSmartNotRecieved)
            {
                UpdateNotification(type, sender);
                _isIcSmartNotRecieved = false;
            }
            sender = "HA_Smart";
            if (allHaSmart > NotRecievedThreshold)
            {
                if (!_ishaSmartNotRecieved)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _ishaSmartNotRecieved = true;
                }
            }
            else if (_ishaSmartNotRecieved)
            {
                UpdateNotification(type, sender);
                _ishaSmartNotRecieved = false;
            }
            sender = "IC_Rah";
            if (allRah > NotRecievedThreshold)
            {
                if (!_isIcRahyabNotRecieved)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _isIcRahyabNotRecieved = true;
                }
            }
            else if (_isIcRahyabNotRecieved)
            {
                UpdateNotification(type, sender);
                _isIcRahyabNotRecieved = false;
            }
            sender = "HA_Rah";
            if (allHaRah > NotRecievedThreshold)
            {
                if (!_ishaRahyabNotRecieved)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _ishaRahyabNotRecieved = true;
                }
            }
            else if (_ishaRahyabNotRecieved)
            {
                UpdateNotification(type, sender);
                _ishaRahyabNotRecieved = false;
            }

            sender = "Jiring";

            if (allJir > NotRecievedThreshold)
            {
                if (!_isJiringNotRecieved)
                {
                    AddNotification(msg, sender, dateTime, type);
                    _isJiringNotRecieved = true;
                }
            }
            else if (_isJiringNotRecieved)
            {
                UpdateNotification(type, sender);
                _isJiringNotRecieved = false;
            }
        }

        private void AddNotification(string msg, string sender, DateTime dateTime, int type)
        {
            var notificationsClass = new NotificationsClass
            {
                Msg = msg,
                Senders = sender,
                StartTime = dateTime,
                Type = type
            };

            _notificationsClasses.Add(notificationsClass);
        }

        private void UpdateNotification(int type, string name)
        {
            var notificationsClass =
                _notificationsClasses.Find(
                    item =>
                        item.Type == type && item.Senders.Equals(name) &&
                        item.EndTime == DateTime.MinValue);
            _notificationsClasses.Remove(notificationsClass);
            notificationsClass.EndTime = DateTime.Now;
            _notificationsClasses.Add(notificationsClass);
        }

        #endregion

        #region work with Modem

        private void ClearAllReadedSmses(SerialPort port)
        {
            Thread.Sleep(2000);
            try
            {
                const string deleteReadedCommand = "AT+CMGD=1,3";
                if (_objclsSms.DeleteMsg(port, deleteReadedCommand))
                {
                    logger.Debug("Messages has deleted successfuly");
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception.Message + " : delete");
            }
            Thread.Sleep(2000);

        }

        private string ReadIrancell2Sms()
        {
            Thread.Sleep(1000);
            while (runningState)
            {
                Thread.Sleep(100);
                if (!isICEnabaled2 || _sendSmses.Count == 0) continue;
                //Read irancel2
                try
                {
                    var uCountSms = _objclsSms.CountSMSmessages(ic_port2);
                    if (uCountSms <= 0) continue;
                    var smsIndex = ReadSmses(2);

                    DeleteSmses(smsIndex, 2);
                }
                catch (Exception ex)
                {
                    logger.Error("ReadIrancell1Sms " + ex.Message);
                }
            }
            return null;
        }

        private string ReadIrancell1Sms()
        {
            Thread.Sleep(1000);
            while (runningState)
            {
                Thread.Sleep(100);
                if (!isICEnabaled || _sendSmses.Count == 0) continue;
                //Read irancel
                try
                {
                    var uCountSms = _objclsSms.CountSMSmessages(ic_port);
                    if (uCountSms <= 0) continue;
                    var smsIndex = ReadSmses(0);

                    DeleteSmses(smsIndex, 0);
                }
                catch (Exception ex)
                {
                    logger.Error("ReadIrancell1Sms " + ex.Message);
                }
            }
            return null;
        }

        private string ReadHamrahAvval1Sms()
        {
            while (runningState)
            {
                Thread.Sleep(100);
                if (!isHaenabaled || _sendSmses.Count == 0) continue;
                //read hamrah avval 
                try
                {
                    var uCountSms = _objclsSms.CountSMSmessages(ha_port);
                    if (uCountSms <= 0) continue;
                    var smsIndex = ReadSmses(1);

                    DeleteSmses(smsIndex, 1);
                }
                catch (Exception ex)
                {
                    logger.Error("ReadHamrahAvvalSms " + ex.Message);
                }
            }
            return null;
        }

        private string ReadHamrahAvval2Sms()
        {
            while (runningState)
            {
                Thread.Sleep(100);
                if (!isHaenabaled2 || _sendSmses.Count == 0) continue;
                //read hamrah avval 
                try
                {
                    var uCountSms = _objclsSms.CountSMSmessages(ha_port2);
                    if (uCountSms <= 0) continue;
                    var smsIndex = ReadSmses(3);

                    DeleteSmses(smsIndex, 3);
                }
                catch (Exception ex)
                {
                    logger.Error("ReadHamrahAvval2Sms " + ex.Message);
                }
            }
            return null;
        }

        private List<int> ReadSmses(int type)
        {
            var port = type == 0 ? ic_port : type == 1 ? ha_port : type == 2 ? ic_port2 : ha_port2;
            //const string readSmsCommand = "AT+CMGL=\"ALL\"";
            const string readSmsCommand = "AT+CMGL=\"REC UNREAD\"";
            _objShortMessageCollection = _objclsSms.ReadSMS(port, readSmsCommand);

            var smsIndex = new List<int>();

            foreach (var msg in _objShortMessageCollection)
            {
                smsIndex.Add(Convert.ToInt32(msg.Index));
                if (!msg.Sender.Contains("JIRING") && !msg.Sender.Contains("733"))
                {
                    continue;
                }
                var message = msg.Message;
                if (!message.Contains(","))
                {
                    var builder = new StringBuilder();
                    var i1 = message.Length/4;

                    for (var i = 0; i < i1; i++)
                    {
                        var a = message.Substring(i*4, 4);

                        var array = a.ToCharArray();

                        var aaa = (array[2] > 64 ? array[2] - 55 : array[2] - 48)*16 +
                                  (array[3] > 64 ? array[3] - 55 : array[3] - 48);

                        var aa = (char) aaa;

                        builder.Append(aa);
                    }
                    message = builder.ToString();
                }
                if (!message.Contains(",")) continue;
                var strings = message.Split(',');
                var myId = Convert.ToInt32(strings[0]);
                var mySms = _sendSmses.Find(sms => sms.sms.SmsId == myId);
                if (mySms == null) continue;
                _sendSmses.Remove(mySms);

                var delay = DateTime.Now.Ticks - mySms.sms.SendTime.Ticks;

                delay /= (TimeSpan.TicksPerMillisecond*1000);

                var smsClass = new RevcieveSms(myId,delay, Convert.ToInt32(strings[1]), strings[2]);

                new RecievedSmsWrapper(_smsClasses, smsClass);
            }
            return smsIndex;
        }

        private void DeleteSmses(List<int> smsIndex,int type)
        {
            var port = type == 0 ? ic_port : type == 1 ? ha_port : type == 2 ? ic_port2 : ha_port2;
            smsIndex.Sort();

            var count = smsIndex.Count;

            for (var i = count; i > 0; i--)
            {
                var deleteReadedCommand = "";
                try
                {
                    deleteReadedCommand = "AT+CMGD=" + smsIndex[i - 1] + ",0";
                }
                catch (Exception e)
                {
                    logger.Error(e.Message + i + " , " + smsIndex.Count);
                }
                try
                {
                    if (_objclsSms.DeleteMsg(port, deleteReadedCommand))
                    {
                        //                                logger.error("Messages has deleted successfuly" );
                    }
                }
                catch (Exception exception)
                {
                    logger.Error("single delete" + exception.Message + i);
                }
                Thread.Sleep(100);
            }
            try
            {
                if (_objclsSms.CountSMSmessages(port) > 20)
                {
                    ClearAllReadedSmses(port);
                }
            }
            catch (Exception e)
            {
                logger.Error("cant read _revcieveSms count " + e.Message);
            }
        }

        #endregion  

        #region sendSmsFunctions

        public string SendSms()
        {
            while (runningState)
            {
                if (isICEnabaled)
                {
                    if (ActiveADP)
                        Task<string>.Factory.StartNew(IcAdpSendSms);
                    if (ActiveSmart)
                        Task<string>.Factory.StartNew(IcSmartSendSms);
                    if (ActiveRahnama)
                        Task<string>.Factory.StartNew(IcSendSmsRahyab);
                }
                if (isICEnabaled2)
                {
                    if (ActiveADP)
                        Task<string>.Factory.StartNew(IcAdpSendSms2);
                    if (ActiveSmart)
                        Task<string>.Factory.StartNew(IcSmartSendSms2);
                    if (ActiveRahnama)
                        Task<string>.Factory.StartNew(IcSendSmsRahyab2);
                }

                if (isHaenabaled)
                {
                    if (ActiveADP)
                        Task<string>.Factory.StartNew(HaAdpSendSms);
                    if (ActiveSmart)
                        Task<string>.Factory.StartNew(HaSmartSendSms);
                    if (ActiveRahnama)
                        Task<string>.Factory.StartNew(HaSendSmsRahyab);
                    if (ActiveJiring)
                        Task<string>.Factory.StartNew(SendJiring1);
                }

                if (isHaenabaled2)
                {
                    if (ActiveADP)
                        Task<string>.Factory.StartNew(HaAdpSendSms2);
                    if (ActiveSmart)
                        Task<string>.Factory.StartNew(HaSmartSendSms2);
                    if (ActiveRahnama)
                        Task<string>.Factory.StartNew(HaSendSmsRahyab2);
                    if (ActiveJiring)
                        Task<string>.Factory.StartNew(SendJiring2);
                }
                Thread.Sleep(SendSmsInterval * 1000);
            }
            return null;
        }

        public string IcAdpSendSms()
        {
            var type = 0;
            SendSmsWithAdp(type);
            return null;
        }
        public string IcAdpSendSms2()
        {
            var type = 2;
            SendSmsWithAdp(type);
            return null;
        }

        public string HaAdpSendSms()
        {
            var type = 1;
            SendSmsWithAdp(type);
            return null;
        }
        public string HaAdpSendSms2()
        {
            var type = 1;
            SendSmsWithAdp(type);
            return null;
        }

        private void SendSmsWithAdp(int type)
        {
            var senderType = type == 0 ? 0 : 4;
            var phoneNumber = type == 0 ? IC_PhoneNumber : type == 1 ? HA_PhoneNumber : type == 2?IC_PhoneNumber2:HA_PhoneNumber2;

            try
            {
                var serviceProvider
                    = new ADPWebRef.JaxRpcMessagingServiceClient();
                var increment = Interlocked.Increment(ref _counter);
                var msg1 = new ADPWebRef.MultiAddressMessageObject()
                {
                    phoneNumbers = new[] {"98" + phoneNumber},
                    content = increment + "," + senderType + ",ADP",
                    clientIds = new string[] {"01"},
                    dueTime = DateTime.Now,
                    encoding = 2
                };
                var msg
                    = new ADPWebRef.MultiAddressMessageObject[1];
                msg[0] = msg1;

                serviceProvider.sendMultiple("pershian01", "pershian733", "982000733",
                    "", "", 1, true, msg);

                var sms = new SendSms {SendTime = DateTime.Now, Type = senderType, TypeName = "Adp", SmsId = increment};

                new SendSmsWrapper(_sendSmses, sms);
            }
            catch (Exception ex)
            {
                logger.Error("ADP : " + ex.Message);
                var sendFailNotifications = new SendFailNotifications
                {
                    Msg = ex.Message,
                    Operator = "آتیه",
                    Problem = "پیامک را ارسال نمیکند",
                    Time = DateTime.Now
                };
                _channelProblemClasses.Add(sendFailNotifications);
            }
        }

        public string IcSmartSendSms()
        {
            var type = 0;
            SendSmsWithSmart(type);
            return null;
        }
        public string HaSmartSendSms()
        {
            var type = 1;
            SendSmsWithSmart(type);
            return null;
        }


        public string IcSmartSendSms2()
        {
            var type = 0;
            SendSmsWithSmart(type);
            return null;
        }
        public string HaSmartSendSms2()
        {
            var type = 1;
            SendSmsWithSmart(type);
            return null;
        }

        private void SendSmsWithSmart(int type)
        {
            var senderType = type == 0 ? 1 :5;
            var phoneNumber = type == 0 ? IC_PhoneNumber : type == 1 ? HA_PhoneNumber : type == 2 ? IC_PhoneNumber2 : HA_PhoneNumber2; 
            try
            {
                var increment = Interlocked.Increment(ref _counter);
                var xmlRequest = "<xmsrequest><userid>16609</userid>"
                                 + "<password>12345@aA</password>"
                                 + "<action>smssend</action><body><type>oto</type>"
                                 + "<recipient "
                                 + " doerid=" + "\"" + "1" + "\""
                                 + " orginator=" + "\"" + "500029991" + "\""
                                 + " mobile=" + "\"" + "0" + phoneNumber + "\""
                                 + ">" + increment + "," + senderType + ",Smart" +
                                 "</recipient></body></xmsrequest>";

                var xmlMessage = new XmlDocument();
                xmlMessage.LoadXml(xmlRequest);

                var remoteProvider = new SmartIPWebRef.SmsSoapClient();
                //                remoteProvider. = 5000;
                var response = remoteProvider.XmsRequest(xmlMessage.InnerXml);

                var responseMessage = new XmlDocument();
                responseMessage.LoadXml(response.ToString());

                (from XmlNode node in responseMessage.SelectNodes("//text()") select node.InnerText).ToList();

                var sms = new SendSms {SendTime = DateTime.Now, Type = senderType, TypeName = "Smart", SmsId = increment};

                new SendSmsWrapper(_sendSmses, sms);
            }
            catch (Exception ex)
            {
                logger.Error("Smart : " + ex.Message);
                var sendFailNotifications = new SendFailNotifications
                {
                    Msg = ex.Message,
                    Operator = "اسمارت",
                    Problem = "پیامک را ارسال نمیکند",
                    Time = DateTime.Now
                };
                _channelProblemClasses.Add(sendFailNotifications);
            }
        }

        public string IcSendSmsRahyab()
        {
            var type = 0;
            SendSmsWithRahyab(type);
            return null;
        }

        public string HaSendSmsRahyab()
        {
            var type = 1;
            SendSmsWithRahyab(type);
            return null;
        }

        public string IcSendSmsRahyab2()
        {
            var type = 0;
            SendSmsWithRahyab(type);
            return null;
        }

        public string HaSendSmsRahyab2()
        {
            var type = 1;
            SendSmsWithRahyab(type);
            return null;
        }

        private void SendSmsWithRahyab(int type)
        {
            var senderType = type == 0 ? 2 : 6;
            var phoneNumber = type == 0 ? IC_PhoneNumber : type == 1 ? HA_PhoneNumber : type == 2 ? IC_PhoneNumber2 : HA_PhoneNumber2;
            try
            {
                var targetNumber = new string[1];
                targetNumber[0] = "0" + phoneNumber;
                var mySmsSendingClassInstance = new Cls_SMS.SMSSend();
                var incrementedId = Interlocked.Increment(ref _counter);
                var smsOut = mySmsSendingClassInstance.SendSMS_Batch(incrementedId + "," + senderType + ",Rah", targetNumber,
                    "kish",
                    "kish15705",
                    "1000733",
                    "http://193.104.22.14:9100/CPSMSService/Access",
                    "RAHYAB",
                    "RAHYAB",
                    true, false);
                
                var sms = new SendSms {SendTime = DateTime.Now, Type = senderType, TypeName = "Rah", SmsId = incrementedId};

                new SendSmsWrapper(_sendSmses, sms);
            }
            catch (Exception ex)
            {
                logger.Error("Rahyab" + ex.Message);
                var sendFailNotifications = new SendFailNotifications
                {
                    Msg = ex.Message,
                    Operator = "رهیاب",
                    Problem = "پیامک را ارسال نمیکند",
                    Time = DateTime.Now
                };
                _channelProblemClasses.Add(sendFailNotifications);
            }
        }

        public string SendJiring1()
        {
            SendSmsJiring(0);
            return null;
        }

        public string SendJiring2()
        {
            SendSmsJiring(0);
            return null;
        }

        public string SendSmsJiring(int type)
        {
            var phoneNumber = type == 0 ? HA_PhoneNumber : HA_PhoneNumber2;
            try
            {
                const string username = "JIRING";
                const string password = "93889e49d6e3701b0a7d40e4b376bb90";
                var incrementedId = Interlocked.Increment(ref _counter);
                var messages = new[] { incrementedId + ",3,JIR" };
                var destinations = new[] { ("+" + "98" + phoneNumber) };
                var originators = new[] {"7007071234567"};

                var udhs = new[] {""};
                var mClass = new[] {""};
                var serviceIds = new[] {"7007071234567"};

                var sendingRef = new MAPFAWebRefrence.HelloWorldClient();
                sendingRef.ServiceSend(username, password, "alladmin", 0, messages, destinations, originators, udhs,
                    mClass, serviceIds);
                var sms = new SendSms { SendTime = DateTime.Now, Type = 3, TypeName = "Jir", SmsId = incrementedId };

                new SendSmsWrapper(_sendSmses, sms);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message +" : Jiring");
                var sendFailNotifications = new SendFailNotifications
                {
                    Msg = ex.Message,
                    Operator = "جیرینگ",
                    Problem = "پیامک را ارسال نمیکند",
                    Time = DateTime.Now
                };
                _channelProblemClasses.Add(sendFailNotifications);
            }
            return null;
        }

        #endregion

        #region button functions
        private void RadButton_Click(object sender, RoutedEventArgs e)
        {
            var window2 = new SettingWindow(IC_PhoneNumber, HA_PhoneNumber, IC_PhoneNumber2, HA_PhoneNumber2, SendSmsInterval, RefreshTime, WaitThresholdTime, SendCleanUpTime, RecieveCleanUpTime, DelayThreshold,
                NotRecievedThreshold,ActiveADP,ActiveSmart,ActiveJiring,ActiveRahnama,ChartsInterVals,isICEnabaled,isHaenabaled);
            window2.ShowDialog();
        }

        public static void SetConfigs(string icphoneNumber, string haPhoneNumber, string icphoneNumber2, string haPhoneNumber2, int smsSendInterval, int refreshTime, int waitThresholdTime,
            int sendCleanUpTime,int recieveCleanUpTime,long delayThreshold,int notRecievedThreshold,bool atieCheckBox,
            bool smartCheckBox,bool jiringCheckBox,bool rahyabCheckBox,int chartsIntevals,bool monitorIC,bool monitorHA)
        {
            var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            IC_PhoneNumber = icphoneNumber;
            HA_PhoneNumber = haPhoneNumber;
            IC_PhoneNumber2 = icphoneNumber2;
            HA_PhoneNumber2 = haPhoneNumber2;

            SendSmsInterval = smsSendInterval;
            RefreshTime = refreshTime;
            WaitThresholdTime = waitThresholdTime;
            SendCleanUpTime = sendCleanUpTime;
            RecieveCleanUpTime = recieveCleanUpTime;
            DelayThreshold = delayThreshold;
            NotRecievedThreshold = notRecievedThreshold;

            ActiveJiring = jiringCheckBox;
            ActiveADP = atieCheckBox;
            ActiveRahnama = rahyabCheckBox;
            ActiveSmart = smartCheckBox;
            ChartsInterVals = chartsIntevals;
            isICEnabaled = monitorIC;
            isHaenabaled = monitorHA;

            if (appPath == null) return;
            var configFile = Path.Combine(appPath, "App.config");
            var configFileMap = new ExeConfigurationFileMap {ExeConfigFilename = configFile};
            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            try
            {
                ConfigurationManager.AppSettings["IC_PhoneNumber"]  = icphoneNumber;
                ConfigurationManager.AppSettings["HA_PhoneNumber"]  = haPhoneNumber;
                ConfigurationManager.AppSettings["IC_PhoneNumber2"]  = icphoneNumber2;
                ConfigurationManager.AppSettings["HA_PhoneNumber2"]  = haPhoneNumber2;

                ConfigurationManager.AppSettings["SendSmsInterval"]  = smsSendInterval.ToString();
                ConfigurationManager.AppSettings["RefreshTime"]  = refreshTime.ToString();
                ConfigurationManager.AppSettings["WaitThresholdTime"]  = waitThresholdTime.ToString();
                ConfigurationManager.AppSettings["SendCleanUpTime"]  = sendCleanUpTime.ToString();
                ConfigurationManager.AppSettings["RecieveCleanUpTime"]  = recieveCleanUpTime.ToString();
                ConfigurationManager.AppSettings["DelayThreshold"]  = delayThreshold.ToString();
                ConfigurationManager.AppSettings["NotRecievedThreshold"]  = notRecievedThreshold.ToString();
                ConfigurationManager.AppSettings["ChartsIntervals"]  = chartsIntevals.ToString();


                config.Save();
            }
            catch (Exception e)
            {
//                logger.Error("SetConfig : "+e.Message);
            }
        }

        private bool runningState = false;

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!runningState)
            {
                StartStopButton.Content = "توقف";
                runningState = true;
                AsyncTasks();
            }
            else
            {
                StartStopButton.Content = "شروع";
                runningState = false;
            }
        }

        private void RadButton_Click_1(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(120000);

            try
            {
                const string deleteReadedCommand = "AT+CMGD=1,4";
                if (_objclsSms.DeleteMsg(this.ic_port, deleteReadedCommand))
                {
                    logger.Error("تمام پیامها موفقیت آمیز پاک شدند");
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception.Message + "clear all");
            }

            SmsRadChart.ItemsSource = null;
            SmsRadChart2.ItemsSource = null;
        }
        #endregion

    }
}