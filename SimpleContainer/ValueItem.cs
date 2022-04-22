using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleContainer
{
    public class ValueItem
    {
        public Type ImplementType { get; set; }

        public Type InterfaceType { get; set; }

        public object Instance { get; set; }

        public override string ToString()
        {
            if (InterfaceType == null)
            {
                return ImplementType.Name;
            }

            return $"{InterfaceType.Name}_{ImplementType.Name}";
        }
    }
}
