/*
    MCP Bioreactor Control Software
    Copyright (C) 2015 iGEM Aachen (Michael Osthege, Sebastian Siegel, Tanya Bafna, Sayantan Dutta)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
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
using Microsoft.Research.DynamicDataDisplay.Markers2;

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
            plotterLeft.Legend.HorizontalAlignment = HorizontalAlignment.Left;
            plotterRight.Legend.HorizontalAlignment = HorizontalAlignment.Left;
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
                    log.ActivatePlot(cultivation.CalculateRuntime);
                foreach (DataPostprocessingLog log in cultivation.PostprocessingLogs.Values)
                    log.ActivatePlot(cultivation.CalculateRuntime);
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
                if (plotter.Children[i] is LineChart)
                {
                    //(plotter.Children[i] as LineChart).RemoveFromPlotter();
                    (plotter.Children[i] as LineChart).RemoveFromPlotter();
                    //plotter.Children.RemoveAt(i);
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
                            //TODO: modify this - the first plot comes onto the left y axis - all additional plots get their own y-axes (which have to be created!!)
                            plotter.AddLineChart(cultivation.PostprocessingLogs[param].DataSource).WithDescription(param).WithStroke(new SolidColorBrush(DimensionSymbol.ParameterColors[param])).WithStrokeThickness(2);
                            //plotter.AddLineGraph((cultivation.PostprocessingLogs[param] as DataPostprocessingLog).DataSource, DimensionSymbol.ParameterColors[param], 2, param);
                            break;
                        default:
                            plotter.AddLineChart(cultivation.LiveLogs[param].DataSource).WithDescription(param).WithStroke(new SolidColorBrush(DimensionSymbol.ParameterColors[param])).WithStrokeThickness(2);    
                            //plotter.AddLineGraph(cultivation.LiveLogs[param].DataSource, DimensionSymbol.ParameterColors[param], 2, param);
                            break;
                    }
                }
            }
        }
    }
}
