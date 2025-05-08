#if VERAFile_CubeRotation
using UnityEngine;
using System;

public static class VERAFile_CubeRotation
{
	
	private const string fileName = "CubeRotation";
	
	public static void CreateCsvEntry(int eventId, string Message, string Rotation)
	{
		VERALogger.Instance.CreateCsvEntry(fileName, eventId, Message, Rotation);
	}
	
	public static void SubmitCsvFile(bool flushOnSubmit = false)
	{
		VERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);
	}
}
#endif
