using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleContainer
{
 
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyInjectAttribute : Attribute { }

}
