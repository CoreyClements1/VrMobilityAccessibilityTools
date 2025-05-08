#if VERAFile_CubeTelemetry
using UnityEngine;
using System;

public static class VERAFile_CubeTelemetry
{
	
	private const string fileName = "CubeTelemetry";
	
	public static void CreateCsvEntry(int eventId, string Message, Transform Transform)
	{
		VERALogger.Instance.CreateCsvEntry(fileName, eventId, Message, Transform);
	}
	
	public static void SubmitCsvFile(bool flushOnSubmit = false)
	{
		VERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);
	}
}
#endif
