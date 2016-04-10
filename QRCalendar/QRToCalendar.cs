using System;
using System.Globalization;
namespace QRCalendar
{
	public static class CalendarFileExtentions
	{
		public static DateTime DateToDateTime(string s){
			string ical = s;
			char[] delim = { '\n' };
			string[] lines = ical.Split(delim);
			delim[0] = ':';
			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].Contains("BEGIN:VEVENT"))
				{
					string[] eventData = new string[9];
					for (int j = 0; j < 9; j++)
						eventData[j] = lines[i + j + 1].Split(delim)[1];
					string strDate = eventData[0].ToString();
					strDate = strDate.Replace("\r", "");

					string format;
					DateTime result;
					CultureInfo provider = CultureInfo.InvariantCulture;
					format = "yyyyMMddThhmmssssZ";
					result = DateTime.ParseExact(strDate, format, provider);
					i += 10;
				}
			}
		}

	}
}

