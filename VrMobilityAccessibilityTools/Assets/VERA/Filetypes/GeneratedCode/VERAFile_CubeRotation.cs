#if VERAFile_CubeRotation
using UnityEngine;
using System;

namespace VERA
{
	
	/// <summary>
	/// Static class for recording new entries to the CubeRotation CSV file.
	/// <br/><br/>This class has been generated based on the CSV file you have defined on the VERA portal.
	/// This class should be the only way you record new CSV entries to the CubeRotation file.
	/// <br/><br/>Notably, use the CreateCsvEntry() method to create new entries in the CubeRotation CSV log file.
	/// </summary>
	public static class VERAFile_CubeRotation
	{
		
		private const string fileName = "CubeRotation";
		
		/// <summary>
		/// Creates a new row entry in the CubeRotation CSV log file.
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
		/// <item>Message: Value for the 'Message' column, of type string.</item>
		/// <item>Rotation: Value for the 'Rotation' column, of type string.</item>
		/// </list>
		/// </summary>
		/// <param name="eventId">eventId: An identifier for this log entry, of type int. Mandatory for each user-generated file type, but may be arbitrarily assigned according to your preferences.</param>
		/// <param name="Message">Message: Value for the 'Message' column, of type string.</param>
		/// <param name="Rotation">Rotation: Value for the 'Rotation' column, of type string.</param>
		public static void CreateCsvEntry(int eventId, string Message, string Rotation)
		{
			VERASessionManager.CreateArbitraryCsvEntry(fileName, eventId, Message, Rotation	);
		}
		
	}
}
#endif
