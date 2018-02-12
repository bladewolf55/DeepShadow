using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepShadow
{
    public class StartedItem
    {
        public string ClassVariable { get; set; }
        public object Item { get; set; }

        public StartedItem(string classVariable, object item)
        {
            ClassVariable = classVariable;
            Item = item;
        }
    }
}
