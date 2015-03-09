using System;
using System.Collections.Generic;
using System.Timers;

namespace TelerikWpfApp2
{
    public class RecievedSmsWrapper
    {
        private List<RecievedSmsWrapper> list;
        private RevcieveSms _revcieveSms;

        public RevcieveSms RevcieveSms
        {
            get { return _revcieveSms; }
        }

        public static int WaitTime = 120000;

        private Timer timer;

        public RecievedSmsWrapper(List<RecievedSmsWrapper> _list, RevcieveSms revcieveSms)
        {
            list = _list;
            _revcieveSms = revcieveSms;

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