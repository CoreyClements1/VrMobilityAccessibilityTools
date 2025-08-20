#if VERAFile_Interactions
using UnityEngine;
using System;

public static class VERAFile_Interactions
{
	
	private const string fileName = "Interactions";
	
	public static void CreateCsvEntry(int eventId, string InteractionType, string Interactable, Transform Transform)
	{
		VERALogger.Instance.CreateCsvEntry(fileName, eventId, InteractionType, Interactable, Transform);
	}
	
	public static void SubmitCsvFile(bool flushOnSubmit = false)
	{
		VERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);
	}
}
#endif
