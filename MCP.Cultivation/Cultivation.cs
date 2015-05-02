﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCP.Equipment;
using MCP.Protocol;
using TCD;

namespace MCP.Cultivation
{
    public class Cultivation : PropertyChangedBase
    {
        private ReactorInformation _Reactor;
        public ReactorInformation Reactor { get { return _Reactor; } set { _Reactor = value; OnPropertyChanged(); } }

        #region Commands
        private RelayCommand _ChangeParametersCommand;
        public RelayCommand ChangeParametersCommand { get { return _ChangeParametersCommand; } set { _ChangeParametersCommand = value; OnPropertyChanged(); } }
        #endregion

        #region Control Parameters
        private int _AgitationRateSetpoint = 500;
        /// <summary>
        /// desired agitation rate [rpm]
        /// </summary>
        public int AgitationRateSetpoint { get { return _AgitationRateSetpoint; } set { _AgitationRateSetpoint = value; OnPropertyChanged(); } }

        private double _AerationRateSetpoint = 1;
        /// <summary>
        /// desired aeration rate [vvm]
        /// </summary>
        public double AerationRateSetpoint { get { return _AerationRateSetpoint; } set { _AerationRateSetpoint = value; OnPropertyChanged(); } }

        private double _DilutionRateSetpoint = 0;
        /// <summary>
        /// desired dilution rate [culture volumes per hour]
        /// </summary>
        public double DilutionRateSetpoint { get { return _DilutionRateSetpoint; } set { _DilutionRateSetpoint = value; OnPropertyChanged(); } }
        #endregion

        #region Events
        //OnNewMessageToSend
        public delegate void AddOnNewMessageToSendDelegate(object sender, Message message);
        public event AddOnNewMessageToSendDelegate NewMessageToSend;
        private void OnNewMessageToSendEvent(object sender, Message message)
        {
            if (NewMessageToSend != null)
                NewMessageToSend(sender, message);
        }
        #endregion

        public Cultivation()
        {
            ChangeParametersCommand = new RelayCommand(async delegate
            {
                Cultivation newCultivation = new Cultivation()
                {
                    DilutionRateSetpoint = this.DilutionRateSetpoint,
                    AgitationRateSetpoint = this.AgitationRateSetpoint,
                    AerationRateSetpoint = this.AerationRateSetpoint
                };
                SetpointWindow sw = new SetpointWindow() { DataContext = newCultivation };
                sw.Show();
                await sw.WaitTask;
                if (sw.Confirmed)
                {
                    DilutionRateSetpoint = newCultivation.DilutionRateSetpoint;
                    AgitationRateSetpoint = newCultivation.AgitationRateSetpoint;
                    AerationRateSetpoint = newCultivation.AerationRateSetpoint;
                    //
                    SendSetpointUpdate();
                    //TODO: save changes to cultivation file within the experiment
                }
            });
        }

        private void SendSetpointUpdate()
        {
            if (Reactor.FeedPump != null)
                OnNewMessageToSendEvent(this, new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Feed_Rate, CalculateFeedPumpSPH().ToString(), Unit.SPH));
            if (Reactor.AerationPump != null)
                OnNewMessageToSendEvent(this, new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Aeration_Rate, CalculateAerationPumpSPH().ToString(), Unit.SPH));
            if (Reactor.HarvestPump != null)
                OnNewMessageToSendEvent(this, new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Harvest_Rate, CalculateHarvestPumpSPH().ToString(), Unit.SPH));
            OnNewMessageToSendEvent(this, new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Agitation_Rate, AgitationRateSetpoint.ToString(), Unit.RPM));
        }

        private double CalculateFeedPumpSPH()
        {
            return DilutionRateSetpoint * Reactor.CultureVolume * Reactor.FeedPump.SpecificPumpingRate;
        }
        private double CalculateAerationPumpSPH()
        {
            return AerationRateSetpoint * 60 * Reactor.CultureVolume * Reactor.AerationPump.SpecificPumpingRate;
        }
        private double CalculateHarvestPumpSPH()
        {
            return DilutionRateSetpoint * Reactor.CultureVolume * Reactor.HarvestPump.SpecificPumpingRate * 1.15;
        }
    }
}