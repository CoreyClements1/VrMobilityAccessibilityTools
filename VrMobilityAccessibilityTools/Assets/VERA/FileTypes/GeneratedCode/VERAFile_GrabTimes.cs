#if VERAFile_GrabTimes
using UnityEngine;
using System;

public static class VERAFile_GrabTimes
{
	
	private const string fileName = "GrabTimes";
	
	public static void CreateCsvEntry(int eventId, string ItemName, string GrabType)
	{
		VERALogger.Instance.CreateCsvEntry(fileName, eventId, ItemName, GrabType);
	}
	
	public static void SubmitCsvFile(bool flushOnSubmit = false)
	{
		VERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);
	}
}
#endif
