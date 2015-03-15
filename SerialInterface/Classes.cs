using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD.Controls;

namespace SerialInterface
{
    public enum BaudRate
    {
        [DisplayAttribute(Name = "300")]
        _300 = 300,
        [DisplayAttribute(Name = "600")]
        _600 = 600,
        [DisplayAttribute(Name = "1200")]
        _1200 = 1200,
        [DisplayAttribute(Name = "2400")]
        _2400 = 2400,
        [DisplayAttribute(Name = "4800")]
        _4800 = 4800,
        [DisplayAttribute(Name = "9600 (Default)")]
        _9600 = 9600,
        [DisplayAttribute(Name = "1440")]
        _14400 = 14400,
        [DisplayAttribute(Name = "19200")]
        _19200 = 19200,
        [DisplayAttribute(Name = "28800")]
        _28800 = 28800,
        [DisplayAttribute(Name = "31250")]
        _31250 = 31250,
        [DisplayAttribute(Name = "38400")]
        _38400 = 38400,
        [DisplayAttribute(Name = "57600")]
        _57600 = 57600,
        [DisplayAttribute(Name = "115200")]
        _115200 = 115200
    }
}
