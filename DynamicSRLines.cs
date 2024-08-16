//### 
//### Dynamic SR Lines
//###
//### User		Date 		
//### ------	-------- 	
//### NT_JoshG	Jan 2018 	
//###
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
		
	public class DynamicSRLines : Indicator
	{
		public int i=0, j=0, x=0;
		int LastBar = 0;
		int pxAbove=0, countAbove=0, maxCountAbove=0;
		int pxBelow=0, countBelow=0, maxCountBelow=0;
		double p=0, p1=0, level=0;
		string str="";

        // Resistance
        double resTop1 = 0;
        double resBottom1 = 0;

        double resTop2 = 0;
        double resBottom2 = 0;

        double resTop3 = 0;
        double resBottom3 = 0;

        // Support
        double supTop1 = 0;
        double supBottom1 = 0;

        double supTop2 = 0;
        double supBottom2 = 0;

        double supTop3 = 0;
        double supBottom3 = 0;

        //Output
        private Series<double> supportVal;
        private Series<double> resistanceVal;

        private Series<double> resistanceTop1;
        private Series<double> resistanceBottom1;

        private Series<double> resistanceTop2;
        private Series<double> resistanceBottom2;

        private Series<double> resistanceTop3;
        private Series<double> resistanceBottom3;

        private Series<double> supportTop1;
        private Series<double> supportBottom1;

        private Series<double> supportTop2;
        private Series<double> supportBottom2;

        private Series<double> supportTop3;
        private Series<double> supportBottom3;

        public struct PRICE_SWING 
		{
			public int    Type;			
			public int    Bar;			
			public double Price;
		} 		
		PRICE_SWING listEntry;
		List<PRICE_SWING>pivot = new List<PRICE_SWING>();	
		NinjaTrader.Gui.Tools.SimpleFont boldFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 10) { Size = 25, Bold = true };
		NinjaTrader.Gui.Tools.SimpleFont textFont = new NinjaTrader.Gui.Tools.SimpleFont("Times", 8) { Size = 25, Bold = false };
		protected override void OnStateChange()
		{
			
			if (State == State.SetDefaults)
			{
				Description									= @"Draws nearest level of S/R lines above and below current market based on historical price swing High/Low pivots";
				Name										= "DynamicSRLines";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				PivotStrength								= 5;
				MaxLookBackBars								= 200;
				PivotTickDiff								= 10;
				ZoneTickSize								= 2;
				MaxLevels									= 3;
				ShowPivots									= false;
				ColorBelow									= Brushes.Blue;
				ColorAbove									= Brushes.Red;

			}
			else if (State == State.Configure)
			{
                supportVal = new Series<double>(this);
                resistanceVal = new Series<double>(this);
                resistanceBottom1 = new Series<double>(this);
                resistanceTop1 = new Series<double>(this);
                resistanceTop2 = new Series<double>(this);
                resistanceBottom2 = new Series<double>(this);
                resistanceTop3 = new Series<double>(this);
                resistanceBottom3 = new Series<double>(this);
                supportTop1 = new Series<double>(this);
                supportBottom1 = new Series<double>(this);
                supportTop2 = new Series<double>(this);
                supportBottom2 = new Series<double>(this);
                supportTop3 = new Series<double>(this);
                supportBottom3 = new Series<double>(this);
            }
		}
		

		protected override void OnBarUpdate()
		{
            //NinjaTrader.Code.Output.Process("------------ Dynamic SR Values START ------------ \n", PrintTo.OutputTab2);
            if ( CurrentBar <= MaxLookBackBars ) 
				return;
		
			if ( LastBar != CurrentBar ) 
			{
				x = HighestBar(High, (PivotStrength*2)+1);
				if ( x == PivotStrength )
				{
					listEntry.Type 		= +1;
					listEntry.Price 	= High[x];
					listEntry.Bar 		= CurrentBar - x;
					pivot.Add(listEntry);
					
					if ( pivot.Count > MaxLookBackBars )
					{
						pivot.RemoveAt(0);
					}
				}
				x = LowestBar(Low, (PivotStrength*2)+1);
				if ( x == PivotStrength )
				{
					listEntry.Type		= -1;
					listEntry.Price		= Low[x];
					listEntry.Bar		= CurrentBar - x;
					pivot.Add(listEntry);
					
					if ( pivot.Count > MaxLookBackBars )
					{
						pivot.RemoveAt(0);
					}
				}
				
				if ( CurrentBar >= Bars.Count-2 && CurrentBar > MaxLookBackBars )
				{
					p = Close[0]; p1 = 0; str = "";
					for ( level=1; level <= MaxLevels ; level++)
					{
						p = get_sr_level (p,+1);
						Draw.Rectangle(this, "resZone"+level, false, 0, p-((ZoneTickSize*TickSize)/2), MaxLookBackBars, p+((ZoneTickSize*TickSize)/2), ColorAbove, ColorAbove, 30 );
						Draw.Line     (this, "resLine"+level, false, 0, p-((ZoneTickSize*TickSize)/2), MaxLookBackBars, p-((ZoneTickSize*TickSize)/2), ColorAbove, DashStyleHelper.Solid, 1);
						str = ( p == p1 ) ? str+" L"+level : "L"+level;	
						//Draw.Text	  (this,  "resTag"+level, false , "\t    "+str , -5 , p , 0, ColorAbove, boldFont, TextAlignment.Right, Brushes.Transparent , Brushes.Transparent , 0 );
						p1 = p; p += (PivotTickDiff*TickSize);
                        //resistanceVal[0] = p1 - .25;

                        double above1 = p - ((ZoneTickSize * TickSize) / 2);
                        double above2 = p + ((ZoneTickSize * TickSize) / 2);
                        double topish1 = ((ZoneTickSize * TickSize) / 2);
                        
                        if (level == 1)
                        {
                            resistanceVal[0] = p1 - .25;
                            resTop1 = p1 + .25;
                            resBottom1 = p1 - .25;

                            resistanceTop1[0] = resTop1;
                            resistanceBottom1[0] = resBottom1;
                            
                            /*NinjaTrader.Code.Output.Process("Top of Red Box1: " + resTop1.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("Bottom of Red Box1: " + resBottom1.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("\n", PrintTo.OutputTab2);
                            */  
                            //NinjaTrader.Code.Output.Process("resistanceTop1[0]: " + resistanceTop1[0].ToString(), PrintTo.OutputTab1);
                        }

                        if (level == 2)
                        {
                            resistanceVal[0] = p1 - .25;
                            resTop2 = p1 + .25;
                            resBottom2 = p1 - .25;

                            resistanceTop2[0] = resTop2;
                            resistanceBottom2[0] = resBottom2;

                            /*NinjaTrader.Code.Output.Process("Top of Red Box2: " + resTop2.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("Bottom of Red Box2: " + resBottom2.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("\n", PrintTo.OutputTab2);*/
                            
                        }

                        if (level == 3)
                        {
                            resistanceVal[0] = p1 - .25;
                            resTop3 = p1 + .25;
                            resBottom3 = p1 - .25;

                            resistanceTop3[0] = resTop3;
                            resistanceBottom3[0] = resBottom3;
                            
                            /*NinjaTrader.Code.Output.Process("Top of Red Box3: " + resTop3.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("Bottom of Red Box3: " + resBottom3.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("\n", PrintTo.OutputTab2);*/
                            
                        }
                        
                    }
					
					p = Close[0]; p1 = 0; str = "";
					for ( level = 1; level <= MaxLevels; level++)
					{
						p = get_sr_level (p, -1);
						Draw.Rectangle(this, "supZone"+level, false, 0, p-((ZoneTickSize*TickSize)/2), MaxLookBackBars, p+((ZoneTickSize*TickSize)/2), ColorBelow, ColorBelow, 30 );
						Draw.Line     (this, "supLine"+level, false, 0, p-((ZoneTickSize*TickSize)/2), MaxLookBackBars, p-((ZoneTickSize*TickSize)/2), ColorBelow, DashStyleHelper.Solid, 1);
						str = ( p == p1 ) ? str+" L"+level : "L"+level;	
						//Draw.Text	  (this, "supTag"+level, false , "\t    "+str , -5 , p , 0, ColorBelow, boldFont, TextAlignment.Right, Brushes.Transparent , Brushes.Transparent , 0 );
						p1 = p; p -= (PivotTickDiff*TickSize);
                        
                        if (level == 1)
                        {
                            supportVal[0] = p1 + .25;
                            supTop1 = p1 + .25;
                            supBottom1 = p1 - .25;

                            supportTop1[0] = supTop1;
                            supportBottom1[0] = supBottom1;
                            /*NinjaTrader.Code.Output.Process("Top of a Blue1: " + supTop1.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("Bottom of a Blue1: " + supBottom1.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("\n", PrintTo.OutputTab2);*/
                        }

                        if (level == 2)
                        {
                            supportVal[1] = p1 + .25;
                            supTop2 = p1 + .25;
                            supBottom2 = p1 - .25;

                            supportTop2[0] = supTop2;
                            supportBottom2[0] = supBottom2;
                            /*NinjaTrader.Code.Output.Process("Top of a Blue2: " + supTop2.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("Bottom of a Blue2: " + supBottom2.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("\n", PrintTo.OutputTab2);*/
                        }

                        if (level == 3)
                        {
                            supportVal[1] = p1 + .25;
                            supTop3 = p1 + .25;
                            supBottom3 = p1 - .25;

                            supportTop3[0] = supTop3;
                            supportBottom3[0] = supBottom3;
                            /*NinjaTrader.Code.Output.Process("Top of a Blue3: " + supTop3.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("Bottom of a Blue3: " + supBottom3.ToString(), PrintTo.OutputTab2);
                            NinjaTrader.Code.Output.Process("\n", PrintTo.OutputTab2);*/
                        }
                        

                        double below1 = p - ((ZoneTickSize * TickSize) / 2);
                        double below2 = p + ((ZoneTickSize * TickSize) / 2);
                    }
				}
			}
            //NinjaTrader.Code.Output.Process("------------ Dynamic SR Values END ------------ \n", PrintTo.OutputTab2);
            LastBar = CurrentBar;
        }
		
		double get_sr_level ( double refPrice, int pos)
		{
			int i=0, j=0;
			double levelPrice=0;
			
			if ( CurrentBar >= Bars.Count-2 && CurrentBar > MaxLookBackBars )
			{
				maxCountAbove=0; maxCountBelow=0; pxAbove=-1; pxBelow=-1;
				for ( i = pivot.Count-1; i >= 0; i--)
				{
					countAbove = 0; countBelow = 0;
					if ( pivot[i].Bar < CurrentBar-MaxLookBackBars ) break;
					for ( j=0; j < pivot.Count-1; j++)
					{
						if ( pivot[j].Bar > CurrentBar-MaxLookBackBars )
						{
							if ( pos > 0 && pivot[i].Price >= refPrice )
							{
								if( Math.Abs(pivot[i].Price-pivot[j].Price)/TickSize <= PivotTickDiff )
								{
									countAbove++;
								}
								if ( countAbove > maxCountAbove)
								{
									maxCountAbove = countAbove;
									levelPrice = pivot[i].Price;
									pxAbove = i;
								}
							}
							else
								if ( pos < 0 && pivot[i].Price <= refPrice )
								{
									if ( Math.Abs(pivot[i].Price-pivot[j].Price)/TickSize <= PivotTickDiff)
									{
										countBelow++;
									}
									if ( countBelow > maxCountBelow )
									{
										maxCountBelow = countBelow;
										levelPrice = pivot[i].Price;
										pxBelow = i;
									}
								}
							}
						}
					}
				
				if ( pos > 0 )
				{
					levelPrice = ( pxAbove >= 0 ) ? pivot[pxAbove].Price : High[HighestBar(High,MaxLookBackBars)];
				}
				if ( pos < 0 )
				{
					levelPrice = ( pxBelow >= 0 ) ? pivot[pxBelow].Price : Low[LowestBar(Low,MaxLookBackBars)];
				}
				
				}
			return Instrument.MasterInstrument.RoundToTickSize( levelPrice );
			
			}
			
		
			
			

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Swing Pivot Strength", Description = "How many bars should be on each side of Swing Hi/Low",  Order=1, GroupName="Parameters")]
		public int PivotStrength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Max Look Back Bars", Description = "Max Number of Bars to Look Back for calculations", Order=2, GroupName="Parameters")]
		public int MaxLookBackBars
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Pivot Tick Proximity", Description = "Proximity of pivots in Ticks to determine where S/R level will be", Order=3, GroupName="Parameters")]
		public int PivotTickDiff
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ZoneTickSize", Description = "Height in Ticks to draw S/R Zone around S/R level", Order=4, GroupName="Parameters")]
		public int ZoneTickSize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Max Levels", Description = "Number of S/R Levels To Draw Above and Below The Market", Order=5, GroupName="Parameters")]
		public int MaxLevels
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show Pivots", Order=6, GroupName="Parameters")]
		public bool ShowPivots
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="S/R Zone Color", Order=7, GroupName="Parameters")]
		public Brush ColorBelow
		{ get; set; }

		[Browsable(false)]
		public string ColorBelowSerializable
		{
			get { return Serialize.BrushToString(ColorBelow); }
			set { ColorBelow = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Resistance Zone Color", Order=8, GroupName="Parameters")]
		public Brush ColorAbove
		{ get; set; }

		[Browsable(false)]
		public string ColorAboveSerializable
		{
			get { return Serialize.BrushToString(ColorAbove); }
			set { ColorAbove = Serialize.StringToBrush(value); }
		}
        
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
        #endregion

        // Resistance
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ResTop1
        {
            get { return resistanceTop1; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ResBottom1
        {
            get { return resistanceBottom1; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ResTop2
        {
            get { return resistanceTop2; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ResBottom2
        {
            get { return resistanceBottom2; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ResTop3
        {
            get { return resistanceTop3; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ResBottom3
        {
            get { return resistanceBottom3; }
        }

        // Support
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SupTop1
        {
            get { return supportTop1; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SupBottom1
        {
            get { return supportBottom1; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SupTop2
        {
            get { return supportTop2; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SupBottom2
        {
            get { return supportBottom2; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SupTop3
        {
            get { return supportTop3; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SupBottom3
        {
            get { return supportBottom3; }
        }

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DynamicSRLines[] cacheDynamicSRLines;
		public DynamicSRLines DynamicSRLines(int pivotStrength, int maxLookBackBars, int pivotTickDiff, int zoneTickSize, int maxLevels, bool showPivots, Brush colorBelow, Brush colorAbove)
		{
			return DynamicSRLines(Input, pivotStrength, maxLookBackBars, pivotTickDiff, zoneTickSize, maxLevels, showPivots, colorBelow, colorAbove);
		}

		public DynamicSRLines DynamicSRLines(ISeries<double> input, int pivotStrength, int maxLookBackBars, int pivotTickDiff, int zoneTickSize, int maxLevels, bool showPivots, Brush colorBelow, Brush colorAbove)
		{
			if (cacheDynamicSRLines != null)
				for (int idx = 0; idx < cacheDynamicSRLines.Length; idx++)
					if (cacheDynamicSRLines[idx] != null && cacheDynamicSRLines[idx].PivotStrength == pivotStrength && cacheDynamicSRLines[idx].MaxLookBackBars == maxLookBackBars && cacheDynamicSRLines[idx].PivotTickDiff == pivotTickDiff && cacheDynamicSRLines[idx].ZoneTickSize == zoneTickSize && cacheDynamicSRLines[idx].MaxLevels == maxLevels && cacheDynamicSRLines[idx].ShowPivots == showPivots && cacheDynamicSRLines[idx].ColorBelow == colorBelow && cacheDynamicSRLines[idx].ColorAbove == colorAbove && cacheDynamicSRLines[idx].EqualsInput(input))
						return cacheDynamicSRLines[idx];
			return CacheIndicator<DynamicSRLines>(new DynamicSRLines(){ PivotStrength = pivotStrength, MaxLookBackBars = maxLookBackBars, PivotTickDiff = pivotTickDiff, ZoneTickSize = zoneTickSize, MaxLevels = maxLevels, ShowPivots = showPivots, ColorBelow = colorBelow, ColorAbove = colorAbove }, input, ref cacheDynamicSRLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DynamicSRLines DynamicSRLines(int pivotStrength, int maxLookBackBars, int pivotTickDiff, int zoneTickSize, int maxLevels, bool showPivots, Brush colorBelow, Brush colorAbove)
		{
			return indicator.DynamicSRLines(Input, pivotStrength, maxLookBackBars, pivotTickDiff, zoneTickSize, maxLevels, showPivots, colorBelow, colorAbove);
		}

		public Indicators.DynamicSRLines DynamicSRLines(ISeries<double> input , int pivotStrength, int maxLookBackBars, int pivotTickDiff, int zoneTickSize, int maxLevels, bool showPivots, Brush colorBelow, Brush colorAbove)
		{
			return indicator.DynamicSRLines(input, pivotStrength, maxLookBackBars, pivotTickDiff, zoneTickSize, maxLevels, showPivots, colorBelow, colorAbove);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DynamicSRLines DynamicSRLines(int pivotStrength, int maxLookBackBars, int pivotTickDiff, int zoneTickSize, int maxLevels, bool showPivots, Brush colorBelow, Brush colorAbove)
		{
			return indicator.DynamicSRLines(Input, pivotStrength, maxLookBackBars, pivotTickDiff, zoneTickSize, maxLevels, showPivots, colorBelow, colorAbove);
		}

		public Indicators.DynamicSRLines DynamicSRLines(ISeries<double> input , int pivotStrength, int maxLookBackBars, int pivotTickDiff, int zoneTickSize, int maxLevels, bool showPivots, Brush colorBelow, Brush colorAbove)
		{
			return indicator.DynamicSRLines(input, pivotStrength, maxLookBackBars, pivotTickDiff, zoneTickSize, maxLevels, showPivots, colorBelow, colorAbove);
		}
	}
}

#endregion
