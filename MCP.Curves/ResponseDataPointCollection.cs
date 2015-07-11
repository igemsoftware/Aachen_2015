using Microsoft.Research.DynamicDataDisplay.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCP.Curves
{
    public class BiomassResponseDataCollection : RingArray<BiomassResponseData>
    {
        //private const int TOTAL_POINTS = ViewModel.SecondsToShow * 1000 / ViewModel.UpdateInterval;
        private const int TOTAL_POINTS = 1000;

        public BiomassResponseDataCollection()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }
    public class GasSensorResponseDataCollection : RingArray<GasSensorResponseData>
    {
        //private const int TOTAL_POINTS = ViewModel.SecondsToShow * 1000 / ViewModel.UpdateInterval;
        private const int TOTAL_POINTS = 1000;

        public GasSensorResponseDataCollection()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }
    public class PumpResponseDataCollection : RingArray<PumpResponseData>
    {
        //private const int TOTAL_POINTS = ViewModel.SecondsToShow * 1000 / ViewModel.UpdateInterval;
        private const int TOTAL_POINTS = 1000;

        public PumpResponseDataCollection()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }
}
