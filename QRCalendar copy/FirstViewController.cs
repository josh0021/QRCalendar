using System;
using System.Globalization;
using ZXing;
using UIKit;
using ZXing.Mobile;
using EventKit;
using EventKitUI;
using Foundation;

namespace QRCalendar
{
	public partial class FirstViewController : UIViewController
	{
		public FirstViewController (IntPtr handle) : base (handle)
		{
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

		}


		override async public void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if (ParentViewController != null) 
			{
				CustomOverlayView customOverlay = new CustomOverlayView();
				var scanner = new ZXing.Mobile.MobileBarcodeScanner (this);

				customOverlay.ButtonCancel.TouchUpInside += delegate {
					Console.WriteLine("cancelpresssed");
					this.TabBarController.SelectedIndex = 1;
				};

				//Tell our scanner to use our custom overlay
				scanner.UseCustomOverlay = true;
				//We can customize the top and bottom text of the default overlay
				scanner.TopText = "Hold camera up to barcode to scan";
				scanner.BottomText = "Barcode will automatically scan";

				//Start scanning
				var result = await scanner.Scan (true);
				HandleScanResult (result);  

			}
		}


		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	
		void HandleScanResult(ZXing.Result result)
		{
			App.Current.EventStore.RequestAccess (EKEntityType.Event, 
				(bool granted, NSError e) => {
					if (granted)
					{
						EKEvent newEvent = EKEvent.FromStore ( App.Current.EventStore );
						// make the event start 20 minutes from now and last 30 minutes
						newEvent.StartDate = DateTimeToNSDate(DateTime.Now.AddMinutes ( 20 ));
						newEvent.EndDate = DateTimeToNSDate(DateTime.Now.AddMinutes ( 50 ));
						newEvent.Title = "Get outside and do some exercise!";
						newEvent.Notes = "This is your motivational event to go and do 30 minutes of exercise. Super important. Do this.";

						newEvent.Calendar = App.Current.EventStore.DefaultCalendarForNewEvents;

					App.Current.EventStore.SaveEvent ( newEvent, EKSpan.ThisEvent, out e );
					}
					else
						new UIAlertView ( "Access Denied", 
							"User Denied Access to Calendar Data", null,
							"ok", null).Show ();
				} );
			

			string testmsg = "test";
			if (result != null && !string.IsNullOrEmpty (result.Text)) {
				pp.Current.EventStore.RequestAccess (EKEntityType.Event, 
					(bool granted, NSError e) => {
						if (granted)
						{
							EKEvent newEvent = EKEvent.FromStore ( App.Current.EventStore );
							// make the event start 20 minutes from now and last 30 minutes
							newEvent.StartDate = DateTimeToNSDate(DateTime.Now.AddMinutes ( 20 ));
							newEvent.EndDate = DateTimeToNSDate(DateTime.Now.AddMinutes ( 50 ));
							newEvent.Title = "Get outside and do some exercise!";
							newEvent.Notes = "This is your motivational event to go and do 30 minutes of exercise. Super important. Do this.";

							newEvent.Calendar = App.Current.EventStore.DefaultCalendarForNewEvents;

							App.Current.EventStore.SaveEvent ( newEvent, EKSpan.ThisEvent, out e );
						}
						else
							new UIAlertView ( "Access Denied", 
								"User Denied Access to Calendar Data", null,
								"ok", null).Show ();
					} );
				string[] msg = VDateToDateTime (result.Text);
				string inputTitle = msg [0];
				string inputLocation = msg [1];
				string inputDStart = msg [2];
				string inputDEnd = msg [3];
			}

			else
				testmsg = "Scanning Canceled!";

			this.InvokeOnMainThread (() => {
				var av = new UIAlertView ("Barcode Result", testmsg, null, "OK", null);
				av.Show ();
			});
		}

		public static NSDate DateTimeToNSDate(DateTime date)
		{
			DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(
				new DateTime(2001, 1, 1, 0, 0, 0) );
			return NSDate.FromTimeIntervalSinceReferenceDate(
				(date - reference).TotalSeconds);
		}

		public static DateTime IcalToDateTime(string strDate){
			string format;
			DateTime resultDate;
			CultureInfo provider = CultureInfo.InvariantCulture;
			format = "yyyyMMddThhmmssssZ";
			resultDate = DateTime.ParseExact(strDate, format, provider);
		}

		public string [] VDateToDateTime(string ical){
			char[] delim = { '\n' };
			string[] lines = ical.Split(delim);
			string[] eventData = new string[4];
			delim[0] = ':';
			for (int i = 0; i < lines.Length; i++) {
				if (lines [i].Contains ("BEGIN:VEVENT")) {
					
					for (int j = 0; j < 4; j++)
					{
					string[] line = lines [i + j + 1].Split (delim);
					string temp = "";
					for (int k = 1; k < line.Length; k++) {
						if (k < line.Length - 1)
							temp += line [k] + ":";
						else
							temp += line [k];
					}
					eventData [j] = temp;
				}
			}
//						string strDate = eventData[0].ToString();
//						strDate = strDate.Replace("\r", "");
//
//						string format;
//						DateTime result;
//						CultureInfo provider = CultureInfo.InvariantCulture;
//						format = "yyyyMMddThhmmssssZ";
//						result = DateTime.ParseExact(strDate, format, provider);
//						i += 10;
//						return result;
			
			}
			return eventData;
//			for(int i =0; i< eventData.Length; i++)
//			{
//				Console.WriteLine (eventData[i].ToString ());
//			}
		}
			
	}
}

	
