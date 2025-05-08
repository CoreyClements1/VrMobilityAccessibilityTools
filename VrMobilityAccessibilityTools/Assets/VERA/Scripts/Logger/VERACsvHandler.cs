using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class VERACsvHandler : MonoBehaviour
{

    // VERACsvHandler handles the entry logging for a single defined CSV file type

    public string filePath { get; private set; } // Where the CSV file is recorded and stored (.../file.csv)
    public VERAColumnDefinition columnDefinition { get; private set; } // The column definition of this CSV
    public UnityWebRequest activeWebRequest { get; private set; }

    // Unwritten entries and flushing
    private List<string> unwrittenEntries = new List<string>(); // A cache of unwritten log entries
    private int unwrittenEntryLimit = 100; // If unwritten entries exceeds this limit, a flush will occur
    private float timeSinceLastFlush = 0f;
    private float flushInterval = 5f; // How frequently a flush of unwritten entries will occur

    // Periodic sync
    private string syncMetaFilePath; // Where the partial sync info is recorded and stored (.../file.syncmeta)


    #region MONOBEHAVIOUR


    // Update, check if we need to flush unwritten entries
    private void Update()
    {
        timeSinceLastFlush += Time.deltaTime;
        CheckFlushUnwritten();
    }


    // OnDestroy, flush any unwritten entries
    private void OnDestroy()
    {
        FlushUnwrittenEntries();
    }


    // OnApplicationQuit, flush any unwritten entries
    private void OnApplicationQuit()
    {
        FlushUnwrittenEntries();
    }


    #endregion


    #region INIT


    public void Initialize(VERAColumnDefinition columnDef)
    {
        string participantUUID = VERALogger.Instance.activeParticipant.participantUUID;
        columnDefinition = columnDef;

        filePath = VERALogger.Instance.baseFilePath + "-" + participantUUID + "-" + columnDefinition.fileType.fileTypeId + ".csv";
        syncMetaFilePath = Path.Combine(VERALogger.Instance.dataPath, Path.GetFileNameWithoutExtension(filePath) + ".syncmeta");

        // Set up the column names for the header row
        List<string> columnNames = new List<string>();
        foreach (VERAColumnDefinition.Column column in columnDefinition.columns)
        {
            columnNames.Add(column.name);
        }

        // Write the initial file using StreamWriter
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(string.Join(",", columnNames));
            writer.Flush();
            Debug.Log("[VERA Logger] CSV File for file type \"" + columnDefinition.fileType.name + ".csv\" created and saved at " + filePath);
        }
    }


    #endregion


    #region ENTRY LOGGING


    // Logs an entry to the file. Doesn't write yet, only writes on flush.
    public void CreateEntry(int eventId, params object[] values)
    {
        if (values.Length != columnDefinition.columns.Count - 2)
        {
            Debug.LogError("[VERA Logger]: You are attempting to create a log entry with " + (values.Length + 2).ToString() +
              " columns. The file type \"" + columnDefinition.fileType.name + "\" expects " + columnDefinition.columns.Count + 
              " columns. Cannot log entry as desired.");
            return;
        }

        List<string> entry = new List<string>();
        // Add timestamp first.
        entry.Add(DateTime.UtcNow.ToString("o"));
        entry.Add(Convert.ToString(eventId));

        for (int i = 0; i < values.Length; i++)
        {
            object value = values[i];
            VERAColumnDefinition.Column column = columnDefinition.columns[i + 2];

            string formattedValue = "";
            switch (column.type)
            {
                case VERAColumnDefinition.DataType.Number:
                    formattedValue = Convert.ToString(value);
                    break;
                case VERAColumnDefinition.DataType.String:
                    formattedValue = FormatValueForCsv(value.ToString());
                    break;
                case VERAColumnDefinition.DataType.JSON:
                    var json = JsonConvert.SerializeObject(value);
                    formattedValue = FormatValueForCsv(json);
                    break;
                case VERAColumnDefinition.DataType.Transform:
                    var transform = value as Transform;
                    if (transform != null)
                    {
                        var settings = new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        };
                        formattedValue = FormatValueForCsv(JsonConvert.SerializeObject(new
                        {
                            position = new { x = transform.position.x, y = transform.position.y, z = transform.position.z },
                            rotation = new { x = transform.rotation.x, y = transform.rotation.y, z = transform.rotation.z, w = transform.rotation.w },
                            localScale = new { x = transform.localScale.x, y = transform.localScale.y, z = transform.localScale.z }
                        }, settings));
                    }
                    else
                    {
                        var newObject = value;
                        if (newObject != null)
                        {
                            formattedValue = FormatValueForCsv(JsonConvert.SerializeObject(newObject));
                        }
                    }
                    break;
                default:
                    formattedValue = FormatValueForCsv(value.ToString());
                    break;
            }

            entry.Add(formattedValue);
        }

        unwrittenEntries.Add(string.Join(",", entry));
        CheckFlushUnwritten();
    }

    // Formats given value for CSV, and returns
    private string FormatValueForCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        value = value.Replace("\"", "\"\"");

        // If it contains a quote, wrap in quotes
        if (value.Contains(",") || value.Contains("\n") || value.Contains("\""))
        {
            return $"\"{value}\"";
        }
        else
        {
            return value;
        }
    }


    #endregion


    #region FLUSH


    // Checks if we need to flush unwritten entries; flushes if we do need to.
    private void CheckFlushUnwritten()
    {
        // If the number of unwritten entries is too large or enough time has passed since last flush, flush
        if (unwrittenEntries.Count >= unwrittenEntryLimit || timeSinceLastFlush >= flushInterval)
        {
            FlushUnwrittenEntries();
        }
    }

    // Flushes all unwritten entries and writes them to the file
    public void FlushUnwrittenEntries()
    {
        timeSinceLastFlush = 0f;

        if (unwrittenEntries.Count == 0)
            return;

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            foreach (string entry in unwrittenEntries)
            {
                writer.WriteLine(entry);
            }
        }

        unwrittenEntries.Clear();
    }


    #endregion


    #region SUBMISSION

