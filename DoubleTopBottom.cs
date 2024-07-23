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
	public class DoubleTopBottom : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "DoubleTopBottom";
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
			}
            else if (State == State.Configure)
            {
                // Add 3 minute series for the cross
                AddDataSeries(BarsPeriodType.Minute, 3);
            }
        }

		protected override void OnBarUpdate()
		{
            // Check if there are at least 3 bars in the chart
            if (CurrentBar <= 2)
                return;
            if (ToTime(Time[0]) > 110000 && ToTime(Time[0]) < 120000)
            {
                NinjaTrader.Code.Output.Process("Time: " + ToTime(Time[0]).ToString(), PrintTo.OutputTab1);
                NinjaTrader.Code.Output.Process("Close: " + Close[1].ToString(), PrintTo.OutputTab1);
            }

            bool isDoubleBottom = (((Math.Min(Open[3], Close[3])) == (Math.Min(Open[2], Close[2]))) && ((Math.Min(Open[2], Close[2])) < (Math.Min(Open[1], Close[1])))  );
            bool isDoubleTop = (((Math.Max(Open[3], Close[3])) == (Math.Max(Open[2], Close[2]))) && ((Math.Max(Open[2], Close[2])) > (Math.Max(Open[1], Close[1]))));
            
            if (isDoubleBottom)
                Draw.ArrowUp(this, "dBottom" + Convert.ToString(CurrentBars[1]), true, 0, Low[1] - 5 * TickSize, Brushes.Lime);
            if (isDoubleTop)
                Draw.ArrowDown(this, "dTop" + Convert.ToString(CurrentBars[1]), true, 0, High[1] + 5 * TickSize, Brushes.Lime);
        }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DoubleTopBottom[] cacheDoubleTopBottom;
		public DoubleTopBottom DoubleTopBottom()
		{
			return DoubleTopBottom(Input);
		}

		public DoubleTopBottom DoubleTopBottom(ISeries<double> input)
		{
			if (cacheDoubleTopBottom != null)
				for (int idx = 0; idx < cacheDoubleTopBottom.Length; idx++)
					if (cacheDoubleTopBottom[idx] != null &&  cacheDoubleTopBottom[idx].EqualsInput(input))
						return cacheDoubleTopBottom[idx];
			return CacheIndicator<DoubleTopBottom>(new DoubleTopBottom(), input, ref cacheDoubleTopBottom);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DoubleTopBottom DoubleTopBottom()
		{
			return indicator.DoubleTopBottom(Input);
		}

		public Indicators.DoubleTopBottom DoubleTopBottom(ISeries<double> input )
		{
			return indicator.DoubleTopBottom(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DoubleTopBottom DoubleTopBottom()
		{
			return indicator.DoubleTopBottom(Input);
		}

		public Indicators.DoubleTopBottom DoubleTopBottom(ISeries<double> input )
		{
			return indicator.DoubleTopBottom(input);
		}
	}
}

#endregion
