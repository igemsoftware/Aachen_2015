﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using MCP.Cultivation;
using TCD;
using TCD.Controls;
using MCP.Measurements;
using MCP.Protocol;
using System.Collections;

namespace MasterControlProgram
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            graphsFilterLeft.ItemsSource = DimensionSymbol.ControlParameters;
            graphsFilterRight.ItemsSource = DimensionSymbol.MeasuredParameters;
            graphsFilterLeft.SelectAll();
            graphsFilterRight.SelectAll();
        }

        private void SelectedCultivation_Changed(object sender, SelectionChangedEventArgs e)
        {
            foreach (Cultivation cultivation in e.RemovedItems)
            {
                if (cultivation != null)
                {
                    foreach (DataLogBase log in cultivation.LiveLogs.Values)
                        log.DeactivatePlot();
                    foreach (DataPostprocessingLog log in cultivation.PostprocessingLogs.Values)
                        log.DeactivatePlot();
                }
            }
            foreach (Cultivation cultivation in cultivationsSelectionBox.SelectedItems)
            {
                foreach (DataLogBase log in cultivation.LiveLogs.Values)
                    log.ActivatePlot();
                foreach (DataPostprocessingLog log in cultivation.PostprocessingLogs.Values)
                    log.ActivatePlot();
            }
            Redraw(graphsFilterLeft, plotterLeft);
            Redraw(graphsFilterRight, plotterRight);
        }
        private void FilterLeft_Changed(object sender, SelectionChangedEventArgs e)
        {
            Redraw(graphsFilterLeft, plotterLeft);
        }
        private void FilterRight_Changed(object sender, SelectionChangedEventArgs e)
        {
            Redraw(graphsFilterRight, plotterRight);
        }
        private void Redraw(ListBox graphsFilter, ChartPlotter plotter)
        {
            //remove old graphs
            for (int i = 0; i < plotter.Children.Count; i++)
            {
                if (plotter.Children[i] is LineGraph)
                {
                    plotter.Children.RemoveAt(i);
                    i--;
                }
            }
            //draw new graphs
            foreach (Cultivation cultivation in cultivationsSelectionBox.SelectedItems)
            {
                foreach (string param in graphsFilter.SelectedItems)
                {
                    switch (param)
                    {
                        case DimensionSymbol.Turbidity:
                        case DimensionSymbol.Biomass_Concentration:
                        case DimensionSymbol.O2_Saturation:
                        case DimensionSymbol.CO2_Saturation:
                        case DimensionSymbol.CHx_Saturation:
                            plotter.AddLineGraph((cultivation.PostprocessingLogs[param] as DataPostprocessingLog).DataSource, DimensionSymbol.ParameterColors[param], 2, param);
                            break;
                        default:
                            plotter.AddLineGraph(cultivation.LiveLogs[param].DataSource, DimensionSymbol.ParameterColors[param], 2, param);
                            break;
                    }
                }
            }
        }
    }
}
