using System.Collections.Generic;

namespace PrivatCurrency
{
    class StateUser
    {
        public User User { get; set; }
        public bool[] ChosenCurrs;
        public int State { get; set; }
        public int Interval { get; set; }
        public int SendTo { get; set; }
        public int SendFrom { get; set; }
        public int WhenToSend { get; set; }
        public List<int> IdOfMsgsOnDelete { get; set; }
        public StateUser(User user)
        {
            State = 0;
            User = user;
            ChosenCurrs = new bool[16];
            Interval = user.Interval.IntervalMinutes;
            SendTo = user.Interval.SendToMinutes;
            SendFrom = user.Interval.SendFromMinutes;
            WhenToSend = user.Interval.WhenToSendMinutes;
            IdOfMsgsOnDelete = new List<int>();
        }
        public void Update()
        {
            Interval = User.Interval.IntervalMinutes;
            SendTo = User.Interval.SendToMinutes;
            SendFrom = User.Interval.SendFromMinutes;
            WhenToSend = User.Interval.WhenToSendMinutes;
        }
        public void ChangeUser()
        {
            State = 0;
            User.Interval.IntervalMinutes = Interval;
            User.Interval.SendToMinutes = SendTo;
            User.Interval.SendFromMinutes = SendFrom;
            User.Interval.WhenToSendMinutes = WhenToSend;
        }
    }
}
