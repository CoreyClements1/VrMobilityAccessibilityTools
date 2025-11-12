#if VERAFile_SpecialEvents
using UnityEngine;
using System;

namespace VERA
{
	
	/// <summary>
	/// Static class for recording new entries to the SpecialEvents CSV file.
	/// <br/><br/>This class has been generated based on the CSV file you have defined on the VERA portal.
	/// This class should be the only way you record new CSV entries to the SpecialEvents file.
	/// <br/><br/>Notably, use the CreateCsvEntry() method to create new entries in the SpecialEvents CSV log file.
	/// </summary>
	public static class VERAFile_SpecialEvents
	{
		
		private const string fileName = "SpecialEvents";
		
		/// <summary>
		/// Creates a new row entry in the SpecialEvents CSV log file.
		/// This CSV entry will automatically have the following fields populated:
		/// <list type="bullet">
		/// <item><description>pID (Participant ID)</description></item>
		/// <item><description>TS (Timestamp in milliseconds since application start)</description></item>
		/// <item><description>Conditions (Experimental conditions the participant was under during this log, in JSON format)</description></item>
		/// </list>
		/// This function has been set up according to your configuration and preferences for this file type on the VERA portal.
		/// Included in your configuration are the following additional columns:
		/// <list type="bullet">
		/// <item>eventId: An identifier for this log entry, of type int. Mandatory for each user-generated file type, but may be arbitrarily assigned according to your preferences.</item>
		/// <item>EventName: Value for the 'EventName' column, of type string.</item>
		/// <item>AssociatedObject: Value for the 'AssociatedObject' column, of type string.</item>
		/// <item>AssociatedTransform: Value for the 'AssociatedTransform' column, of type Transform.</item>
		/// </list>
		/// </summary>
		/// <param name="eventId">eventId: An identifier for this log entry, of type int. Mandatory for each user-generated file type, but may be arbitrarily assigned according to your preferences.</param>
		/// <param name="EventName">EventName: Value for the 'EventName' column, of type string.</param>
		/// <param name="AssociatedObject">AssociatedObject: Value for the 'AssociatedObject' column, of type string.</param>
		/// <param name="AssociatedTransform">AssociatedTransform: Value for the 'AssociatedTransform' column, of type Transform.</param>
		public static void CreateCsvEntry(int eventId, string EventName, string AssociatedObject, Transform AssociatedTransform)
		{
			VERASessionManager.CreateArbitraryCsvEntry(fileName, eventId, EventName, AssociatedObject, AssociatedTransform	);
		}
		
	}
}
#endif
