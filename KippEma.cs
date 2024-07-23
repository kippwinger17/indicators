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
	public class KippEma : Indicator
	{
        private double constant1;
        private double constant2;
        private Series<double> MySeries;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "KippEma";
				Calculate									= Calculate.OnPriceChange;
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
                Period = 14;

                AddPlot(Brushes.AliceBlue, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameEMA);
            }
			else if (State == State.Configure)
			{
				AddDataSeries("ES 09-23", Data.BarsPeriodType.Minute, 5, Data.MarketDataType.Last);
                constant1 = 2.0 / (1 + Period);
                constant2 = 1 - (2.0 / (1 + Period));
            }
		}

		protected override void OnBarUpdate()
		{
            var open = Open[0].ToString();
            var high = High[0].ToString();
            var low = Low[0].ToString();
            var close = Close[0].ToString();
            MySeries[0] = (Open[0] + High[0] + Low[0] + Close[0]) / 4;
            var val = MySeries[0].ToString();
            Value[0] = (CurrentBar == 0 ? Input[0] : Input[0] * constant1 + constant2 * Value[1]);
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
		private KippEma[] cacheKippEma;
		public KippEma KippEma(int period)
		{
			return KippEma(Input, period);
		}

		public KippEma KippEma(ISeries<double> input, int period)
		{
			if (cacheKippEma != null)
				for (int idx = 0; idx < cacheKippEma.Length; idx++)
					if (cacheKippEma[idx] != null && cacheKippEma[idx].Period == period && cacheKippEma[idx].EqualsInput(input))
						return cacheKippEma[idx];
			return CacheIndicator<KippEma>(new KippEma(){ Period = period }, input, ref cacheKippEma);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.KippEma KippEma(int period)
		{
			return indicator.KippEma(Input, period);
		}

		public Indicators.KippEma KippEma(ISeries<double> input , int period)
		{
			return indicator.KippEma(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.KippEma KippEma(int period)
		{
			return indicator.KippEma(Input, period);
		}

		public Indicators.KippEma KippEma(ISeries<double> input , int period)
		{
			return indicator.KippEma(input, period);
		}
	}
}

#endregion
