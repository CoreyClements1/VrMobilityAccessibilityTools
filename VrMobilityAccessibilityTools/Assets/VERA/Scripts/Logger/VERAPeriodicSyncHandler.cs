using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class VERAPeriodicSyncHandler : MonoBehaviour
{

    // VERAPeriodicSyncHandler will periodically sync all participant data to the site


    private Coroutine periodicSyncRoutineHandle;
    private float periodicSyncInterval = 3f;
    private bool isFinalSynced = false;
    private bool syncStopped = false;

    private float baseInterval = 2f;      // initial polling interval
    private float maxInterval = 200f;     // cap exponential backoff

    private float currentInterval;
    private int failureCount = 0;
    private Coroutine syncRoutine;
    #region SYNC MANAGEMENT


    // Starts the periodic sync of data
    public void StartPeriodicSync()
    {
        periodicSyncRoutineHandle = StartCoroutine(PeriodicSyncRoutine());
    }

    private IEnumerator PeriodicSyncRoutine()
    {
        while (true)
        {
            // 1) Check network
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("[VERA Sync] Network unreachable; backing off.");
                yield return new WaitForSeconds(currentInterval);
                Backoff();
                continue;
            }

            // 2) Check experiment state
            if (VERALogger.Instance.sessionFinalized)
            {
                Debug.Log("[VERA Sync] Session finalized; stopping periodic sync.");
                yield break;
            }

            // 3) Attempt upload
            Debug.Log($"[VERA Sync] Attempting sync (interval={currentInterval}s) at {DateTime.Now}");
            yield return StartCoroutine(UploadAllPending());

            // 4) Wait and reset backoff on success
            yield return new WaitForSeconds(currentInterval);
            ResetBackoff();
        }
    }

     private void Backoff()
    {
        failureCount++;
        currentInterval = Mathf.Min(baseInterval * Mathf.Pow(2, failureCount), maxInterval);
    }

    private void ResetBackoff()
    {
        failureCount = 0;
        currentInterval = baseInterval;
    }

    public IEnumerator UploadAllPending()
    {
        // Upload all CSV handlers
        foreach (var csvHandler in VERALogger.Instance.csvHandlers)
        {
            // Only upload partial if still collecting, or final if sessionFinalized
            bool final = VERALogger.Instance.sessionFinalized;
            yield return StartCoroutine(csvHandler.SubmitFileWithRetry(final));
        }
    }

    // Periodically syncs data in an interval
    private IEnumerator PeriodicCSVSyncRoutine()
    {
        while (!syncStopped)
        {
            Debug.Log($"[VERA Sync] Periodic sync triggered at {DateTime.Now}");
            yield return new WaitForSeconds(periodicSyncInterval);
            if (!syncStopped)
                UploadUnsyncedCSVPortions();
        }
    }


    #endregion


    #region UPLOAD UNSYCNED


    // Uploads the unsynced portions of all CSVs
    private void UploadUnsyncedCSVPortions(bool isFinal = false)
    {
        if (VERALogger.Instance.sessionFinalized || VERALogger.Instance.activeParticipant.IsInFinalizedState())
        {
            Debug.LogWarning("[VERA Sync] Experiment has concluded or participant is in a conclusive state, skipping CSV sync.");
            return;
        }

        string participantUUID = VERALogger.Instance.activeParticipant.participantUUID;

        // Perform sync for each CSV
        foreach (VERACsvHandler csvHandler in VERALogger.Instance.csvHandlers)
        {
            string fileTypeName = csvHandler.columnDefinition.fileType.name;
            try
            {
                string fileTypeId = csvHandler.columnDefinition.fileType.fileTypeId;
                string[] allLines = File.ReadAllLines(csvHandler.filePath);

                if (allLines.Length <= 1)
                {
                    Debug.Log("[VERA Sync] No new data found (<=1 lines) for file \"" + fileTypeName + "\" . Skipping sync for this file.");
                    continue;
                }

                DateTime lastSyncTime = csvHandler.GetLastSyncedTimestamp();
                var header = allLines[0];
                var newLines = allLines.Skip(1).Where(line => DateTime.TryParse(line.Split(',')[0], out DateTime ts) && ts > lastSyncTime).ToList();
                if (newLines.Count == 0 || newLines.All(l => DateTime.Parse(l.Split(',')[0]) <= lastSyncTime))
                {
                    if (isFinal)
                    {
                        Debug.Log("[VERA Sync] No new data, but submitting final empty file to mark session complete for file \"" + fileTypeName + "\".");
                        string tempEmptyFilePath = Path.Combine(VERALogger.Instance.dataPath, $"final_unsynced_{participantUUID}_empty.csv");
                        using (var writer = new StreamWriter(tempEmptyFilePath))
                        {
                            writer.WriteLine(allLines[0]); // only header
                        }
                        StartCoroutine(SubmitFinalCSVCoroutine(tempEmptyFilePath, lastSyncTime,csvHandler));
                    }
                    else
                    {
                        Debug.Log("[VERA Sync] No new entries after last sync. Skipping partial sync for file \"" + fileTypeName + "\".");
                    }
                    continue;
                }
                string tempPartialFilePath = Path.Combine(VERALogger.Instance.dataPath, isFinal ? 
                    $"final_unsynced_{participantUUID}_{fileTypeId}.csv" : 
                    $"partial_{participantUUID}_{fileTypeId}_latest.csv");

                using (var writer = new StreamWriter(tempPartialFilePath))
                {
                    writer.WriteLine(header);
                    foreach (var line in newLines) writer.WriteLine(line);
                }

                DateTime latestTimestamp = newLines.Select(line => DateTime.Parse(line.Split(',')[0])).Max();

                if (isFinal)
                {
                    StartCoroutine(SubmitFinalCSVCoroutine(tempPartialFilePath, latestTimestamp,csvHandler));
                }
                else
                {
                    StartCoroutine(SubmitPartialCSVCoroutine(tempPartialFilePath,csvHandler , latestTimestamp));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[VERA Sync] CSV Sync Failed for file \"" + fileTypeName + "\": " + e.Message);
            }
        }
    }


    // Submits a partial CSV to the given file type ID for the current participant
    private IEnumerator SubmitPartialCSVCoroutine(string partialFilePath, VERACsvHandler csvHandler, DateTime newLastTimestamp)
    {

        string participant_UUID = VERALogger.Instance.activeParticipant.participantUUID;
        string experiment_UUID = VERALogger.Instance.experimentUUID;
        string site_UUID = VERALogger.Instance.siteUUID;

        string host = VERAHost.hostUrl;
        string url =$"{host}/api/participants/{participant_UUID}/filetypes/{csvHandler.columnDefinition.fileType.fileTypeId}/files";
        byte[] fileData = null;
                yield return VERALogger.Instance.ReadBinaryDataFile(partialFilePath, (result) => fileData = result);

        if (fileData == null)
        {
            Debug.Log("[VERA Sync] No file data to submit.");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("experiment_UUID", experiment_UUID);
        form.AddField("participant_UUID", participant_UUID);
        form.AddField("site_UUID", site_UUID);
        form.AddBinaryData("fileUpload", fileData, experiment_UUID + "-" + site_UUID + "-" + participant_UUID + "-" + csvHandler.columnDefinition.fileType.fileTypeId + ".csv", "text/csv");


        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Authorization", "Bearer " + VERALogger.Instance.apiKey);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
          csvHandler.UpdateLastSyncedTimestamp(newLastTimestamp);
          Debug.Log("[SYNC] Partial CSV synced successfully. Updated timestamp to " + newLastTimestamp);
        }
        else
        {
          Debug.LogError("[SYNC] Partial CSV upload failed: " + request.error);
        }
    }


    #endregion


    #region FINAL SYNC



    private IEnumerator SubmitFinalCSVCoroutine(string finalFilePath, DateTime newLastTimestamp, VERACsvHandler csvHandler)
    {
       string participant_UUID = VERALogger.Instance.activeParticipant.participantUUID;
        string experiment_UUID = VERALogger.Instance.experimentUUID;
        string site_UUID = VERALogger.Instance.siteUUID;

        string host = VERAHost.hostUrl;
        string url = $"{host}/api/participants/{participant_UUID}/filetypes/{csvHandler.columnDefinition.fileType.fileTypeId}/files?final=true";

        byte[] fileData = null;
                yield return VERALogger.Instance.ReadBinaryDataFile(finalFilePath, (result) => fileData = result);

        if (fileData == null)
        {
            Debug.Log("[VERA Sync] No file data to submit.");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("experiment_UUID", experiment_UUID);
        form.AddField("participant_UUID", participant_UUID);
        form.AddField("site_UUID", site_UUID);
        form.AddBinaryData("fileUpload", fileData, experiment_UUID + "-" + site_UUID + "-" + participant_UUID + "-" + csvHandler.columnDefinition.fileType.fileTypeId + ".csv", "text/csv");


        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Authorization", "Bearer " + VERALogger.Instance.apiKey);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
          csvHandler.UpdateLastSyncedTimestamp(newLastTimestamp);
          Debug.Log("[FINAL SYNC] Final CSV synced successfully. Updated timestamp to " + newLastTimestamp);
        }
        else
        {
          Debug.LogError("[FINAL SYNC] Final CSV upload failed: " + request.error);
        }
    }


    public IEnumerator FinalSyncAndCompleteStatus()
    {
        if (isFinalSynced) yield break;
        isFinalSynced = true;

        foreach (VERACsvHandler csvHandler in VERALogger.Instance.csvHandlers)
            csvHandler.FlushUnwrittenEntries();

        yield return null;

        UploadUnsyncedCSVPortions(isFinal: true);

        yield return new WaitForSeconds(1f); // give sync time

        syncStopped = true;
        if (periodicSyncRoutineHandle != null)
        {
            StopCoroutine(periodicSyncRoutineHandle);
            Debug.Log("[VERA Sync] Stopped periodic sync coroutine after final upload.");
        }
    }


    #endregion


}
