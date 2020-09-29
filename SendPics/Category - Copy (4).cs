using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendPics
{
    public class Image
    {
        public int ImageId { get; set; }
        public string Path { get; set; }
        public virtual Category Category { get; set; }
        public virtual User User { get; set; }
    }
}
