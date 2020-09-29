using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendPics
{
    public class User
    {
        public int UserId { get; set; }
        public string Nickname { get; set; }
        public long TelegramId { get; set; }
        public virtual List<Image> Images { get; set; }
    }
}
