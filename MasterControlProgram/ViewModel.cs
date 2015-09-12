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
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TCD;
using MCP.Equipment;
using MCP.Protocol;
using MCP.Cultivation;
using MCP.Measurements;


namespace MasterControlProgram
{
    public class ViewModel : PropertyChangedBase
    {
        private MCPSettings _MCPSettings = new MCPSettings();
        public MCPSettings MCPSettings { get { return _MCPSettings; } set { _MCPSettings = value; OnPropertyChanged(); } }

        private Inventory _Inventory = new Inventory();
        public Inventory Inventory { get { return _Inventory; } set { _Inventory = value; OnPropertyChanged(); } }

        private ExperimentLibrary _ExperimentLibrary = new ExperimentLibrary();
        public ExperimentLibrary ExperimentLibrary { get { return _ExperimentLibrary; } set { _ExperimentLibrary = value; OnPropertyChanged(); } }

        private SerialIO _PrimarySerial = new SerialIO();
        public SerialIO PrimarySerial { get { return _PrimarySerial; } set { _PrimarySerial = value; OnPropertyChanged(); } }

        #region Debug Properties
        public bool IsDebugMode { get { return System.Diagnostics.Debugger.IsAttached; } }

        private bool _IsRandomizerEnabled;
        public bool IsRandomizerEnabled { get { return _IsRandomizerEnabled; } set { _IsRandomizerEnabled = value; OnPropertyChanged(); } }

        private static Random rnd = new Random();
			
        #endregion

        #region Sequential Offgas Analysis
        private ParticipantID _CurrentOffgasScope = ParticipantID.Reactor_1;
        public ParticipantID CurrentOffgasScope { get { return _CurrentOffgasScope; } set { _CurrentOffgasScope = value; OnPropertyChanged(); } }
        
			


        #endregion



        public ViewModel()
        {
            MCPSettings.HomeDirectoryChanged += MCPSettings_HomeDirectoryChanged;
            Inventory.Initialize(MCPSettings.PumpDirectoryPath, MCPSettings.ReactorDirectoryPath, MCPSettings.SensorDirectoryPath);
            ExperimentLibrary.Initialize(MCPSettings.ExperimentsDirectoryPath);
            PrimarySerial.NewMessageReceived += PrimarySerial_NewMessageReceived;
            if (IsDebugMode)
                StartRandomValuesGenerator();
            Cultivation.OffgasCollected += delegate(ParticipantID sender) { NextOffgasStream(); };
        }


        private void PrimarySerial_NewMessageReceived(object sender, Message message)
        {
            Experiment receivingExperiment;
            Cultivation receiver = ExperimentLibrary.FindRunningCultivation(message.Sender, out receivingExperiment);
            if (receiver == null)
                return;
            receiver.ReceiveMessage(message);
        }

        private void MCPSettings_HomeDirectoryChanged(object sender, EventArgs e)
        {
            Inventory.Initialize(MCPSettings.PumpDirectoryPath, MCPSettings.ReactorDirectoryPath, MCPSettings.SensorDirectoryPath);
            ExperimentLibrary.Initialize(MCPSettings.ExperimentsDirectoryPath);
            NextOffgasStream();
        }

        #region Offgas Switching
        private void NextOffgasStream()
        {
            //TODO: only consider Reactors with configured offgas sensors for offgas analysis!
            List<Cultivation> cultivations = ExperimentLibrary.RunningCultivations;//(from Cultivation c in ExperimentLibrary.RunningCultivations where c.g;
            Cultivation next = null;
            for (int i = 0; i < cultivations.Count; i++)
            {
                if (cultivations[i].Reactor.ParticipantID == CurrentOffgasScope)
                {
                    if (i+1 < cultivations.Count)
                        next = cultivations[i+1];
                    else
                        next = cultivations[0];
                    break;
                }
            }
            if (next != null)
            {
                CurrentOffgasScope = next.Reactor.ParticipantID;
                foreach (DataPostprocessingLog dpl in next.PostprocessingLogs.Values)
                {
                    dpl.ResetInterval();//start a new measurement interval from here
                }
                //switch the valves to the next reactor
                //TODO: the switching is broken... maybe avoid the timer and get the interval information from the first vs. current DataPoint?
                //TODO: who shall receive the signal for switching?
                PrimarySerial.SendMessage(new Message(ParticipantID.MCP, ParticipantID.Master, MessageType.Command, "offgas", ((int)CurrentOffgasScope).ToString()));
            }
        }

        #endregion




        private void StartRandomValuesGenerator()
        {
            DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            dt.Tick += delegate
            {
                if (IsRandomizerEnabled)
                {
                    Message temp = new Message(ParticipantID.Reactor_1, ParticipantID.MCP, MessageType.Data, string.Format("{0}\t{1}\t{2}", DimensionSymbol.Temperature, 36 + 2 * rnd.NextDouble(), Unit.Celsius));
                    PrimarySerial.InterpretMessage(temp.Raw);

                    Message biom = new Message(ParticipantID.Reactor_1, ParticipantID.MCP, MessageType.Data, string.Format("{0}\t{1}\t{2}", DimensionSymbol.Biomass, 345 + 13 * rnd.NextDouble(), Unit.Analog));
                    PrimarySerial.InterpretMessage(biom.Raw);


                    Message ox = new Message(CurrentOffgasScope, ParticipantID.MCP, MessageType.Data, string.Format("{0}\t{1}\t{2}", DimensionSymbol.O2_Saturation, 345 + 13 * rnd.NextDouble(), Unit.Analog));
                    PrimarySerial.InterpretMessage(ox.Raw);
                    Message cd = new Message(CurrentOffgasScope, ParticipantID.MCP, MessageType.Data, string.Format("{0}\t{1}\t{2}", DimensionSymbol.CO2_Saturation, 120 + 23 * rnd.NextDouble(), Unit.Analog));
                    PrimarySerial.InterpretMessage(cd.Raw);
                    Message ch = new Message(CurrentOffgasScope, ParticipantID.MCP, MessageType.Data, string.Format("{0}\t{1}\t{2}", DimensionSymbol.CHx_Saturation, 600 + 12 * rnd.NextDouble(), Unit.Analog));
                    PrimarySerial.InterpretMessage(ch.Raw);
                }                
            };
            dt.Start();
        }

    }
}

