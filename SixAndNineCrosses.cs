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
	public class SixAndNineCrosses : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SixAndNineCrosses";
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
			}
		}

		protected override void OnBarUpdate()
		{
            // 6 EMA Crossing Above 9 EMA
            if (CrossAbove(EMA(6), EMA(9), 1))
            {
                //NinjaTrader.Code.Output.Process("6 Cross UP", PrintTo.OutputTab2);
                //Draw.Text(this, "emaUpText" + Convert.ToString(CurrentBars[0]), "6EMA", 0, Low[0] - 7 * TickSize, Brushes.White);
                Draw.ArrowUp(this, "emaUp" + Convert.ToString(CurrentBars[0]), true, 0, Low[0] - 3 * TickSize, Brushes.Gold);

            }

            // 6 EMA Crossing Below 9 EMA
            if (CrossBelow(EMA(6), EMA(9), 1))
            {
                //NinjaTrader.Code.Output.Process("6 Cross DOWN", PrintTo.OutputTab2);
                //Draw.Text(this, "emaDownText" + Convert.ToString(CurrentBars[0]), "EMA", 0, Low[0] + 7 * TickSize, Brushes.White);
                Draw.ArrowDown(this, "emaDown" + Convert.ToString(CurrentBars[0]), true, 0, High[0] + 3 * TickSize, Brushes.Gold);

            }
        }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SixAndNineCrosses[] cacheSixAndNineCrosses;
		public SixAndNineCrosses SixAndNineCrosses()
		{
			return SixAndNineCrosses(Input);
		}

		public SixAndNineCrosses SixAndNineCrosses(ISeries<double> input)
		{
			if (cacheSixAndNineCrosses != null)
				for (int idx = 0; idx < cacheSixAndNineCrosses.Length; idx++)
					if (cacheSixAndNineCrosses[idx] != null &&  cacheSixAndNineCrosses[idx].EqualsInput(input))
						return cacheSixAndNineCrosses[idx];
			return CacheIndicator<SixAndNineCrosses>(new SixAndNineCrosses(), input, ref cacheSixAndNineCrosses);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SixAndNineCrosses SixAndNineCrosses()
		{
			return indicator.SixAndNineCrosses(Input);
		}

		public Indicators.SixAndNineCrosses SixAndNineCrosses(ISeries<double> input )
		{
			return indicator.SixAndNineCrosses(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SixAndNineCrosses SixAndNineCrosses()
		{
			return indicator.SixAndNineCrosses(Input);
		}

		public Indicators.SixAndNineCrosses SixAndNineCrosses(ISeries<double> input )
		{
			return indicator.SixAndNineCrosses(input);
		}
	}
}

#endregion
