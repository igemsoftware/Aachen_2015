﻿using Microsoft.Research.DynamicDataDisplay.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCP.Curves
{
    public class ResponseDataPointCollection : RingArray<ResponseData>
    {
        //private const int TOTAL_POINTS = ViewModel.SecondsToShow * 1000 / ViewModel.UpdateInterval;
        private const int TOTAL_POINTS = 1000;

        public ResponseDataPointCollection()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }
}