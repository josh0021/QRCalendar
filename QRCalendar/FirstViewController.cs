using System;
using System.Globalization;
using ZXing;
using UIKit;
using ZXing.Mobile;
using EventKit;
using EventKitUI;
using Foundation;
using System.Threading.Tasks;

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
			this.TabBarItem.Image = UIImage.FromFile ("qr.png");
			this.Title = "Scan";
		}


		override async public void ViewDidAppear (bool animated)
		{
			this.TabBarController.SelectedIndex =1;
			if (ParentViewController != null) 
			{
				CustomOverlayView customOverlay = new CustomOverlayView();
				var scanner = new ZXing.Mobile.MobileBarcodeScanner (this);

				customOverlay.ButtonCancel.TouchUpInside += delegate {
					Console.WriteLine("cancelpresssed");
					this.NavigationController.TabBarController.SelectedIndex = 0;
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
	
	   void HandleScanResult (ZXing.Result result)
		{
			
			string testmsg = " ";
			if (result != null && !string.IsNullOrEmpty (result.Text)) {
				string[] msg = VDateToDateTime (result.Text);
				string inputTitle = (string.IsNullOrEmpty (msg [0])) ? ("") : msg [0];
				string inputLocation = (string.IsNullOrEmpty (msg [1])) ? ("") : msg [1];
				DateTime inputDStart = IcalToDateTime (msg [2]);
				DateTime inputDEnd = IcalToDateTime (msg [3]);


				// Create a new Alert Controller
				UIAlertController actionSheetAlert = UIAlertController.Create ("Action Sheet", "Select an event from below", UIAlertControllerStyle.ActionSheet);

				// Add Actions
				actionSheetAlert.AddAction (UIAlertAction.Create ("Add To Calendar", UIAlertActionStyle.Default, (action) => Add (0, inputDStart, inputDEnd, inputTitle, inputLocation)));
				actionSheetAlert.AddAction (UIAlertAction.Create ("Add To Reminders", UIAlertActionStyle.Default, (action) => Add (1, inputDStart, inputDEnd, inputTitle, inputLocation)));
				actionSheetAlert.AddAction (UIAlertAction.Create ("Add To Both", UIAlertActionStyle.Default, (action) => Add (2, inputDStart, inputDEnd, inputTitle, inputLocation)));
				actionSheetAlert.AddAction (UIAlertAction.Create ("Cancel", UIAlertActionStyle.Cancel, (action) => Console.WriteLine ("Cancel button pressed.")));

				// Xamarin code
				// Required for iPad - You must specify a source for the Action Sheet since it is
				// displayed as a popover
				UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
				if (presentationPopover != null) {
					presentationPopover.SourceView = this.View;
					presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
				}

				// Display the alert
				this.PresentViewController (actionSheetAlert, true, null);
			} 
			else
				new UIAlertView ("QR Code Scan Results:", "Scanning Canceled!", null, "ok", null).Show ();
		}

		public void Add(int actionResult, DateTime inputDStart, DateTime inputDEnd, string inputTitle, string inputLocation)
		{
			App.Current.EventStore.RequestAccess (EKEntityType.Event, 
				(bool granted, NSError e) => {
					if (granted) {
						EKEvent newEvent = EKEvent.FromStore (App.Current.EventStore);
						// make the event start 20 minutes from now and last 30 minutes
						newEvent.StartDate = DateTimeToNSDate (inputDStart);
						newEvent.EndDate = DateTimeToNSDate (inputDEnd);
						newEvent.Title = inputTitle;
						newEvent.Location = inputLocation;

						EKReminder reminder = EKReminder.Create (App.Current.EventStore);
						reminder.Title = "Do something awesome!";
						reminder.Calendar = App.Current.EventStore.DefaultCalendarForNewReminders;

						if (actionResult == 0) {
							newEvent.Calendar = App.Current.EventStore.DefaultCalendarForNewEvents;
							App.Current.EventStore.SaveEvent (newEvent, EKSpan.ThisEvent, out e);
						} else if (actionResult == 1) {
							NSError ee;
							App.Current.EventStore.SaveReminder (reminder, true, out ee);
						} else if (actionResult == 2) {
							newEvent.Calendar = App.Current.EventStore.DefaultCalendarForNewEvents;
							App.Current.EventStore.SaveEvent (newEvent, EKSpan.ThisEvent, out e);
						}
						this.InvokeOnMainThread (() => {
							var av = new UIAlertView ("QR Code Scan Results:", "Successfully Added event!", null, "ok", null);
							av.Show ();
						});
					} else {
						this.InvokeOnMainThread (() => {
							var av = new UIAlertView ("Access Denied", "User Denied Access to Calendar Data", null, "ok", null);
							av.Show ();
						});
					}
				});
		}
				
//				// Display the alert
//				this.PresentViewController(actionSheetAlert,true,null);
//				App.Current.EventStore.RequestAccess (EKEntityType.Event, 
//					(bool granted, NSError e) => {
//						if (granted)
//						{
//							EKEvent newEvent = EKEvent.FromStore ( App.Current.EventStore );
//							// make the event start 20 minutes from now and last 30 minutes
//							newEvent.StartDate = DateTimeToNSDate(inputDStart);
//							newEvent.EndDate = DateTimeToNSDate(inputDEnd);
//							newEvent.Title = inputTitle;
//							newEvent.Location = inputLocation;
//
//							newEvent.Calendar = App.Current.EventStore.DefaultCalendarForNewEvents;
//
//							App.Current.EventStore.SaveEvent ( newEvent, EKSpan.ThisEvent, out e );
//						}
//						else
//							new UIAlertView ( "Access Denied", 
//								"User Denied Access to Calendar Data", null,
//								"ok", null).Show ();
//					} );
//				testmsg = "Successfully Added event!";
//			}
//
//			else
//				testmsg = "Scanning Canceled!";
//
//			this.InvokeOnMainThread (() => {
//				var av = new UIAlertView ("QR Code Scan Results:", testmsg, null, "OK", null);
//				av.Show ();
//			});
//		}
//
		public static NSDate DateTimeToNSDate(DateTime date)
		{
			DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(
				new DateTime(2001, 1, 1, 0, 0, 0) );
			return NSDate.FromTimeIntervalSinceReferenceDate(
				(date - reference).TotalSeconds);
		}

		public static DateTime IcalToDateTime(string strDate){
			strDate = strDate.Replace("\r", "");
			Console.WriteLine (strDate);
			string format;
			DateTime resultDate;
			CultureInfo provider = CultureInfo.InvariantCulture;
			format = "yyyyMMddTHHmmssssZ";
			resultDate = DateTime.ParseExact(strDate, format, provider);
			Console.WriteLine (resultDate.ToString ());
			Console.WriteLine ("test");
			return resultDate;
		}

		public string [] VDateToDateTime(string ical){
			char[] delim = { '\n' };
			string[] lines = ical.Split(delim);
			string[] eventData = new string[4];
			delim[0] = ':';
			for (int i = 0; i < lines.Length; i++) {
				if (lines [i].Contains ("BEGIN:VEVENT")) {
					
					for (int j = 0; j < 4; j++) {
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
			}
			return eventData;
		}
	}
}
////			for(int i =0; i< eventData.Length; i++)
////			{
////				Console.WriteLine (eventData[i].ToString ());
////			}
//		}
//			
//	}
