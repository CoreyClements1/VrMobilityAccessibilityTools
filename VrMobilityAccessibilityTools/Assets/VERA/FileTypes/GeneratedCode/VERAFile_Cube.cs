#if VERAFile_Cube
using UnityEngine;
using System;

public static class VERAFile_Cube
{
	
	private const string fileName = "Cube";
	
	public static void CreateCsvEntry(int eventId, string message, Transform tr)
	{
		VERALogger.Instance.CreateCsvEntry(fileName, eventId, message, tr);
	}
	
	public static void SubmitCsvFile(bool flushOnSubmit = false)
	{
		VERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);
	}
}
#endif
