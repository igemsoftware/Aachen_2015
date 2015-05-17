using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TCD.Controls;

namespace MCP.Protocol
{
    public enum ParticipantID
    {
        MCP = 0,
        Master = 1,
        [Display(Name = "Reactor 1")]
        Reactor_1 = 2,
        [Display(Name = "Reactor 2")]
        Reactor_2 = 3,
        [Display(Name = "Reactor 3")]
        Reactor_3 = 4,
        [Display(Name = "Reactor 4")]
        Reactor_4 = 5,
        [Display(Name = "Reactor 5")]
        Reactor_5 = 6,
        [Display(Name = "Reactor 6")]
        Reactor_6 = 7,
        [Display(Name = "Reactor 7")]
        Reactor_7 = 8,
        [Display(Name = "Reactor 8")]
        Reactor_8 = 9,
        [Display(Name = "Reactor 9")]
        Reactor_9 = 10,
        [Display(Name = "Reactor 10")]
        Reactor_10 = 11,
        [Display(Name = "Reactor 11")]
        Reactor_11 = 12,
        [Display(Name = "Reactor 12")]
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
        public static string Temperature = "T";
        public static string Biomass_Concentration = "X";
        public static string O2_Saturation = "O2";
        public static string CO2_Saturation = "CO2";
        public static string CHx_Saturation = "CHx";

        public static string[] ControlParameters = new string[] { Dilution_Rate, Agitation_Rate, Temperature, Aeration_Rate };
        public static string[] MeasuredParameters = new string[] { Biomass_Concentration, O2_Saturation, CO2_Saturation, CHx_Saturation };
        public static Dictionary<string, Color> ParameterColors { get; set; }

        static DimensionSymbol()
        {
            ParameterColors = new Dictionary<string, Color>();
            ParameterColors.Add(Dilution_Rate, DesignColors.Blue);
            ParameterColors.Add(Agitation_Rate, DesignColors.DarkGrey);
            ParameterColors.Add(Temperature, DesignColors.Red);
            ParameterColors.Add(Aeration_Rate, DesignColors.LightGrey);
            //
            ParameterColors.Add(Biomass_Concentration, DesignColors.Yellow);
            ParameterColors.Add(O2_Saturation, DesignColors.Blue);
            ParameterColors.Add(CO2_Saturation, DesignColors.DarkGrey);
            ParameterColors.Add(CHx_Saturation, DesignColors.Red);
        }
    }
    public static class Unit
    {
        public static string SPH = "sph";
        public static string RPM = "rpm";
        public static string VVM = "vvm";
        public static string Celsius = "°C";
        public static string PerHour = "1/h";
        public static string Percent = "%";
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
