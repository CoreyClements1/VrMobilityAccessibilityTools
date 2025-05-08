#if VERAFile_PlayerTransforms
using UnityEngine;
using System;

public static class VERAFile_PlayerTransforms
{
	
	private const string fileName = "PlayerTransforms";
	
	public static void CreateCsvEntry(int eventId, Transform Transform)
	{
		VERALogger.Instance.CreateCsvEntry(fileName, eventId, Transform);
	}
	
	public static void SubmitCsvFile(bool flushOnSubmit = false)
	{
		VERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);
	}
}
#endif
