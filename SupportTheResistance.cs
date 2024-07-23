#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SupportTheResistance : Indicator
	{
        //Output
        private Series<double> supportVal;
        private Series<double> resistanceVal;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Show Support/Resistance using a timeframe";
				Name										= "SupportTheResistance";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
                AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Dash, 2), PlotStyle.Square, "HighestHigh");
                AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Dash, 2), PlotStyle.Square, "LowestLow");
                
            }
			else if (State == State.Configure)
			{
                supportVal = new Series<double>(this);
                resistanceVal = new Series<double>(this);
            }
		}
        private DateTime startDateTime;
        private DateTime endDateTime;
        protected override void OnBarUpdate()
		{
            if (BarsInProgress != 0)
                return;

            // Set the start / end datetime 
            startDateTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, StartHour, StartMinute, 0);
            endDateTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, EndHour, EndMinute, 0);
            //NinjaTrader.Code.Output.Process("startDateTime: " + startDateTime.ToString(), PrintTo.OutputTab1);
            //NinjaTrader.Code.Output.Process("endDateTime: " + endDateTime.ToString(), PrintTo.OutputTab1);

            // Get the bars from the start/end datetime
            int startBarsAgo = Bars.GetBar(startDateTime);
            int endBarsAgo = Bars.GetBar(endDateTime);
            //NinjaTrader.Code.Output.Process("startBarsAgo: " + startBarsAgo.ToString(), PrintTo.OutputTab1);
            //NinjaTrader.Code.Output.Process("endBarsAgo: " + endBarsAgo.ToString(), PrintTo.OutputTab1);

            if (CurrentBar > endBarsAgo)
            {
                //Get the MAX/MIN using the barsago
                double highestHigh = MAX(High, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];
                double lowestLow = MIN(Low, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];
               // NinjaTrader.Code.Output.Process("highestHigh: " + highestHigh.ToString(), PrintTo.OutputTab1);
                //NinjaTrader.Code.Output.Process("lowestLow: " + lowestLow.ToString(), PrintTo.OutputTab1);

                resistanceVal[0] = highestHigh;
                supportVal[0] = lowestLow;
            }
        }

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ResistancePlot
        {
            get { return resistanceVal; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SupportPlot
        {
            get { return supportVal; }
        }

        [Range(0, 23)]
        [NinjaScriptProperty]
        [Display(Name = "Start hour", Description = "Enter start hour, Military time format 0 - 23", Order = 1, GroupName = "Parameters")]
        public int StartHour
        { get; set; }

        [Range(0, 59)]
        [NinjaScriptProperty]
        [Display(Name = "Start minute", Description = "Enter start minute(s) 0 - 59", Order = 2, GroupName = "Parameters")]
        public int StartMinute
        { get; set; }

        [Range(0, 23)]
        [NinjaScriptProperty]
        [Display(Name = "End hour", Description = "Enter end hour, Military time format 0 - 23", Order = 3, GroupName = "Parameters")]
        public int EndHour
        { get; set; }

        [Range(0, 59)]
        [NinjaScriptProperty]
        [Display(Name = "End minute", Description = " Enter end minute(s) 0 - 59", Order = 4, GroupName = "Parameters")]
        public int EndMinute
        { get; set; }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SupportTheResistance[] cacheSupportTheResistance;
		public SupportTheResistance SupportTheResistance(int startHour, int startMinute, int endHour, int endMinute)
		{
			return SupportTheResistance(Input, startHour, startMinute, endHour, endMinute);
		}

		public SupportTheResistance SupportTheResistance(ISeries<double> input, int startHour, int startMinute, int endHour, int endMinute)
		{
			if (cacheSupportTheResistance != null)
				for (int idx = 0; idx < cacheSupportTheResistance.Length; idx++)
					if (cacheSupportTheResistance[idx] != null && cacheSupportTheResistance[idx].StartHour == startHour && cacheSupportTheResistance[idx].StartMinute == startMinute && cacheSupportTheResistance[idx].EndHour == endHour && cacheSupportTheResistance[idx].EndMinute == endMinute && cacheSupportTheResistance[idx].EqualsInput(input))
						return cacheSupportTheResistance[idx];
			return CacheIndicator<SupportTheResistance>(new SupportTheResistance(){ StartHour = startHour, StartMinute = startMinute, EndHour = endHour, EndMinute = endMinute }, input, ref cacheSupportTheResistance);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SupportTheResistance SupportTheResistance(int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.SupportTheResistance(Input, startHour, startMinute, endHour, endMinute);
		}

		public Indicators.SupportTheResistance SupportTheResistance(ISeries<double> input , int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.SupportTheResistance(input, startHour, startMinute, endHour, endMinute);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SupportTheResistance SupportTheResistance(int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.SupportTheResistance(Input, startHour, startMinute, endHour, endMinute);
		}

		public Indicators.SupportTheResistance SupportTheResistance(ISeries<double> input , int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.SupportTheResistance(input, startHour, startMinute, endHour, endMinute);
		}
	}
}

#endregion
