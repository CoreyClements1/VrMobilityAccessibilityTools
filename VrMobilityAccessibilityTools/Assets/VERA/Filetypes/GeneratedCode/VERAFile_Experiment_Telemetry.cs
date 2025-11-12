#if VERAFile_Experiment_Telemetry
using UnityEngine;
using System;

namespace VERA
{
	
	/// <summary>
	/// Static class for recording new entries to the Experiment_Telemetry CSV file.
	/// <br/><br/>This class has been generated based on the CSV file you have defined on the VERA portal.
	/// This class should be the only way you record new CSV entries to the Experiment_Telemetry file.
	/// <br/><br/>Notably, use the CreateCsvEntry() method to create new entries in the Experiment_Telemetry CSV log file.
	/// </summary>
	public static class VERAFile_Experiment_Telemetry
	{
		
		private const string fileName = "Experiment_Telemetry";
		
		/// <summary>
		/// Creates a new row entry in the Experiment_Telemetry CSV log file.
		/// This file is automatically populated and handled by VERA; researchers should NOT need to call this function directly.
		public static void CreateCsvEntry(int sampleIndex, int headset_detected, float Headset_Pos_X, float Headset_Pos_Y, float Headset_Pos_Z, string headset_rot, int left_detected, float LeftController_Pos_X, float LeftController_Pos_Y, float LeftController_Pos_Z, string left_rot, float left_trigger, float left_grip, int left_primaryButton, int left_secondaryButton, int left_primary2DAxisClick, int right_detected, float RightController_Pos_X, float RightController_Pos_Y, float RightController_Pos_Z, string right_rot, float right_trigger, float right_grip, int right_primaryButton, int right_secondaryButton, int right_primary2DAxisClick)
		{
			VERASessionManager.CreateArbitraryCsvEntry(fileName, sampleIndex, headset_detected, Headset_Pos_X, Headset_Pos_Y, Headset_Pos_Z, headset_rot, left_detected, LeftController_Pos_X, LeftController_Pos_Y, LeftController_Pos_Z, left_rot, left_trigger, left_grip, left_primaryButton, left_secondaryButton, left_primary2DAxisClick, right_detected, RightController_Pos_X, RightController_Pos_Y, RightController_Pos_Z, right_rot, right_trigger, right_grip, right_primaryButton, right_secondaryButton, right_primary2DAxisClick	);
		}
		
	}
}
#endif
