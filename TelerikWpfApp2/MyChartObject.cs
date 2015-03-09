using System;
using System.Collections.ObjectModel;

namespace TelerikWpfApp2
{
    public class MyChartObject
    {
        public double IcApdVal { get; set; }
        public double IcSmartVal { get; set; }
        public double IcRahVal { get; set; }
        public double JiringVal { get; set; }
        public double HaApdVal { get; set; }
        public double HaSmartVal { get; set; }
        public double HaRahVal { get; set; }
        public string StartTime { get; set; }



        public MyChartObject(double icApdVal, double icSmartVal, double icRahVal, double jiringVal, double haAdpVal,double haSmartVal,double haRahVal, DateTime time)
        {
            IcApdVal = icApdVal;
            IcSmartVal = icSmartVal;
            IcRahVal = icRahVal;
            
            JiringVal = jiringVal;
            HaApdVal = haAdpVal;
            HaSmartVal = haSmartVal;
            HaRahVal = haRahVal;

            StartTime = time.ToString("h:mm:ss");
        }

        public static ObservableCollection<MyChartObject> GetData(int count)
        {
            var r = new Random();
            var result = new ObservableCollection<MyChartObject>();

            for (var i = 0; i < count; i++)
            {
                result.Add(new MyChartObject(r.Next(0, 100), r.Next(0, 100), r.Next(0, 100), r.Next(0, 100), r.Next(0, 100), r.Next(0, 100), r.Next(0, 100), DateTime.Now));
            }

            return result;
        }
    }
}