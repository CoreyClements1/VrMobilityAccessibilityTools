#if VERAFile_testf
using UnityEngine;
using System;

public static class VERAFile_testf
{
	
	private const string fileName = "testf";
	
	public static void CreateCsvEntry(int eventId, int testc, string testh, string testf)
	{
		VERALogger.Instance.CreateCsvEntry(fileName, eventId, testc, testh, testf);
	}
	
	public static void SubmitCsvFile(bool flushOnSubmit = false)
	{
		VERALogger.Instance.SubmitCsvFile(fileName, flushOnSubmit);
	}
}
#endif
