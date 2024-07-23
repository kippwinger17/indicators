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
using System.Globalization;

using System.Net;
using System.IO;
using System.IO.Compression;
using System.Net.Cache;
using System.Xml;
using System.Text;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;
//This namespace holds Indicators in this folder and is required. Do not change it. 


// 04-22-2019 - Changed logic that select brush color by impact to account for unexpected impact (caused object error on current brush)
// 03-15-2021 - Changed URL for news

namespace NinjaTrader.NinjaScript.Indicators
{
	public class jtEconNews3a : Indicator
	{
		
		private	List<TextLine> list;		
		
		private class TextColumn 
		{			
			public TextColumn(float padding, string text)
			{
				this.padding = padding;
				this.text = text;
			}
			public float padding;
			public string text;	
		}
		
		
		private class TextLine {
			public TextLine(SimpleFont  font,    System.Windows.Media.Brush brush)
			{	
				this.font = font;
				this.brush = brush;
			}
			public TextColumn timeColumn;
			public TextColumn impactColumn;
			public TextColumn descColumn;
			public SimpleFont font;
			public    System.Windows.Media.Brush brush;
		}
				
		public class NewsEvent 
		{
			public int ID;
			public string Title;
			public string Country;
			public string Date;
			public string Time;
			public string Impact;
			public string Forecast;
			public string Previous;
			[XmlIgnore()]
			public DateTime DateTimeLocal;
			public override string ToString(){
				return string.Format("ID: {0}, Title: {1}, Country: {2}, Date: {3}, Time: {4}, Impact: {5}, Forecast: {6}, Previous: {7}, DateTimeLocal: {8}",
					ID, Title, Country, Date, Time,Impact, Forecast, Previous, DateTimeLocal);  
			}
		}
; 
		//private const string ffNewsUrl = @"http://cdn-nfs.faireconomy.media/ff_calendar_thisweek.xml";   // 04-09-20919, was: http://www.forexfactory.com/ffcal_week_this.xml
		private const string ffNewsUrl = @"http://nfs.faireconomy.media/ff_calendar_thisweek.xml";  // changed 03/15/2021
		private const string TIME = "Time";
		private const string IMPACT = "Impact";
		private const string DESC = "News Event Description (prev/forecast)";
		private const float TIME_PAD = 10;
		private const float IMPACT_PAD = 10;
		private const float DESC_PAD = 0;
		public NewsEvent[] newsEvents = null; //was private
		private DateTime lastNewsUpdate = DateTime.MinValue;
		private string lastLoadError;
	
		private float widestTimeCol = 0;
		private float widestImpactCol = 0;
		private float widestDescCol = 0;
		private float totalHeight = 0;
		private float longestLine = 0;		
		private int lastNewsPtr = 0;
		private DateTime lastMinute;
        public bool highTimes = false;

		// Must specify that culture that should be used to parse the ForexFactory date/time data.
		private CultureInfo ffDateTimeCulture = CultureInfo.CreateSpecificCulture("en-US");
	
		private int newsItemPtr = 0;

