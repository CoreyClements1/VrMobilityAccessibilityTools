#if VERAFile_CleanTimes
using UnityEngine;
using System;

public static class VERAFile_CleanTimes
{
	
	private const string fileName = "CleanTimes";
	
	public static void CreateCsvEntry(int eventId, string ItemType, Transform Transform)
	{
		VERALogger.Instance.CreateCsvEntry(fileName, eventId, ItemType, Transform);
	}
	
	public static void SubmitCsvFile(bool flushOnSubmit = false)
	{
		VERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);
	}
}
#endif
