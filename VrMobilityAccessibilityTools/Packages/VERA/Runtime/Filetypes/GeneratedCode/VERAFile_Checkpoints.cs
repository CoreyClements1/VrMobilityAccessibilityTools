#if VERAFile_Checkpoints
using UnityEngine;
using System;

public static class VERAFile_Checkpoints
{
	
	private const string fileName = "Checkpoints";
	
	public static void CreateCsvEntry(int eventId, string Checkpoint)
	{
		VERALogger.Instance.CreateCsvEntry(fileName, eventId, Checkpoint);
	}
	
	public static void SubmitCsvFile(bool flushOnSubmit = false)
	{
		VERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);
	}
}
#endif