		private	NinjaTrader.Gui.Tools.SimpleFont titleFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 10) { Bold = true };			
		private	NinjaTrader.Gui.Tools.SimpleFont defaultFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 10) { };		
		private	NinjaTrader.Gui.Tools.SimpleFont lineAlertFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 10) {  Bold = true,Italic = true };
	
		private    System.Windows.Media.Brush headerColor = Brushes.White;
		private    System.Windows.Media.Brush lineHighColor = Brushes.Red;
		private    System.Windows.Media.Brush lineMedColor = Brushes.DarkGreen;
		private    System.Windows.Media.Brush lineLowColor = Brushes.Blue;
		private    System.Windows.Media.Brush backgroundColor = Brushes.LightGray;	
	
		private    System.Windows.Media.Brush headerTitleBrush;
		private    System.Windows.Media.Brush lineHighBrush;
		private    System.Windows.Media.Brush lineMedBrush;
		private    System.Windows.Media.Brush lineLowBrush;
		private    System.Windows.Media.Brush lineNormalBrush;

  public override string DisplayName
  {
  get { return this.Name.ToString(); }
  }
		
		protected override void OnStateChange()
		{
			
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "jtEconNews3a";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				USOnlyEvents = false;
				Debug =false;
				NewsRefeshInterval = 15;
				ShowLowPriority =true;
				Use24timeFormat =false;
				TodaysNewsOnly=true;
				SendAlerts=true;
				AlertInterval=15;
				MaxNewsItems =10;					
				AlertWavFileName = "Alert1.wav";
			}
			else if (State == State.Configure)
			{
				headerTitleBrush = (headerColor);
				lineHighBrush = (lineHighColor);
				lineMedBrush = (lineMedColor);
				lineLowBrush =(lineLowColor);
				lastNewsPtr = -1;
				lastMinute = DateTime.MinValue;			
				list = new List<TextLine>();				
				LoadNews();				
			}
		}

		protected override void OnBarUpdate()
		{
            highTimes = false;
            // we only need to update at most every minute since this is the max granularity of the 
            //	 date/time of the news events.  This saves a little wear and tear.
            if (Time[0] >= lastMinute.AddMinutes(1)) 
			{
				if (Debug) Print("OnBarUpdate running...");
				
				lastMinute = Time[0];

				// download the news data every news refresh interval (not bar interval).
				if (lastNewsUpdate.AddMinutes(NewsRefeshInterval) < DateTime.Now){
					LoadNews();
				}
				
				newsItemPtr = -1;  // this will indicate that there are no pending items at this time.
				// set pointer to the first "pending" news item in the list based on current datetime.
				if (newsEvents != null && newsEvents.Length > 0)
				{
					for(int x = 0; x < newsEvents.Length ; x++){
						NewsEvent item = newsEvents[x];
						if (item.DateTimeLocal >= DateTime.Now ){
								newsItemPtr = x;
								break;
						}
					}
					
					BuildList();
				}
			}
		}
		
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
			
			 //Call base.OnRender() to ensure all base behavior is performed
            base.OnRender(chartControl, chartScale);

            // Instatiate a factory, which is required for the next step
            SharpDX.DirectWrite.Factory factory = new SharpDX.DirectWrite.Factory();
				
			TextFormat format_defaultFont =defaultFont.ToDirectWriteTextFormat();
			NinjaTrader.Gui.Tools.SimpleFont headerFont	= defaultFont;
			headerFont.Bold=true;
			TextFormat format_headerFont =headerFont.ToDirectWriteTextFormat();
			TextFormat format_lineAlertFont =lineAlertFont.ToDirectWriteTextFormat();
			TextFormat format_titleFont =titleFont.ToDirectWriteTextFormat();
	
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;
			SharpDX.DirectWrite.TextLayout textLayout1 =new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,"NEWS!", format_titleFont , ChartPanel.X + ChartPanel.W, format_titleFont.FontSize);
			float startPointY=(int)((ChartPanel.H*.05 ) ) ;
			float startPointX=(int)(ChartPanel.W*.05);					
			float lineSpaceOffsetAP = -textLayout1.Metrics.Height; //Gets hight of text
		
			startPointY= startPointY- lineSpaceOffsetAP;
			SharpDX.Vector2 upperTextPoint = new SharpDX.Vector2(startPointX,startPointY);

			int recXStartingPoint =(int)(ChartPanel.W*.05);
			int recYStartingPoint =(int)((ChartPanel.H*.05));

			//Need new vector with this offset.
			SharpDX.Vector2 upperTextPoint2 = new SharpDX.Vector2(startPointX,startPointY);
	
			widestTimeCol = 0;				
			widestImpactCol = 0;				
			widestDescCol = 0;				
			totalHeight = 0;
			totalHeight=0;
				
			SharpDX.DirectWrite.TextLayout textLayout3;
			
			for(int i =0; i< list.Count() ; i++)
			{	 
				textLayout3 =new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, list[i].timeColumn.text, format_defaultFont, ChartPanel.X + ChartPanel.W,  list[i].font.TextFormatHeight);
			
				float f = 	textLayout3.Metrics.Width;
		
				if (f > widestTimeCol)
					widestTimeCol = f;
			
				textLayout3 =new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, list[i].impactColumn.text, format_defaultFont, ChartPanel.X + ChartPanel.W,  list[i].font.TextFormatHeight);
				f = 	textLayout3.Metrics.Width;
				
				if (f > widestImpactCol) 
					widestImpactCol = f;

				textLayout3 =new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, list[i].descColumn.text, format_defaultFont, ChartPanel.X + ChartPanel.W,  list[i].font.TextFormatHeight);
				f = textLayout3.Metrics.Width;
				
				if (f > widestDescCol) 
					widestDescCol = f;
				
					
				lineSpaceOffsetAP = -textLayout3.Metrics.Height;
				
			}
			
			longestLine = widestTimeCol + widestImpactCol + widestDescCol;
				
			totalHeight = Math.Max(defaultFont.TextFormatHeight, titleFont.TextFormatHeight);
				
			int rec_X=		(int)(ChartPanel.W*.05);

			if(ShowBackground == true)
			{
				SharpDX.Rectangle recBackground = new SharpDX.Rectangle(rec_X,(int)( recYStartingPoint- lineSpaceOffsetAP), (int)(rec_X + longestLine+widestTimeCol), (int)(totalHeight- lineSpaceOffsetAP*(list.Count+2)));	
				RenderTarget.FillRectangle(recBackground, BackgroundColor.ToDxBrush(RenderTarget));
		
			}	
				RenderTarget.DrawTextLayout(upperTextPoint, textLayout1, 	headerTitleBrush.ToDxBrush(RenderTarget),	SharpDX.Direct2D1.DrawTextOptions.NoSnap);	
		
			startPointY= startPointY- lineSpaceOffsetAP;
	
			upperTextPoint2 = new SharpDX.Vector2(startPointX,startPointY);			
		
		
			int counter=0;
				for(int i =0; i< list.Count() ; i++)
				{
					TextFormat format_ToUse=format_defaultFont;
					if(counter<1)
					{
						format_ToUse=format_headerFont;
						
					}
					
					counter++;
					startPointY= startPointY- lineSpaceOffsetAP;
					upperTextPoint2 = new SharpDX.Vector2(startPointX,startPointY);	
					textLayout3 =new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, list[i].timeColumn.text, format_ToUse, ChartPanel.X + ChartPanel.W, list[i].font.TextFormatHeight);
					
					RenderTarget.DrawTextLayout(upperTextPoint2, textLayout3,  list[i].brush.ToDxBrush(RenderTarget),	SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					startPointX += startPointX + 		widestTimeCol;
		
					upperTextPoint2 = new SharpDX.Vector2(startPointX,startPointY);						
					SharpDX.DirectWrite.TextLayout textLayout4 =new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,  list[i].impactColumn.text, format_ToUse, ChartPanel.X + ChartPanel.W,  list[i].font.TextFormatHeight);
					RenderTarget.DrawTextLayout(upperTextPoint2, textLayout4, 	 list[i].brush.ToDxBrush(RenderTarget),	SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					
					startPointX +=  widestImpactCol/2+ widestTimeCol;

					upperTextPoint2 = new SharpDX.Vector2(startPointX,startPointY);				
					SharpDX.DirectWrite.TextLayout textLayout5 =new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,  list[i].descColumn.text, format_ToUse, ChartPanel.X + ChartPanel.W,  list[i].font.TextFormatHeight);
					RenderTarget.DrawTextLayout(upperTextPoint2, textLayout5,  list[i].brush.ToDxBrush(RenderTarget),	SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					
					startPointX=(int)(ChartPanel.W*.05);
					
				}
		
			format_defaultFont.Dispose(); //10/28/2019
			format_headerFont.Dispose(); //10/28/2019
			format_lineAlertFont.Dispose(); //10/28/2019
			format_titleFont.Dispose();	//10/28/2019
		
		}
						
			private void BuildList(){

			if (Debug) Print("Building List. lastNewsPtr: " + lastNewsPtr + " newsItemPtr: " + newsItemPtr);
		
			list = new System.Collections.Generic.List<TextLine>();
	
			// add headers
			TextLine line = new TextLine(defaultFont, headerTitleBrush);
			line.timeColumn = new TextColumn(TIME_PAD, TIME);
			line.impactColumn = new TextColumn(IMPACT_PAD, IMPACT);
			line.descColumn = new TextColumn(DESC_PAD, DESC);
			list.Add(line);
				
			int lineCnt = 0;

			// add detail lines...
			for (int x = newsItemPtr; x < newsEvents.Length; x++){
					
				lineCnt++;
					
				// limit the number of pending events to be displayed.
				if (lineCnt > MaxNewsItems) break;
				
				NewsEvent item = newsEvents[x];

				Priority alertPriority = Priority.Low;
				System.Windows.Media.Brush  lineBrush = lineNormalBrush;
				
				if (item.Impact.ToUpper() == "HIGH")
				{
					lineBrush = lineHighBrush;
					alertPriority = Priority.High;
				} 
				else 	if (item.Impact.ToUpper() == "MEDIUM")
				{
					lineBrush = lineMedBrush;
					alertPriority = Priority.Medium;
				} 
				else 	if (item.Impact.ToUpper() == "LOW")
				{
					lineBrush = lineLowBrush;
				} 
				else lineBrush = lineLowBrush; // added 04/22/2019 to account for impact that doesn't match high/medium/low

				
				string tempTime = null;
				if (Use24timeFormat){
					tempTime = item.DateTimeLocal.ToString(" HH:mm", ffDateTimeCulture);
				} else {
					tempTime = item.DateTimeLocal.ToString("hh:mm tt", ffDateTimeCulture);
				}

				
				if (!TodaysNewsOnly){
					tempTime = item.DateTimeLocal.ToString("MM/dd ") + tempTime;
				}
				

				SimpleFont tempFont;
				TimeSpan diff = item.DateTimeLocal - DateTime.Now;

			
				if (SendAlerts && diff.TotalMinutes <= AlertInterval  )
				{
					tempFont = lineAlertFont;
			    Alert("jtEconNewsAlert"+item.ID.ToString(), Priority.High, string.Format( "News Alert: {0} {1}: {2}" , item.DateTimeLocal,item.Country, item.Title), NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+AlertWavFileName, 10, Brushes.Black, Brushes.Yellow);                   

					
				} else {
					tempFont = defaultFont;
				}
				
	
				line = new TextLine(tempFont, lineBrush);
				line.timeColumn = new TextColumn(TIME_PAD, tempTime);
				line.impactColumn = new TextColumn(IMPACT_PAD, item.Impact);
				
				string templine = null;
				if (item.Previous.Trim().Length == 0 && item.Forecast.Trim().Length == 0){
					templine = string.Format("{0}{1}", USOnlyEvents?"":item.Country+": " , item.Title);
				} else {
					templine = string.Format("{0}{1} ({2}/{3})", USOnlyEvents?"":item.Country+": " , item.Title, item.Previous, item.Forecast  );
				}
				
				line.descColumn = new TextColumn(DESC_PAD, templine);
				list.Add(line);
				
			}
		}
		private void LoadNews(){

			lastNewsUpdate = DateTime.Now; 
			lastLoadError = null;

			try {
				
				if (Debug)
				{
					Print("LoadNews()....");
					string[] patts = CultureInfo.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns();
					Print("All DateTime Patterns for culture: " + CultureInfo.CurrentCulture.Name);
					foreach(string patt in patts)
					{
						Print("    " + patt);
					}
					Print("End of DateTime Patterns");
				}
					
				
				// add a random query string to defeat server side caching.
				string urltweak = ffNewsUrl + "?x=" + Convert.ToString(DateTime.Now.Ticks);
				
				if (Debug) Print("Loading news from URL: " + urltweak);
				
				HttpWebRequest newsReq = (HttpWebRequest)HttpWebRequest.Create(urltweak);
				newsReq.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Reload);
				
				// fetch the xml doc from the web server
				
				using (HttpWebResponse newsResp = (HttpWebResponse)newsReq.GetResponse()){
					// check that we got a valid reponse
					if (newsResp != null && newsResp.StatusCode == HttpStatusCode.OK){
						// read the response stream into and xml document
						Stream receiveStream = newsResp.GetResponseStream();
						Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
						StreamReader readStream = new StreamReader( receiveStream, encode );
						string xmlString = readStream.ReadToEnd();
						
						if (Debug) Print("RAW http response: " + xmlString);
						XmlDocument newsDoc = new XmlDocument();
						newsDoc.LoadXml(xmlString);

						if (Debug) Print("XML news event node count: " + newsDoc.DocumentElement.ChildNodes.Count );
						
						// build collection of events
						
						System.Collections.ArrayList list = new System.Collections.ArrayList();
						int itemId = 0;
				
						for(int i =1; i<newsDoc.DocumentElement.ChildNodes.Count; i++)
			
						//	(XmlNode xmlNode in newsDoc.DocumentElement.ChildNodes)
						
					//	foreach(XmlNode xmlNode in newsDoc.DocumentElement.ChildNodes)
						{
							
							NewsEvent newsEvent = new NewsEvent();
							newsEvent.Time = 			newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("time").InnerText;
							if (string.IsNullOrEmpty(newsEvent.Time)) continue;  // ignore tentative events!
							newsEvent.Date = 			newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("date").InnerText;
							// assembly and convert event date/time to local time.
							if (Debug) Print(string.Format("About to parse Date '{0}', Time '{1}'", newsEvent.Date, newsEvent.Time));
							newsEvent.DateTimeLocal = DateTime.SpecifyKind(DateTime.Parse( newsEvent.Date + " " + newsEvent.Time, ffDateTimeCulture), DateTimeKind.Utc).ToLocalTime();
							if (Debug) Print("Succesfully parsed datetime: " + newsEvent.DateTimeLocal.ToString() + " to local time.");
							// filter events based on settings...
							DateTime startTime = DateTime.Now;
							DateTime endTime = startTime.AddDays(1);
							
							// filter news events based on various property settings...
							if (newsEvent.DateTimeLocal >= startTime && (!TodaysNewsOnly || newsEvent.DateTimeLocal.Date < endTime.Date))
							{
								newsEvent.ID = ++itemId;
								newsEvent.Country = 			newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("country").InnerText;
								if (USOnlyEvents && newsEvent.Country != "USD") continue;
								newsEvent.Forecast = 			newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("forecast").InnerText;
								newsEvent.Impact = 			newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("impact").InnerText;
								if (!ShowLowPriority && newsEvent.Impact.ToUpper() == "LOW") continue;
								newsEvent.Previous = 			newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("previous").InnerText;
								newsEvent.Title = 			newsDoc.DocumentElement.ChildNodes[i].SelectSingleNode("title").InnerText;
								list.Add(newsEvent);
								if (Debug) Print("Added: " + newsEvent.ToString());
                                if (newsEvent.Impact == "High")
                                    highTimes = true;

                            }
						}

						newsEvents = (NewsEvent[])list.ToArray(typeof(NewsEvent));
						if (Debug) Print("Added a total of " + list.Count  + " events to array.");
					} else {
						// handle unexpected scenarios...
						if (newsResp == null) throw new Exception("Web response was null.");
						else throw new Exception("Web response status code = " + newsResp.StatusCode.ToString());
					}
				}
				
			} catch (Exception ex){
				Print("LoadNews error in jtEconNews2A"+ex.ToString());
				Log("LoadNews error in jtEconNews2A"+ex.ToString(),LogLevel.Information);
				lastLoadError = ex.Message;
			}
		}
		
		
		public static DateTime ParseDateFromString(string s)
		{
			//Used to parse news columns
				string[] formats= { "yyyyMMdd","MM/dd/yyyy","MM-dd-yyyy","M-dd-yyyy", "M-d-yyyy", "MM-d-yyyy",
				"yyyy-MM-dd", "yyyy-M-d", "yyyy-MM-d", "yyyy-M-dd",
				"M/dd/yyyy", "M/d/yyyy", "MM/d/yyyy", 	"MM/dd/yyyy hh:mm:ss tt", 	"yyyy-MM-dd hh:mm:ss" ,	
				//Got from news parser, probably good to have just added them.				
				"M/d/yyyy","M/d/yy","MM/dd/yy","MM/dd/yyyy","yy/MM/dd","yyyy-MM-dd",
				"dd-MMM-yy","dddd, MMMM d, yyyy","dddd, MMMM dd, yyyy","MMMM dd, yyyy","dddd, dd MMMM, yyyy","dd MMMM, yyyy",
				"dddd, MMMM d, yyyy h:mm tt","dddd, MMMM d, yyyy hh:mm tt","dddd, MMMM d, yyyy H:mm",
				"dddd, MMMM d, yyyy HH:mm","dddd, MMMM dd, yyyy h:mm tt","dddd, MMMM dd, yyyy hh:mm tt",
				"dddd, MMMM dd, yyyy H:mm","dddd, MMMM dd, yyyy HH:mm","MMMM dd, yyyy h:mm tt",
				"MMMM dd, yyyy hh:mm tt","MMMM dd, yyyy H:mm","MMMM dd, yyyy HH:mm","dddd, dd MMMM, yyyy h:mm tt",
				"dddd, dd MMMM, yyyy hh:mm tt","dddd, dd MMMM, yyyy H:mm","dddd, dd MMMM, yyyy HH:mm","dd MMMM, yyyy h:mm tt",
				"dd MMMM, yyyy hh:mm tt","dd MMMM, yyyy H:mm","dd MMMM, yyyy HH:mm","dddd, MMMM d, yyyy h:mm:ss tt","dddd, MMMM d, yyyy hh:mm:ss tt",
				"dddd, MMMM d, yyyy H:mm:ss","dddd, MMMM d, yyyy HH:mm:ss","dddd, MMMM dd, yyyy h:mm:ss tt","dddd, MMMM dd, yyyy hh:mm:ss tt",
				"dddd, MMMM dd, yyyy H:mm:ss","dddd, MMMM dd, yyyy HH:mm:ss","MMMM dd, yyyy h:mm:ss tt","MMMM dd, yyyy hh:mm:ss tt",
				"MMMM dd, yyyy H:mm:ss","MMMM dd, yyyy HH:mm:ss","dddd, dd MMMM, yyyy h:mm:ss tt",
				"dddd, dd MMMM, yyyy hh:mm:ss tt","dddd, dd MMMM, yyyy H:mm:ss","dddd, dd MMMM, yyyy HH:mm:ss","dd MMMM, yyyy h:mm:ss tt",
				"dd MMMM, yyyy hh:mm:ss tt","dd MMMM, yyyy H:mm:ss","dd MMMM, yyyy HH:mm:ss","M/d/yyyy h:mm tt","M/d/yyyy hh:mm tt","M/d/yyyy H:mm",
				"M/d/yyyy HH:mm","M/d/yy h:mm tt","M/d/yy hh:mm tt","M/d/yy H:mm","M/d/yy HH:mm","MM/dd/yy h:mm tt",
				"MM/dd/yy hh:mm tt","MM/dd/yy H:mm","MM/dd/yy HH:mm","MM/dd/yyyy h:mm tt",
				"MM/dd/yyyy hh:mm tt","MM/dd/yyyy H:mm","MM/dd/yyyy HH:mm","yy/MM/dd h:mm tt",
				"yy/MM/dd hh:mm tt","yy/MM/dd H:mm","yy/MM/dd HH:mm","yyyy-MM-dd h:mm tt","yyyy-MM-dd hh:mm tt",
				"yyyy-MM-dd H:mm","yyyy-MM-dd HH:mm","dd-MMM-yy h:mm tt","dd-MMM-yy hh:mm tt","dd-MMM-yy H:mm","dd-MMM-yy HH:mm",
				"M/d/yyyy h:mm:ss tt","M/d/yyyy hh:mm:ss tt","M/d/yyyy H:mm:ss",
				"M/d/yyyy HH:mm:ss","M/d/yy h:mm:ss tt","M/d/yy hh:mm:ss tt","M/d/yy H:mm:ss","M/d/yy HH:mm:ss",
				"MM/dd/yy h:mm:ss tt","MM/dd/yy hh:mm:ss tt","MM/dd/yy H:mm:ss","MM/dd/yy HH:mm:ss","MM/dd/yyyy h:mm:ss tt",
				"MM/dd/yyyy hh:mm:ss tt","MM/dd/yyyy H:mm:ss","MM/dd/yyyy HH:mm:ss","yy/MM/dd h:mm:ss tt",
				"yy/MM/dd hh:mm:ss tt","yy/MM/dd H:mm:ss","yy/MM/dd HH:mm:ss","yyyy-MM-dd h:mm:ss tt","yyyy-MM-dd hh:mm:ss tt",
				"yyyy-MM-dd H:mm:ss","yyyy-MM-dd HH:mm:ss","dd-MMM-yy h:mm:ss tt","dd-MMM-yy hh:mm:ss tt","dd-MMM-yy H:mm:ss",
				"dd-MMM-yy HH:mm:ss","MMMM dd","MMMM dd","yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK","yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK",
				"ddd, dd MMM yyyy HH':'mm':'ss 'GMT'","ddd, dd MMM yyyy HH':'mm':'ss 'GMT'","yyyy'-'MM'-'dd'T'HH':'mm':'ss",
				"h:mm tt","hh:mm tt","H:mm","HH:mm","h:mm:ss tt","hh:mm:ss tt","H:mm:ss","HH:mm:ss",
				"yyyy'-'MM'-'dd HH':'mm':'ss'Z'","dddd, MMMM d, yyyy h:mm:ss tt","dddd, MMMM d, yyyy hh:mm:ss tt","dddd, MMMM d, yyyy H:mm:ss",
				"dddd, MMMM d, yyyy HH:mm:ss","dddd, MMMM dd, yyyy h:mm:ss tt",
				"dddd, MMMM dd, yyyy hh:mm:ss tt","dddd, MMMM dd, yyyy H:mm:ss","dddd, MMMM dd, yyyy HH:mm:ss","MMMM dd, yyyy h:mm:ss tt",
				"MMMM dd, yyyy hh:mm:ss tt","MMMM dd, yyyy H:mm:ss","MMMM dd, yyyy HH:mm:ss","dddd, dd MMMM, yyyy h:mm:ss tt",
				"dddd, dd MMMM, yyyy hh:mm:ss tt","dddd, dd MMMM, yyyy H:mm:ss", "dddd, dd MMMM, yyyy HH:mm:ss","dd MMMM, yyyy h:mm:ss tt",
				"dd MMMM, yyyy hh:mm:ss tt","dd MMMM, yyyy H:mm:ss", 
				"dd MMMM, yyyy HH:mm:ss", "MMMM yyyy", "MMMM, yyyy", "MMMM yyyy","MMMM, yyyy"};
			
				
				string myString;
				DateTime dateFortmated;					

				//Having a try catch in here was changing our results!!!  Cause I had to return something so I returned datetime.price.
				dateFortmated = DateTime.ParseExact(s, formats, new CultureInfo("en-GB"), DateTimeStyles.None);
				return dateFortmated;		
		}
		
		
		  #region Properties

		[NinjaScriptProperty]
		[Display(Name="ShowBackground", Description="Enable or disable background display.", Order=1, GroupName="Display")]
		public bool ShowBackground
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MaxNewsItems",Description="Max number of pending news events to display.", Order=1, GroupName="Filter News")]
		public int MaxNewsItems
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="NewsRefeshInterval",Description="News refresh interval in minutes.", Order=1, GroupName="Parameters")]
		public int NewsRefeshInterval
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="Use24timeFormat", Description="Time display format, true for 24 hour, false for AM/PM.", Order=1, GroupName="Display")]
		public bool Use24timeFormat
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="Debug", Description="Debug to OutputWindow", Order=1, GroupName="Debug")]
		public bool Debug
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="TodaysNewsOnly", Description="Show only todays news events.", Order=1, GroupName="Filter News")]
		public bool TodaysNewsOnly
		{ get; set; }
		

 		[NinjaScriptProperty]
		[Display(Name="SendAlerts", Description="Send alerts to the Alerts Window..", Order=1, GroupName="Alerts")]
		public bool SendAlerts
		{ get; set; }
		
		

		[NinjaScriptProperty]
		[Display(Name="AlertInterval",Description="Number of minutes to warn before an event.", Order=1, GroupName="Alerts")]
		public int AlertInterval
		{ get; set; }
		
	
		[NinjaScriptProperty]
		[Display(Name="AlertWavFileName", Description="Alert wav file, blank to turn off audio alerts.", Order=1, GroupName="Alerts")]
		public string AlertWavFileName
		{ get; set; }
		
	
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="DefaultFont", Description = "Default Event Font", GroupName = "Fonts", Order = 4)]
		public SimpleFont DefaultFont
		{
			get{return defaultFont;}
			set{defaultFont = value;}
		}
		
		[Browsable(false)]	
		public string DefaultFontSerialize 
		{
			
	    get { return defaultFont.FamilySerialize;}
	    set { defaultFont = new SimpleFont(value, DefaultFontSizeSerialize); }

		}
		[Browsable(false)]	
		public double DefaultFontSizeSerialize 
		{
			
	    get { return DefaultFont.Size;}
	    set {DefaultFont.Size = value; }

		}
		

		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="DefaultFont", Description = "Default Event Font", GroupName = "Fonts", Order = 4)]
		public SimpleFont WarningFont
		{
			get{return lineAlertFont;}
			set{lineAlertFont = value;}
		}
		
		[Browsable(false)]	
		public string WarningFontSerialize 
		{
			
	    get { return lineAlertFont.FamilySerialize;}
	    set { lineAlertFont = new SimpleFont(value, DefaultFontSizeSerialize); }

		}

		[Browsable(false)]	
		public double WarningFontSizeSerialize 
		{
			
	    get { return WarningFont.Size;}
	    set {WarningFont.Size = value; }

		}

	 
		[NinjaScriptProperty]
		[Display(Name="USOnlyEvents", Description="how only US events.", Order=1, GroupName="Filter News")]
		public bool USOnlyEvents
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="ShowLowPriority", Description="ShowLowPriority", Order=1, GroupName="Filter News")]
		public bool ShowLowPriority
		{ get; set; }
		
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="HeaderColor", Description = "Header Color", GroupName = "Colors", Order = 4)]
		public    System.Windows.Media.Brush HeaderColor
		{
			get{return headerColor;}
			set{headerColor = value;}
		}
		 [Browsable(false)]
		public string HeaderColorSerialize
		{
			get{return Serialize.BrushToString(headerColor);}
			set{headerColor = Serialize.StringToBrush(value);}
		}
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="HighPriorityColor", Description = "High Priority Color", GroupName = "Colors", Order = 4)]
		public    System.Windows.Media.Brush HighPriorityColor
		{
			get{return lineHighColor;}
			set{lineHighColor = value;}
		}
		 [Browsable(false)]
		public string HighPriorityColorSerialize
		{
			get{return Serialize.BrushToString(lineHighColor);}
			set{lineHighColor = Serialize.StringToBrush(value);}
		}
		
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="MediumPriorityColor", Description = "Medium Priority Color", GroupName = "Colors", Order = 4)]
		public    System.Windows.Media.Brush MediumPriorityColor
		{
			get{return lineMedColor;}
			set{lineMedColor = value;}
		}
		[Browsable(false)]
		public string MediumPriorityColorSerialize
		{
			get{return Serialize.BrushToString(lineMedColor);}
			set{lineMedColor = Serialize.StringToBrush(value);}
		}
		
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="LowPriorityColor", Description = "Low Priority Color", GroupName = "Colors", Order = 4)]
		public    System.Windows.Media.Brush LowPriorityColor
		{
			get{return lineLowColor;}
			set{lineLowColor = value;}
		}
		[Browsable(false)]
		public string LowPriorityColorSerialize
		{
			get{return Serialize.BrushToString(lineLowColor);}
			set{lineLowColor = Serialize.StringToBrush(value);}
		}
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(Name="BackgroundColor", Description = "Background Color", GroupName = "Colors", Order = 4)]
		public    System.Windows.Media.Brush BackgroundColor
		{
			get{return backgroundColor;}
			set{backgroundColor = value;}
		}
		[Browsable(false)]
		public string BackgroundColorSerialize
		{
			get{return Serialize.BrushToString(backgroundColor);}
			set{backgroundColor = Serialize.StringToBrush(value);}
		}

        [Browsable(false)]
        [XmlIgnore]
        public bool HighTimes
        {
            get { Update();  return highTimes; }
        }


        #endregion

    }
	
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private jtEconNews3a[] cachejtEconNews3a;
		public jtEconNews3a jtEconNews3a(bool showBackground, int maxNewsItems, int newsRefeshInterval, bool use24timeFormat, bool debug, bool todaysNewsOnly, bool sendAlerts, int alertInterval, string alertWavFileName, SimpleFont defaultFont, SimpleFont warningFont, bool uSOnlyEvents, bool showLowPriority, System.Windows.Media.Brush headerColor, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush backgroundColor)
		{
			return jtEconNews3a(Input, showBackground, maxNewsItems, newsRefeshInterval, use24timeFormat, debug, todaysNewsOnly, sendAlerts, alertInterval, alertWavFileName, defaultFont, warningFont, uSOnlyEvents, showLowPriority, headerColor, highPriorityColor, mediumPriorityColor, lowPriorityColor, backgroundColor);
		}

		public jtEconNews3a jtEconNews3a(ISeries<double> input, bool showBackground, int maxNewsItems, int newsRefeshInterval, bool use24timeFormat, bool debug, bool todaysNewsOnly, bool sendAlerts, int alertInterval, string alertWavFileName, SimpleFont defaultFont, SimpleFont warningFont, bool uSOnlyEvents, bool showLowPriority, System.Windows.Media.Brush headerColor, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush backgroundColor)
		{
			if (cachejtEconNews3a != null)
				for (int idx = 0; idx < cachejtEconNews3a.Length; idx++)
					if (cachejtEconNews3a[idx] != null && cachejtEconNews3a[idx].ShowBackground == showBackground && cachejtEconNews3a[idx].MaxNewsItems == maxNewsItems && cachejtEconNews3a[idx].NewsRefeshInterval == newsRefeshInterval && cachejtEconNews3a[idx].Use24timeFormat == use24timeFormat && cachejtEconNews3a[idx].Debug == debug && cachejtEconNews3a[idx].TodaysNewsOnly == todaysNewsOnly && cachejtEconNews3a[idx].SendAlerts == sendAlerts && cachejtEconNews3a[idx].AlertInterval == alertInterval && cachejtEconNews3a[idx].AlertWavFileName == alertWavFileName && cachejtEconNews3a[idx].DefaultFont == defaultFont && cachejtEconNews3a[idx].WarningFont == warningFont && cachejtEconNews3a[idx].USOnlyEvents == uSOnlyEvents && cachejtEconNews3a[idx].ShowLowPriority == showLowPriority && cachejtEconNews3a[idx].HeaderColor == headerColor && cachejtEconNews3a[idx].HighPriorityColor == highPriorityColor && cachejtEconNews3a[idx].MediumPriorityColor == mediumPriorityColor && cachejtEconNews3a[idx].LowPriorityColor == lowPriorityColor && cachejtEconNews3a[idx].BackgroundColor == backgroundColor && cachejtEconNews3a[idx].EqualsInput(input))
						return cachejtEconNews3a[idx];
			return CacheIndicator<jtEconNews3a>(new jtEconNews3a(){ ShowBackground = showBackground, MaxNewsItems = maxNewsItems, NewsRefeshInterval = newsRefeshInterval, Use24timeFormat = use24timeFormat, Debug = debug, TodaysNewsOnly = todaysNewsOnly, SendAlerts = sendAlerts, AlertInterval = alertInterval, AlertWavFileName = alertWavFileName, DefaultFont = defaultFont, WarningFont = warningFont, USOnlyEvents = uSOnlyEvents, ShowLowPriority = showLowPriority, HeaderColor = headerColor, HighPriorityColor = highPriorityColor, MediumPriorityColor = mediumPriorityColor, LowPriorityColor = lowPriorityColor, BackgroundColor = backgroundColor }, input, ref cachejtEconNews3a);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.jtEconNews3a jtEconNews3a(bool showBackground, int maxNewsItems, int newsRefeshInterval, bool use24timeFormat, bool debug, bool todaysNewsOnly, bool sendAlerts, int alertInterval, string alertWavFileName, SimpleFont defaultFont, SimpleFont warningFont, bool uSOnlyEvents, bool showLowPriority, System.Windows.Media.Brush headerColor, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush backgroundColor)
		{
			return indicator.jtEconNews3a(Input, showBackground, maxNewsItems, newsRefeshInterval, use24timeFormat, debug, todaysNewsOnly, sendAlerts, alertInterval, alertWavFileName, defaultFont, warningFont, uSOnlyEvents, showLowPriority, headerColor, highPriorityColor, mediumPriorityColor, lowPriorityColor, backgroundColor);
		}

		public Indicators.jtEconNews3a jtEconNews3a(ISeries<double> input , bool showBackground, int maxNewsItems, int newsRefeshInterval, bool use24timeFormat, bool debug, bool todaysNewsOnly, bool sendAlerts, int alertInterval, string alertWavFileName, SimpleFont defaultFont, SimpleFont warningFont, bool uSOnlyEvents, bool showLowPriority, System.Windows.Media.Brush headerColor, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush backgroundColor)
		{
			return indicator.jtEconNews3a(input, showBackground, maxNewsItems, newsRefeshInterval, use24timeFormat, debug, todaysNewsOnly, sendAlerts, alertInterval, alertWavFileName, defaultFont, warningFont, uSOnlyEvents, showLowPriority, headerColor, highPriorityColor, mediumPriorityColor, lowPriorityColor, backgroundColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.jtEconNews3a jtEconNews3a(bool showBackground, int maxNewsItems, int newsRefeshInterval, bool use24timeFormat, bool debug, bool todaysNewsOnly, bool sendAlerts, int alertInterval, string alertWavFileName, SimpleFont defaultFont, SimpleFont warningFont, bool uSOnlyEvents, bool showLowPriority, System.Windows.Media.Brush headerColor, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush backgroundColor)
		{
			return indicator.jtEconNews3a(Input, showBackground, maxNewsItems, newsRefeshInterval, use24timeFormat, debug, todaysNewsOnly, sendAlerts, alertInterval, alertWavFileName, defaultFont, warningFont, uSOnlyEvents, showLowPriority, headerColor, highPriorityColor, mediumPriorityColor, lowPriorityColor, backgroundColor);
		}

		public Indicators.jtEconNews3a jtEconNews3a(ISeries<double> input , bool showBackground, int maxNewsItems, int newsRefeshInterval, bool use24timeFormat, bool debug, bool todaysNewsOnly, bool sendAlerts, int alertInterval, string alertWavFileName, SimpleFont defaultFont, SimpleFont warningFont, bool uSOnlyEvents, bool showLowPriority, System.Windows.Media.Brush headerColor, System.Windows.Media.Brush highPriorityColor, System.Windows.Media.Brush mediumPriorityColor, System.Windows.Media.Brush lowPriorityColor, System.Windows.Media.Brush backgroundColor)
		{
			return indicator.jtEconNews3a(input, showBackground, maxNewsItems, newsRefeshInterval, use24timeFormat, debug, todaysNewsOnly, sendAlerts, alertInterval, alertWavFileName, defaultFont, warningFont, uSOnlyEvents, showLowPriority, headerColor, highPriorityColor, mediumPriorityColor, lowPriorityColor, backgroundColor);
		}
	}
}

#endregion
