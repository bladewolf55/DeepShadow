using System;

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
