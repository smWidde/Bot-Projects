using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendPics
{
    class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public virtual List<Image> Images { get; set; }

    }
}
