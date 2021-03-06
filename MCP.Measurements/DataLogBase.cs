﻿using DynamicDataDisplay.Markers.DataSources;
using MCP.Curves;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace MCP.Measurements
{
    public class DataLogBase
    {
        public string _FilePath { get; set; }



        //private SensorDataPointCollection _SensorDataCollection = new SensorDataPointCollection();//contains only recent datapoints
        //[XmlIgnore]
        //public SensorDataPointCollection SensorDataCollection { get { return _SensorDataCollection; } set { _SensorDataCollection = value; } }

        private ObservableCollection<DataPoint> _SensorDataSet = new ObservableCollection<DataPoint>();//contains all datapoints
        [XmlIgnore]
        public ObservableCollection<DataPoint> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; } }

        

        [XmlIgnore]
        public EnumerableDataSource DataSource { get; set; }
        
        public delegate double CalculateRuntimeMethodDelegate(DateTime point);
        

        private bool IsPlotActivated { get; set; }


        public DataLogBase()
        {
            //DataSource = new EnumerableDataSource(SensorDataSet);
            //DataSource.DataToPoint = new Func<object, Point>(dp => new Point(((dp as DataPoint).Time - StartTime).TotalHours, (dp as DataPoint).YValue));
        }

        public void Initialize(params string[] headers)
        {
            bool existed = File.Exists(_FilePath);
            if (!existed)
            {
                StreamWriter writer = File.CreateText(_FilePath);
                writer.WriteLine(string.Join("\t", headers));
                writer.Flush();
                writer.Dispose();
            }
        }

        public virtual bool AddRawData(DataPoint data)
        {
            WriteLine(data.ToString());
            if (!IsPlotActivated)
                return true;
            try
            {
                SensorDataSet.Add(data);
            }
            catch { }
            return true;
        }
        public void WriteLine(string text)
        {
            StreamWriter writer = File.AppendText(_FilePath);
            writer.WriteLine(text);
            writer.Flush();
            writer.Dispose();
        }

        public void ActivatePlot(DataLogBase.CalculateRuntimeMethodDelegate  calculateRuntimeMethod)
        {
            IsPlotActivated = true;
            try
            {
                SensorDataSet.Clear();
            }
            catch { }
            //create the data source
            DataSource = new EnumerableDataSource(SensorDataSet);
            DataSource.DataToPoint = new Func<object, Point>(dp => new Point(calculateRuntimeMethod.Invoke((dp as DataPoint).Time), (dp as DataPoint).YValue));
            string[] lines = File.ReadAllLines(_FilePath);
            for (int i = 1; i < lines.Length; i++)
            {
                DataPoint dp = new DataPoint(lines[i]);
                try
                {
                    SensorDataSet.Add(dp);
                }
                catch
                {
                    //TODO: this feels dirty
                }
            }
        }
        public void DeactivatePlot()
        {
            IsPlotActivated = false;
            DataSource = null;
            if (SensorDataSet.Count > 0)
                try
                {
                    SensorDataSet.Clear();
                }
                catch { }
        }
    }
}
