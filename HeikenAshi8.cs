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
using SharpDX;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	// 04-27-2018 Changed line 220 from: chartControl.GetXByBarIndex(chartControl.BarsArray[0], idx); to: chartControl.GetXByBarIndex(ChartBars, idx); to
	// ensure compatibility when used in any panel on the chart.
	// Changed default shadow from black to dimgray.
	public class HeikenAshi8 : Indicator
	{
        private Brush	barColorDown	= Brushes.Red;
        private Brush	barColorUp      = Brushes.Lime;
        private Brush	shadowColor     = Brushes.DimGray;  // changed 4-27-2018 (was black which did not work on black background)
        private Pen     shadowPen       = null;
        private int     shadowWidth     = 1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"HeikenAshi technique discussed in the article 'Using Heiken-Ashi Technique' in February 2004 issue of TASC magazine.";
				Name								= "HeikenAshi8";
				Calculate							= Calculate.OnEachTick;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= false;
				
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive			= false;
				BarsRequiredToPlot					= 1;
				AddPlot(Brushes.Gray, "HAOpen");
				AddPlot(Brushes.Gray, "HAHigh");
				AddPlot(Brushes.Gray, "HALow");
				AddPlot(Brushes.Gray, "HAClose");
			}
		}

		protected override void OnBarUpdate()
		{

			//Clear out regular candles
			BarBrushes[0] = Brushes.Transparent;
			//CandleOutlineBrushes[0] = Brushes.Transparent;

			if (CurrentBar == 0)
            {				
                HAOpen[0] 	=	Open[0];
                HAHigh[0] 	=	High[0];
                HALow[0]	=	Low[0];
                HAClose[0]	=	Close[0];
                return;
            }

            HAClose[0]	=	((Open[0] + High[0] + Low[0] + Close[0]) * 0.25); // Calculate the close
            HAOpen[0]	=	((HAOpen[1] + HAClose[1]) * 0.5); // Calculate the open
            HAHigh[0]	=	(Math.Max(High[0], HAOpen[0])); // Calculate the high
            HALow[0]	=	(Math.Min(Low[0], HAOpen[0])); // Calculate the low	
		}

		#region Properties
		
		[XmlIgnore]
		[Display(Name="BarColorDown", Description="Color of Down bars", Order=2, GroupName="Visual")]
		public Brush BarColorDown
		{ 
			get { return barColorDown;}
			set { barColorDown = value;}
		}

		[Browsable(false)]
		public string BarColorDownSerializable
		{
			get { return Serialize.BrushToString(barColorDown); }
			set { barColorDown = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="BarColorUp", Description="Color of Up bars", Order=1, GroupName="Visual")]
		public Brush BarColorUp
		{ 
			get { return barColorUp;}
			set { barColorUp = value;}
		}

		[Browsable(false)]
		public string BarColorUpSerializable
		{
			get { return Serialize.BrushToString(barColorUp); }
			set { barColorUp = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="ShadowColor", Description="Wick/tail color", Order=3, GroupName="Visual")]
		public Brush ShadowColor
		{ 
			get { return shadowColor;}
			set { shadowColor = value;}
		}

		[Browsable(false)]
		public string ShadowColorSerializable
		{
			get { return Serialize.BrushToString(shadowColor); }
			set { shadowColor = Serialize.StringToBrush(value); }
		}			

		[Range(1, int.MaxValue)]
		[Display(Name="ShadowWidth", Description="Shadow (tail/wick) width", Order=4, GroupName="Visual")]
		public int ShadowWidth
		{ 
			get { return shadowWidth;}
			set { shadowWidth = value;}
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HAOpen
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HAHigh
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HALow
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HAClose
		{
			get { return Values[3]; }
		}
		
		#endregion
	
		#region Miscellaneous

       	public override void OnCalculateMinMax()
        {
            base.OnCalculateMinMax();
			
            if (Bars == null || ChartControl == null)
                return;

            for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
            {
                double tmpHigh 	= 	HAHigh.GetValueAt(idx);
                double tmpLow 	= 	HALow.GetValueAt(idx);
				
                if (tmpHigh != 0 && tmpHigh > MaxValue)
                    MaxValue = tmpHigh;
                if (tmpLow != 0 && tmpLow < MinValue)
                    MinValue = tmpLow;										
            }
        }		
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
			
            if (Bars == null || ChartControl == null)
                return;			

            int barPaintWidth = Math.Max(3, 1 + 2 * ((int)ChartControl.BarWidth - 1) + 2 * shadowWidth);

            for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
            {
                if (idx - Displacement < 0 || idx - Displacement >= BarsArray[0].Count || ( idx - Displacement < BarsRequiredToPlot)) 
                    continue;
		
                double valH = HAHigh.GetValueAt(idx);
                double valL = HALow.GetValueAt(idx);
                double valC = HAClose.GetValueAt(idx);
                double valO = HAOpen.GetValueAt(idx);
                int x  = chartControl.GetXByBarIndex(ChartBars, idx);  //was chartControl.BarsArray[0]
                int y1 = chartScale.GetYByValue(valO);
                int y2 = chartScale.GetYByValue(valH);
                int y3 = chartScale.GetYByValue(valL);
                int y4 = chartScale.GetYByValue(valC);

				SharpDX.Direct2D1.Brush	shadowColordx 	= shadowColor.ToDxBrush(RenderTarget);  // prepare for the color to use
                var xy2 = new Vector2(x, y2);
                var xy3 = new Vector2(x, y3);
                RenderTarget.DrawLine(xy2, xy3, shadowColordx, shadowWidth);	

                if (y4 == y1)
				    RenderTarget.DrawLine( new Vector2( x - barPaintWidth / 2, y1),  new Vector2( x + barPaintWidth / 2, y1), shadowColordx, shadowWidth);
                else
                {
                    if (y4 > y1)
					{
						SharpDX.Direct2D1.Brush	barColorDowndx 	= barColorDown.ToDxBrush(RenderTarget);  // prepare for the color to use						
                        RenderTarget.FillRectangle( new RectangleF(x - barPaintWidth / 2, y1, barPaintWidth, y4 - y1), barColorDowndx);
						barColorDowndx.Dispose();
					}
                    else
					{
						SharpDX.Direct2D1.Brush	barColorUpdx 	= barColorUp.ToDxBrush(RenderTarget);  // prepare for the color to use
                        RenderTarget.FillRectangle( new RectangleF(x - barPaintWidth / 2, y4, barPaintWidth, y1 - y4),barColorUpdx);
						barColorUpdx.Dispose();
					}
                     RenderTarget.DrawRectangle( new RectangleF( x - barPaintWidth / 2 + (float)shadowWidth / 2,
                       Math.Min(y4, y1), barPaintWidth - (float)shadowWidth, Math.Abs(y4 - y1)), shadowColordx, shadowWidth);
				}	
				shadowColordx.Dispose();	
            }
        }		
			
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HeikenAshi8[] cacheHeikenAshi8;
		public HeikenAshi8 HeikenAshi8()
		{
			return HeikenAshi8(Input);
		}

		public HeikenAshi8 HeikenAshi8(ISeries<double> input)
		{
			if (cacheHeikenAshi8 != null)
				for (int idx = 0; idx < cacheHeikenAshi8.Length; idx++)
					if (cacheHeikenAshi8[idx] != null &&  cacheHeikenAshi8[idx].EqualsInput(input))
						return cacheHeikenAshi8[idx];
			return CacheIndicator<HeikenAshi8>(new HeikenAshi8(), input, ref cacheHeikenAshi8);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HeikenAshi8 HeikenAshi8()
		{
			return indicator.HeikenAshi8(Input);
		}

		public Indicators.HeikenAshi8 HeikenAshi8(ISeries<double> input )
		{
			return indicator.HeikenAshi8(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HeikenAshi8 HeikenAshi8()
		{
			return indicator.HeikenAshi8(Input);
		}

		public Indicators.HeikenAshi8 HeikenAshi8(ISeries<double> input )
		{
			return indicator.HeikenAshi8(input);
		}
	}
}

#endregion
