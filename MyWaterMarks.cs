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
	public class MyWaterMarks : Indicator
	{
		
		private string myWaterMarkText;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Add Watermarks to a chart";
				Name										= "MyWaterMarks";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				WaterMarkSize					= 50;
				WaterMarkOpacity				= 0.1;
				WaterMarkColor					= Brushes.Black;
				UseCustomText					= false;
				CustomText						= "Sample Text";
			}
			else if (State == State.DataLoaded)
			{			
				if (UseCustomText)
				{
					myWaterMarkText = CustomText;
				}
				else
				{
					myWaterMarkText = Time[0].ToString("MM/dd"); //ChartControl.Instrument.MasterInstrument.Description;
				}
						
				Brush myWaterMarkBrush = new SolidColorBrush();
				myWaterMarkBrush = WaterMarkColor.Clone();
				myWaterMarkBrush.Opacity = WaterMarkOpacity;
				myWaterMarkBrush.Freeze();
				
				NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont(ChartControl.Properties.LabelFont.FamilySerialize, 20) {Size = WaterMarkSize, Bold = false};
				Draw.TextFixed(this, "myWaterMark", myWaterMarkText, TextPosition.BottomLeft, myWaterMarkBrush, myFont, Brushes.Transparent, Brushes.Transparent, 1);		
			}
		}

		protected override void OnBarUpdate()
		{
            NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont(ChartControl.Properties.LabelFont.FamilySerialize, 20) { Size = WaterMarkSize, Bold = false };
            string myWaterMarkText;
            Brush myWaterMarkBrush = new SolidColorBrush();
            myWaterMarkBrush = WaterMarkColor.Clone();
            myWaterMarkBrush.Opacity = WaterMarkOpacity;
            myWaterMarkText = Time[0].ToString("MM/dd"); //ChartControl.Instrument.MasterInstrument.Description;
            Draw.TextFixed(this, "myWaterMark", myWaterMarkText, TextPosition.BottomLeft, myWaterMarkBrush, myFont, Brushes.Transparent, Brushes.Transparent, 1);
        }


        #region Properties
        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="WaterMarkSize", Order=1, GroupName="Parameters")]
		public int WaterMarkSize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 1)]
		[Display(Name="WaterMarkOpacity", Description="Enter Opacity from 0 to 1", Order=2, GroupName="Parameters")]
		public double WaterMarkOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="BandColor", Order=3, GroupName="Parameters")]
		public Brush WaterMarkColor
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="UseCustomText", Description="Uncheck to use instrument symbol", Order=4, GroupName="Parameters")]
		public bool UseCustomText
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="CustomText", Description="Leave this blank if you are using stock symbol", Order=5, GroupName="Parameters")]
		public string CustomText
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MyWaterMarks[] cacheMyWaterMarks;
		public MyWaterMarks MyWaterMarks(int waterMarkSize, double waterMarkOpacity, bool useCustomText, string customText)
		{
			return MyWaterMarks(Input, waterMarkSize, waterMarkOpacity, useCustomText, customText);
		}

		public MyWaterMarks MyWaterMarks(ISeries<double> input, int waterMarkSize, double waterMarkOpacity, bool useCustomText, string customText)
		{
			if (cacheMyWaterMarks != null)
				for (int idx = 0; idx < cacheMyWaterMarks.Length; idx++)
					if (cacheMyWaterMarks[idx] != null && cacheMyWaterMarks[idx].WaterMarkSize == waterMarkSize && cacheMyWaterMarks[idx].WaterMarkOpacity == waterMarkOpacity && cacheMyWaterMarks[idx].UseCustomText == useCustomText && cacheMyWaterMarks[idx].CustomText == customText && cacheMyWaterMarks[idx].EqualsInput(input))
						return cacheMyWaterMarks[idx];
			return CacheIndicator<MyWaterMarks>(new MyWaterMarks(){ WaterMarkSize = waterMarkSize, WaterMarkOpacity = waterMarkOpacity, UseCustomText = useCustomText, CustomText = customText }, input, ref cacheMyWaterMarks);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MyWaterMarks MyWaterMarks(int waterMarkSize, double waterMarkOpacity, bool useCustomText, string customText)
		{
			return indicator.MyWaterMarks(Input, waterMarkSize, waterMarkOpacity, useCustomText, customText);
		}

		public Indicators.MyWaterMarks MyWaterMarks(ISeries<double> input , int waterMarkSize, double waterMarkOpacity, bool useCustomText, string customText)
		{
			return indicator.MyWaterMarks(input, waterMarkSize, waterMarkOpacity, useCustomText, customText);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MyWaterMarks MyWaterMarks(int waterMarkSize, double waterMarkOpacity, bool useCustomText, string customText)
		{
			return indicator.MyWaterMarks(Input, waterMarkSize, waterMarkOpacity, useCustomText, customText);
		}

		public Indicators.MyWaterMarks MyWaterMarks(ISeries<double> input , int waterMarkSize, double waterMarkOpacity, bool useCustomText, string customText)
		{
			return indicator.MyWaterMarks(input, waterMarkSize, waterMarkOpacity, useCustomText, customText);
		}
	}
}

#endregion
