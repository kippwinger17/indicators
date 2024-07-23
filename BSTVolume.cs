// 
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class BSTVolume : Indicator
	{
		private double	buys 		= 0;
		private double	sells 		= 0;
		private double	ratio		= 2.0;
		private bool 	showTotal 	= true;
		private bool 	showMarker	= true;
		private int		activeBar;				// added 6/30/16 for renko & linebreak bars that remove bars

		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= @"Plots a histogram splitting volume between trades at the ask or higher and trades at the bid and lower.  Only works on historical data if using Tick Replay";
				Name					= "BST Volume";
				BarsRequiredToPlot		= 0;
				Calculate				= Calculate.OnEachTick;
				IsOverlay				= false;
				DisplayInDataBox		= true;	
				DrawOnPricePanel 		= false;
				PaintPriceMarkers		= false;
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Bar, "Total");
				AddPlot(new Stroke(Brushes.Chartreuse, 2), PlotStyle.Bar, "Buys");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "Sells");	
			}
		}

		protected override void OnMarketData(MarketDataEventArgs e)
		{			
			if(e.MarketDataType == MarketDataType.Last)
			{				
				if(e.Price >= e.Ask)
				{
					buys += e.Volume;
				}
				else if (e.Price <= e.Bid)
				{
					sells += e.Volume;
				}
				
			}	
		}
		
		protected override void OnBarUpdate()
		{	
			
			if (CurrentBar < activeBar)  	//added 6/30/16, needed for renko and linebreak
				return;			
			
			// Update (plot) the bar
			Sells[0] 	= sells;
			Buys[0] 	= buys;
			if (showTotal)
			{
				Total[0] 	= buys + sells;
			}
			
			if (showMarker)
			{
			
				if ((buys / sells) >= ratio)
				{
					Draw.TriangleUp (this, "test1", true, 0, (buys+sells) * 1.1, Brushes.Lime);
				}
				else if ((sells / buys) >= ratio)
				{
					Draw.TriangleDown (this, "test2", true, 0, (buys+sells)* 1.1, Brushes.Red);
				}
				else 
				{
					RemoveDrawObject("test1");
					RemoveDrawObject("test2");
				}
					
			}
			
			
			// Reset accumulators/plots on new bar
			if (CurrentBar != activeBar)
			{
				RemoveDrawObject("test1");
				RemoveDrawObject("test2");
				buys 		= 0;
				sells		= 0;
				Sells[0]	= 0;
				Buys[0] 	= 0;
				activeBar 	= CurrentBar;		// added 6/30/16
				if (showTotal)
				{
					Total[0] = 0;
					
				}
				
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Show Total Volume", Order=1, GroupName="Parameters")]
		public bool ShowTotal
		{
			get { return showTotal; }
			set { showTotal = value; }
		}	
		[NinjaScriptProperty]
		[Display(Name="Show direction marker", Order=2, GroupName="Parameters")]
		public bool ShowMarker
		{
			get { return showMarker; }
			set { showMarker = value; }
		}	
		[NinjaScriptProperty]
		[Display(Name="Ratio for Marker", Description="Ratio of buy/sell or sell/buy to show dynamic marker", Order=3, GroupName="Parameters")]		
		public double Ratio
		{
			get { return ratio; }
			set { ratio =value; }
		}		
			
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Sells
		{
			get { return Values[2]; }
		}		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Buys
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Total
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BSTVolume[] cacheBSTVolume;
		public BSTVolume BSTVolume(bool showTotal, bool showMarker, double ratio)
		{
			return BSTVolume(Input, showTotal, showMarker, ratio);
		}

		public BSTVolume BSTVolume(ISeries<double> input, bool showTotal, bool showMarker, double ratio)
		{
			if (cacheBSTVolume != null)
				for (int idx = 0; idx < cacheBSTVolume.Length; idx++)
					if (cacheBSTVolume[idx] != null && cacheBSTVolume[idx].ShowTotal == showTotal && cacheBSTVolume[idx].ShowMarker == showMarker && cacheBSTVolume[idx].Ratio == ratio && cacheBSTVolume[idx].EqualsInput(input))
						return cacheBSTVolume[idx];
			return CacheIndicator<BSTVolume>(new BSTVolume(){ ShowTotal = showTotal, ShowMarker = showMarker, Ratio = ratio }, input, ref cacheBSTVolume);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BSTVolume BSTVolume(bool showTotal, bool showMarker, double ratio)
		{
			return indicator.BSTVolume(Input, showTotal, showMarker, ratio);
		}

		public Indicators.BSTVolume BSTVolume(ISeries<double> input , bool showTotal, bool showMarker, double ratio)
		{
			return indicator.BSTVolume(input, showTotal, showMarker, ratio);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BSTVolume BSTVolume(bool showTotal, bool showMarker, double ratio)
		{
			return indicator.BSTVolume(Input, showTotal, showMarker, ratio);
		}

		public Indicators.BSTVolume BSTVolume(ISeries<double> input , bool showTotal, bool showMarker, double ratio)
		{
			return indicator.BSTVolume(input, showTotal, showMarker, ratio);
		}
	}
}

#endregion
