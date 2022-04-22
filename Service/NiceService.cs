using ServiceInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service
{

    public class NiceService : INiceService
    {
        public string Prefix()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}
