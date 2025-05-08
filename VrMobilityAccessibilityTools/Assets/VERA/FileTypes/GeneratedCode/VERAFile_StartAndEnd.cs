#if VERAFile_StartAndEnd
using UnityEngine;
using System;

public static class VERAFile_StartAndEnd
{
	
	private const string fileName = "StartAndEnd";
	
	public static void CreateCsvEntry(int eventId)
	{
		VERALogger.Instance.CreateCsvEntry(fileName, eventId);
	}
	
	public static void SubmitCsvFile(bool flushOnSubmit = false)
	{
		VERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);
	}
}
#endif
