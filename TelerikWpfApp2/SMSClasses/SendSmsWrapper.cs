using System;
using System.Collections.Generic;
using System.Timers;

namespace TelerikWpfApp2
{
    public class SendSmsWrapper
    {
        private List<SendSmsWrapper> list;
        private SendSms Sms;

        public SendSms sms
        {
            get { return Sms; }
        }

        public static int WaitTime = 300000;

        private Timer timer;

        public SendSmsWrapper(List<SendSmsWrapper> _list, SendSms _sms)
        {
            list = _list;
            Sms = _sms;

            list.Add(this);

            timer = new Timer { Interval = WaitTime };
            timer.Elapsed += Tick;
            timer.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            list.Remove(this);
        }
    }
}