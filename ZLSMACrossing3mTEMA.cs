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
	public class ZLSMACrossing3mTEMA : Indicator
	{
        private TEMA Tema3m;
        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ZLSMACrossing3mTEMA";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
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
            else if (State == State.DataLoaded)
            {
                Tema3m = TEMA(BarsArray[1], 21);
            }
        }

		protected override void OnBarUpdate()
		{
            if (CurrentBars[0] < 1 || CurrentBars[1] < 100)
                return;

            // ZLSMA Crossing TEMA
            if (CrossAbove(ZLSMA(13), Tema3m, 1))
            {
                
                //NinjaTrader.Code.Output.Process("ZLSMA/3mTEMA UP", PrintTo.OutputTab2);
                //Draw.Text(this, "zlsmaUp2Text" + Convert.ToString(CurrentBars[0]), "ZLSMA/3mTEMA", 0, Low[0] - 11 * TickSize, Brushes.White);
                Draw.ArrowUp(this, "zlsmaUp2" + Convert.ToString(CurrentBars[0]), true, 0, Low[0] - 8 * TickSize, Brushes.IndianRed);
                
            }

            // ZLSMA Crossing Below SMA
            if (CrossBelow(ZLSMA(13), Tema3m, 1))
            {
                //NinjaTrader.Code.Output.Process("ZLSMA/3mTEMA DOWN", PrintTo.OutputTab2);
                //Draw.Text(this, "zlsmaDown2Text" + Convert.ToString(CurrentBars[0]), "ZLSMA/3mTEMA", 0, Low[0] + 11 * TickSize, Brushes.White);
                Draw.ArrowDown(this, "zlsmaDown2" + Convert.ToString(CurrentBars[0]), true, 0, High[0] + 8 * TickSize, Brushes.IndianRed);
                
            }
        }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZLSMACrossing3mTEMA[] cacheZLSMACrossing3mTEMA;
		public ZLSMACrossing3mTEMA ZLSMACrossing3mTEMA()
		{
			return ZLSMACrossing3mTEMA(Input);
		}

		public ZLSMACrossing3mTEMA ZLSMACrossing3mTEMA(ISeries<double> input)
		{
			if (cacheZLSMACrossing3mTEMA != null)
				for (int idx = 0; idx < cacheZLSMACrossing3mTEMA.Length; idx++)
					if (cacheZLSMACrossing3mTEMA[idx] != null &&  cacheZLSMACrossing3mTEMA[idx].EqualsInput(input))
						return cacheZLSMACrossing3mTEMA[idx];
			return CacheIndicator<ZLSMACrossing3mTEMA>(new ZLSMACrossing3mTEMA(), input, ref cacheZLSMACrossing3mTEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZLSMACrossing3mTEMA ZLSMACrossing3mTEMA()
		{
			return indicator.ZLSMACrossing3mTEMA(Input);
		}

		public Indicators.ZLSMACrossing3mTEMA ZLSMACrossing3mTEMA(ISeries<double> input )
		{
			return indicator.ZLSMACrossing3mTEMA(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZLSMACrossing3mTEMA ZLSMACrossing3mTEMA()
		{
			return indicator.ZLSMACrossing3mTEMA(Input);
		}

		public Indicators.ZLSMACrossing3mTEMA ZLSMACrossing3mTEMA(ISeries<double> input )
		{
			return indicator.ZLSMACrossing3mTEMA(input);
		}
	}
}

#endregion