public IEnumerator SubmitFileWithRetry(bool finalUpload = false)
    {
        int maxAttempts = 5;
        float delay = 2f;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            // 1) If experiment finalized but this isn't finalUpload, skip
            if (VERALogger.Instance.sessionFinalized && !finalUpload)
            {
                Debug.Log($"[VERA CSV] Session finalized; skipping partial upload for {columnDefinition.fileType.name}.");
                yield break;
            }

            // 2) Try upload
            yield return StartCoroutine(SubmitFileCoroutine(finalUpload));

            // 3) Check result of last UnityWebRequest
            var req = activeWebRequest;
            if (req != null && req.result == UnityWebRequest.Result.Success)
            {
                // success → exit
                yield break;
            }
            else
            {
                // failure → log and back off
                string err = req != null ? req.error : "unknown error";
                Debug.LogWarning($"[VERA CSV] Attempt {attempt} failed for {columnDefinition.fileType.name}: {err}");
                if (attempt < maxAttempts)
                {
                    Debug.Log($"[VERA CSV] Retrying in {delay}s...");
                    yield return new WaitForSeconds(delay);
                    delay *= 2f; // exponential backoff
                }
                else
                {
                    Debug.LogError($"[VERA CSV] All {maxAttempts} attempts failed for {columnDefinition.fileType.name}.");
                }
            }
        }
    }

    // Submits the file to the server
    public IEnumerator SubmitFileCoroutine(bool finalUpload = false)
    {
        Debug.Log("[VERA Logger] Submitting file associated with file type \"" + 
            columnDefinition.fileType.name + "\" (" + filePath + ")");

        // Paths, keys, and IDs
        string basename = Path.GetFileName(filePath);
        string apiKey = VERALogger.Instance.apiKey;
        string experimentUUID = VERALogger.Instance.experimentUUID;
        string siteUUID = VERALogger.Instance.siteUUID;
        string participantUUID = VERALogger.Instance.activeParticipant.participantUUID;
        string fileTypeId = columnDefinition.fileType.fileTypeId;

        string host = VERAHost.hostUrl;
        string url = $"{host}/api/participants/{participantUUID}/filetypes/{fileTypeId}/files";
        byte[] fileData = null;

        // Read the data associated with the file
        yield return VERALogger.Instance.ReadBinaryDataFile(filePath, (result) => fileData = result);
        if (fileData == null)
        {
            Debug.Log("[VERA Logger] Attempted to upload CSV for file type \"" + columnDefinition.fileType.name + 
                "\", but there is no file data to submit.");
            yield break;
        }

        // Set up the request
        WWWForm form = new WWWForm();
        form.AddField("participant_UUID", participantUUID);
        form.AddBinaryData("fileUpload", fileData, experimentUUID + "-" + siteUUID + "-" + participantUUID + "-" + fileTypeId + ".csv", "text/csv");

        // Send the request
        activeWebRequest = UnityWebRequest.Post(url, form);
        activeWebRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
        yield return activeWebRequest.SendWebRequest();

        // Check success
        if (activeWebRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[VERA Logger] Successfully uploaded file \"" + columnDefinition.fileType.name + "\".");
            if (finalUpload)
                VERALogger.Instance.OnCsvFullyUploaded(filePath);
        }
        else
        {
            Debug.LogError("[VERA Logger] Failed to upload file \"" + columnDefinition.fileType.name + "\".");
        }
    }


    #endregion


    #region PERIODIC SYNC


    // Gets the last synced line index in this CSV file
    public int GetLastSyncedLineIndex()
    {
        if (!File.Exists(filePath)) return 0;
        string content = File.ReadAllText(syncMetaFilePath);
        return int.TryParse(content, out int index) ? index : 0;
    }

    public DateTime GetLastSyncedTimestamp()
    {
        if (!File.Exists(syncMetaFilePath)) return DateTime.MinValue;
        string content = File.ReadAllText(syncMetaFilePath);
        return DateTime.TryParse(content, out DateTime ts) ? ts : DateTime.MinValue;
    }

    public void UpdateLastSyncedTimestamp(DateTime ts)
    {
        File.WriteAllText(syncMetaFilePath, ts.ToString("o"));
    }


    #endregion


}
