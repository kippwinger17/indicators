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
	public class ZLSMA : Indicator
	{
        
        private double[] buffer;
        private double[] lagBuffer;
        private int lagPeriod;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ZLSMA";
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
                IsDataSeriesRequired = true;
                //DefaultWidth = 100;
                //DefaultForeground = Brushes.Crimson;
                Period = 14;

                AddPlot(Brushes.Orange, "ZLSMA");
            }
			else if (State == State.Configure)
			{

                //lagPeriod = 2; // Lag period, can be adjusted
                lagPeriod = Period;
                buffer = new double[lagPeriod + 1];
                lagBuffer = new double[lagPeriod + 1];
            }
		}


        protected override void OnBarUpdate()
        {
            if (CurrentBar <= lagPeriod)
                return;

            double sum = 0;
            double lagSum = 0;

            // Calculate SMA for current bar
            for (int i = 0; i <= lagPeriod; i++)
            {
                buffer[i] = (Inputs[0][0] + Inputs[0][i]) / 2;
                sum += buffer[i];
            }

            double sma = sum / (lagPeriod + 1);

            // Calculate SMA for lagged values
            for (int i = 0; i <= lagPeriod; i++)
            {
                lagBuffer[i] = (Inputs[0][CurrentBar - lagPeriod] + Inputs[0][CurrentBar - lagPeriod + i]) / 2;
                lagSum += lagBuffer[i];
            }

            double lagSma = lagSum / (lagPeriod + 1);

            Values[0][0] = sma;
        }

        #region Properties
        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
        public int Period
        { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZLSMA[] cacheZLSMA;
		public ZLSMA ZLSMA(int period)
		{
			return ZLSMA(Input, period);
		}

		public ZLSMA ZLSMA(ISeries<double> input, int period)
		{
			if (cacheZLSMA != null)
				for (int idx = 0; idx < cacheZLSMA.Length; idx++)
					if (cacheZLSMA[idx] != null && cacheZLSMA[idx].Period == period && cacheZLSMA[idx].EqualsInput(input))
						return cacheZLSMA[idx];
			return CacheIndicator<ZLSMA>(new ZLSMA(){ Period = period }, input, ref cacheZLSMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZLSMA ZLSMA(int period)
		{
			return indicator.ZLSMA(Input, period);
		}

		public Indicators.ZLSMA ZLSMA(ISeries<double> input , int period)
		{
			return indicator.ZLSMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZLSMA ZLSMA(int period)
		{
			return indicator.ZLSMA(Input, period);
		}

		public Indicators.ZLSMA ZLSMA(ISeries<double> input , int period)
		{
			return indicator.ZLSMA(input, period);
		}
	}
}

#endregion
