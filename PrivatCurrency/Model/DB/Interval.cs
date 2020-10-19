using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrivatCurrency
{
    public class Interval
    {
        public int IntervalId { get; set; }
        public int IntervalMinutes { get; set; }
        public int SendFromMinutes { get; set; }
        public int SendToMinutes { get; set; }
        public int WhenToSendMinutes { get; set; }
    }
}
