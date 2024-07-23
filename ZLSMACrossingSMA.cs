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
	public class ZLSMACrossingSMA : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ZLSMACrossingSMA";
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
            // ZLSMA Crossing Above SMA
            if (CrossAbove(ZLSMA(13), SMA(21), 1))
            {
                //NinjaTrader.Code.Output.Process("ZLSMA/SMA UP", PrintTo.OutputTab2);
                //Draw.Text(this, "zlsmaUpText" + Convert.ToString(CurrentBars[0]), "ZLSMA/SMA", 0, Low[0] - 7 * TickSize, Brushes.White);
                Draw.ArrowUp(this, "zlsmaUp" + Convert.ToString(CurrentBars[0]), true, 0, Low[0] - 3 * TickSize, Brushes.SteelBlue);
                
            }

            // ZLSMA Crossing Below SMA
            if (CrossBelow(ZLSMA(13), SMA(21), 1))
            {
                //NinjaTrader.Code.Output.Process("ZLSMA/SMA DOWN", PrintTo.OutputTab2);
                //Draw.Text(this, "zlsmaDownText" + Convert.ToString(CurrentBars[0]), "ZLSMA/SMA", 0, Low[0] + 7 * TickSize, Brushes.White);
                Draw.ArrowDown(this, "zlsmaDown" + Convert.ToString(CurrentBars[0]), true, 0, High[0] + 3 * TickSize, Brushes.SteelBlue);
                
            }
        }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZLSMACrossingSMA[] cacheZLSMACrossingSMA;
		public ZLSMACrossingSMA ZLSMACrossingSMA()
		{
			return ZLSMACrossingSMA(Input);
		}

		public ZLSMACrossingSMA ZLSMACrossingSMA(ISeries<double> input)
		{
			if (cacheZLSMACrossingSMA != null)
				for (int idx = 0; idx < cacheZLSMACrossingSMA.Length; idx++)
					if (cacheZLSMACrossingSMA[idx] != null &&  cacheZLSMACrossingSMA[idx].EqualsInput(input))
						return cacheZLSMACrossingSMA[idx];
			return CacheIndicator<ZLSMACrossingSMA>(new ZLSMACrossingSMA(), input, ref cacheZLSMACrossingSMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZLSMACrossingSMA ZLSMACrossingSMA()
		{
			return indicator.ZLSMACrossingSMA(Input);
		}

		public Indicators.ZLSMACrossingSMA ZLSMACrossingSMA(ISeries<double> input )
		{
			return indicator.ZLSMACrossingSMA(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZLSMACrossingSMA ZLSMACrossingSMA()
		{
			return indicator.ZLSMACrossingSMA(Input);
		}

		public Indicators.ZLSMACrossingSMA ZLSMACrossingSMA(ISeries<double> input )
		{
			return indicator.ZLSMACrossingSMA(input);
		}
	}
}

#endregion
