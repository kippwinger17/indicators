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
	public class SixtyNineCross : Indicator
	{
        public Brush WeakBrush;
        public Brush StrongBrush;
        bool crossUp = false;
        bool crossDown = false;
        bool overNine = false;
        bool overSix = false;
        bool underSix = false;
        bool underNine = false; 

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"1.0";
				Name										= "SixtyNineCross";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                TBCheck = true;
                BACheck = true;
                TBBrush = Brushes.DarkOrange;
                WeakBrush = Brushes.White;
                StrongBrush = Brushes.DeepSkyBlue;
            }
            else if (State == State.Configure)
            {
            }

        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0)
                return;

            WeakCross();

            CrossRespectingThirty();

            ThirtyCrossUpAfterSixtyNine();

            ThirtyCrossDownAfterSixtyNine();
        }

        private void WeakCross()
        {
            if (CrossAbove(EMA(6), EMA(9), 1))
            { 
                Draw.ArrowUp(this, "WeakCrossUp" + Convert.ToString(CurrentBars[0]), true, 0, Low[0] - 3 * TickSize, WeakBrush);
                crossUp = true;
            }

            if (CrossAbove(EMA(9), EMA(6), 1))
            { 
                Draw.ArrowDown(this, "WeakCrossDown" + Convert.ToString(CurrentBars[0]), true, 0, High[0] + 3 * TickSize, WeakBrush);
                crossDown = true;
            }
        }

        private void CrossRespectingThirty()
        {
            if (CrossAbove(EMA(6), EMA(9), 1))
            {
                if (EMA(6)[0] > EMA(30)[0])
                    Draw.ArrowUp(this, "CrossUp" + Convert.ToString(CurrentBars[0]), true, 0, Low[0] - 3 * TickSize, TBBrush);
            }
            if (CrossAbove(EMA(9), EMA(6), 1))
            {
                if (EMA(6)[0] < EMA(30)[0])
                    Draw.ArrowDown(this, "CrossDown" + Convert.ToString(CurrentBars[0]), true, 0, High[0] + 3 * TickSize, TBBrush);
            }

        }

        private void ThirtyCrossUpAfterSixtyNine()
        {
            double ema30 = EMA(30)[0];
            double ema9 = EMA(9)[0];
            double ema6 = EMA(6)[0];
            //check to see when the 6 crosses above the 30 and then when the 9 crosses above the 30

            // 6 & 9 cross happened; looking for 6 crossing 30
            if (crossUp && (CrossAbove(EMA(6), EMA(30), 1)))
            {
                overSix = true;
            }
           

            // 6 & 9 cross happened; looking for 9 crossing 30
            if (crossUp && (CrossAbove(EMA(9), EMA(30), 1)))
            {
                overNine = true;
            }


            if (overSix && overNine)
            {
                Draw.ArrowUp(this, "30AfterCrossUp" + Convert.ToString(CurrentBars[0]), true, 0, Low[0] - 2 * TickSize, StrongBrush);
                overNine = false;
                overSix = false;
            }

        }

        private void ThirtyCrossDownAfterSixtyNine()
        {
            double ema30 = EMA(30)[0];
            double ema9 = EMA(9)[0];
            double ema6 = EMA(6)[0];
            //check to see when the 6 crosses above the 30 and then when the 9 crosses above the 30

            // 6 & 9 cross happened; looking for 6 crossing 30
            if (crossDown && (CrossBelow(EMA(6), EMA(30), 1)))
            {
                underSix = true;
            }


            // 6 & 9 cross happened; looking for 9 crossing 30
            if (crossDown && (CrossBelow(EMA(9), EMA(30), 1)))
            {
                underNine = true;
            }


            if (underNine && underSix)
            {
                Draw.ArrowDown(this, "30AfterCrossDown" + Convert.ToString(CurrentBars[0]), true, 0, High[0] + 2 * TickSize, StrongBrush);
                underNine = false;
                underSix = false;
            }

        }






        #region Parameters
        [Range(0, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Bars Ago Count", GroupName = "Parameters", Order = 1)]
        public bool BACheck
        { get; set; }

        [Range(0, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Total Bars Count", GroupName = "Parameters", Order = 2)]
        public bool TBCheck
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Total Bars", GroupName = "Text Color", Order = 1)]
        public Brush TBBrush
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Bars Ago", GroupName = "Text Color", Order = 2)]
        public Brush BABrush
        { get; set; }
        #endregion
    }


}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SixtyNineCross[] cacheSixtyNineCross;
		public SixtyNineCross SixtyNineCross(bool bACheck, bool tBCheck)
		{
			return SixtyNineCross(Input, bACheck, tBCheck);
		}

		public SixtyNineCross SixtyNineCross(ISeries<double> input, bool bACheck, bool tBCheck)
		{
			if (cacheSixtyNineCross != null)
				for (int idx = 0; idx < cacheSixtyNineCross.Length; idx++)
					if (cacheSixtyNineCross[idx] != null && cacheSixtyNineCross[idx].BACheck == bACheck && cacheSixtyNineCross[idx].TBCheck == tBCheck && cacheSixtyNineCross[idx].EqualsInput(input))
						return cacheSixtyNineCross[idx];
			return CacheIndicator<SixtyNineCross>(new SixtyNineCross(){ BACheck = bACheck, TBCheck = tBCheck }, input, ref cacheSixtyNineCross);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SixtyNineCross SixtyNineCross(bool bACheck, bool tBCheck)
		{
			return indicator.SixtyNineCross(Input, bACheck, tBCheck);
		}

		public Indicators.SixtyNineCross SixtyNineCross(ISeries<double> input , bool bACheck, bool tBCheck)
		{
			return indicator.SixtyNineCross(input, bACheck, tBCheck);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SixtyNineCross SixtyNineCross(bool bACheck, bool tBCheck)
		{
			return indicator.SixtyNineCross(Input, bACheck, tBCheck);
		}

		public Indicators.SixtyNineCross SixtyNineCross(ISeries<double> input , bool bACheck, bool tBCheck)
		{
			return indicator.SixtyNineCross(input, bACheck, tBCheck);
		}
	}
}

#endregion
