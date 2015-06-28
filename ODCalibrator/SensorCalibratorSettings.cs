using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;
using TCD.Serialization.Json;

namespace ODCalibrator
{
    public class SensorCalibratorSettings : PropertyChangedBase
    {
        public ObservableCollection<CalibrationPoint> ODCalibrationPoints
        {
            get
            {
                string json = Properties.Settings.Default.ODInputs;
                if (string.IsNullOrWhiteSpace(json))
                    return DefaultODCalibrationPoints;
                var points = (ObservableCollection<CalibrationPoint>)JsonDeSerializer.DeserializeFromString(json, typeof(ObservableCollection<CalibrationPoint>));
                return points;
            }
            set
            {
                Properties.Settings.Default.ODInputs = JsonDeSerializer.SerializeToString(value);
                OnPropertyChanged();
                SaveSettings();
            }
        }

        private ObservableCollection<CalibrationPoint> DefaultODCalibrationPoints;


        public SensorCalibratorSettings()
        {
            DefaultODCalibrationPoints = new ObservableCollection<CalibrationPoint>();
            DefaultODCalibrationPoints.Add(new CalibrationPoint(0, 10));
        }


        private void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }
    }
}
