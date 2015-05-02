using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD.Controls;

namespace MCP.Protocol
{
    public enum ParticipantID
    {
        MCP = 0,
        Master = 1,
        Reactor_1 = 2,
        Reactor_2 = 3,
        Reactor_3 = 4,
        Reactor_4 = 5,
        Reactor_5 = 6,
        Reactor_6 = 7,
        Reactor_7 = 8,
        Reactor_8 = 9,
        Reactor_9 = 10,
        Reactor_10 = 11,
        Reactor_11 = 12,
        Reactor_12 = 13
    }
    public enum MessageType
    {
        Data = 0,
        Command = 1,
        DataFormat = 2,
        CommandFormat = 3
    }
    public static class DimensionSymbol
    {
        public static string Dilution_Rate = "D";
        public static string Agitation_Rate = "n";
        public static string Aeration_Rate = "q_g";
        public static string Feed_Rate = "S_fin";
        public static string Harvest_Rate = "S_fout";
    }
    public static class Unit
    {
        public static string SPH = "sph";
        public static string RPM = "rpm";
    }
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
