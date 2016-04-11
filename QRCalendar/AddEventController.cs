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
	public class AddEventController
	{
		public AddEventController ()
		{
		}

		public static void Add(int actionResult, DateTime inputDStart, DateTime inputDEnd, string inputTitle, string inputLocation)
		{
			App.Current.EventStore.RequestAccess (EKEntityType.Event, 
				(bool granted, NSError e) => {
					if (granted)
					{
						EKEvent newEvent = EKEvent.FromStore ( App.Current.EventStore );
						// make the event start 20 minutes from now and last 30 minutes
						newEvent.StartDate = DateTimeToNSDate(inputDStart);
						newEvent.EndDate = DateTimeToNSDate(inputDEnd);
						newEvent.Title = inputTitle;
						newEvent.Location = inputLocation;

						if(actionResult == 0)
						{
						newEvent.Calendar = App.Current.EventStore.DefaultCalendarForNewEvents;
						App.Current.EventStore.SaveEvent ( newEvent, EKSpan.ThisEvent, out e );
						}

						else if(actionResult==1)
						{
							
						}

						else if(actionResult==2){
							newEvent.Calendar = App.Current.EventStore.DefaultCalendarForNewEvents;
							App.Current.EventStore.SaveEvent ( newEvent, EKSpan.ThisEvent, out e );
						}
						new UIAlertView ("QR Code Scan Results:", "Successfully Added event!", null, "ok", null).Show ();
						}
					else
					{
						new UIAlertView ( "Access Denied", "User Denied Access to Calendar Data", null, "ok", null).Show ();
					}
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

			}
			return eventData;
		}
	
	}
}

