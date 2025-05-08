using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using System.Linq;
using UnityEngine.Events;
using MimeTypes;

public class VERALogger : MonoBehaviour
{

    public static VERALogger Instance;

    public VERAParticipantManager activeParticipant { get; private set; }
    private VERAPeriodicSyncHandler periodicSyncHandler;
    private VERAGenericFileHelper genericFileHelper;

    // Keys
    public string apiKey;
    public string experimentUUID;
    public string siteUUID;

    // Paths
    public string baseFilePath = "";
    public string dataPath = "";
    public string genericDataPath = "";

    // Experiment management
    public bool sessionFinalized { get; private set; } = false;
    public bool collecting = false;
    public VERACsvHandler[] csvHandlers { get; private set; }

    // Upload events and progress
    private UnityWebRequest trackedUploadRequest;
    public UnityEvent onBeginFileUpload = new UnityEvent();
    public UnityEvent onFileUploadExited = new UnityEvent();


    #region INITIALIZATION


    // Awake, initializes various components and sets up experiment
    public void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        // Set up authentication and paths
        LoadAuthentication();
        SetupKeysAndPaths();

        // Upload any existing unuploaded files
        UploadExistingUnuploadedFiles(Path.Combine(dataPath, "uploadedCSVs.txt"), dataPath, ".csv");
        UploadExistingUnuploadedFiles(Path.Combine(dataPath, "uploadedImages.txt"), dataPath, ".png");
        UploadExistingUnuploadedFiles(Path.Combine(dataPath, "uploadedGeneric.txt"), genericDataPath, "");

        collecting = true;

        // Setup any file types, CSV logging, and the generic file helper
        SetupFileTypes();
        genericFileHelper = gameObject.AddComponent<VERAGenericFileHelper>();

        // Begin the periodic sync
        periodicSyncHandler = gameObject.AddComponent<VERAPeriodicSyncHandler>();
        periodicSyncHandler.StartPeriodicSync();

        Application.wantsToQuit += WantsToQuitHandler;
    }


    // Loads authentication from streaming assets into PlayerPrefs
    private void LoadAuthentication()
    {
        TextAsset authFile = Resources.Load<TextAsset>("VERAAuthentication");
        if (authFile != null)
        {
            // Load from file
            string json = authFile.text;
            VERAAuthInfo authInfo = JsonUtility.FromJson<VERAAuthInfo>(json);

            // Set PlayerPrefs to match file
            PlayerPrefs.SetInt("VERA_Authenticated", authInfo.authenticated ? 1 : 0);
            PlayerPrefs.SetString("VERA_AuthToken", authInfo.authToken);
            PlayerPrefs.SetString("VERA_UserId", authInfo.userId);
            PlayerPrefs.SetString("VERA_UserName", authInfo.userName);
            PlayerPrefs.SetString("VERA_ActiveExperiment", authInfo.activeExperiment);
            PlayerPrefs.SetString("VERA_ActiveSite", authInfo.activeSite);
        }
        else
        {
            // File not found, authentication likely not set up yet; set default values
            PlayerPrefs.SetInt("VERA_Authenticated", 0);
            PlayerPrefs.SetString("VERA_AuthToken", String.Empty);
            PlayerPrefs.SetString("VERA_UserId", String.Empty);
            PlayerPrefs.SetString("VERA_UserName", String.Empty);
            PlayerPrefs.SetString("VERA_ActiveExperiment", String.Empty);
            PlayerPrefs.SetString("VERA_ActiveSite", String.Empty);

            // Log unauthenticated user
            Debug.LogError("[VERA Authentication] You are not authenticated, and no info will be linked to you " +
                "or your experiment. Authenticate via the Unity menu bar item, VERA -> Settings.");
        }
    }


    // Sets up the various important keys and paths relating to logging data
    private void SetupKeysAndPaths()
    {
        // Keys and IDs
        apiKey = PlayerPrefs.GetString("VERA_AuthToken");
        experimentUUID = PlayerPrefs.GetString("VERA_ActiveExperiment");
        siteUUID = PlayerPrefs.GetString("VERA_ActiveSite");

        if (string.IsNullOrEmpty(experimentUUID) || experimentUUID == "N/A")
        {
            Debug.LogError("[VERA Logger] You do not have an active experiment. In the Menu Bar at the top of " +
                "the editor, please click on the 'VERA' dropdown, then select 'Settings' to pick an active experiment.");
            return;
        }

        // Participant
        activeParticipant = gameObject.AddComponent<VERAParticipantManager>();
        activeParticipant.CreateParticipant();

        // CSV and file paths
        if (baseFilePath == "")
        {
            #if UNITY_EDITOR
            // Save to Assets/VERA/data
            baseFilePath = Path.Combine(Application.dataPath, "VERA", "data", experimentUUID + "-" + siteUUID);
            dataPath = Path.Combine(Application.dataPath, "VERA", "data");
            genericDataPath = Path.Combine(Application.dataPath, "VERA", "data", "generic");
            #else
            baseFilePath = Path.Combine(Application.persistentDataPath, experimentUUID + "-" + siteUUID);
            dataPath = Path.Combine(Application.persistentDataPath);
            genericDataPath = Path.Combine(Application.persistentDataPath, "generic");
            #endif
        }
    }


    // Tests the connection to the VERA server via simple API call; prints result.
    public IEnumerator TestConnection()
    {
        string host = VERAHost.hostUrl;
        string url = host + "/api/";

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + apiKey);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
            Debug.Log("[VERA Connection] You are connected to the VERA servers.");
        else
            Debug.LogError("[VERA Connection] Failed to connect to the VERA servers: " + www.error);
    }


    #endregion


    #region FILE TYPE / COLUMN DEFINITION MANAGEMENT


    // Sets up the file types and associated column definitions for this experiment
    private void SetupFileTypes()
    {
        // Load column definitions
        VERAColumnDefinition[] columnDefinitions = Resources.LoadAll<VERAColumnDefinition>("");
        if (columnDefinitions == null || columnDefinitions.Length == 0)
        {
            Debug.Log("[VERA Logger] No CSV file types found; continuing assuming no CSV logging will occur.");
            return;
        }

        csvHandlers = new VERACsvHandler[columnDefinitions.Length];

        // For each column definition, set it up as its own CSV handler
        for (int i = 0; i < csvHandlers.Length; i++)
        {
            csvHandlers[i] = gameObject.AddComponent<VERACsvHandler>();
            csvHandlers[i].Initialize(columnDefinitions[i]);
        }
    }


    // Finds a csv handler by provided FileType name
    // Ignores extension, returns null on failure to find
    public VERACsvHandler FindCsvHandlerByFileName(string name)
    {
        // Remove extension to check for name directly with no extension
        if (name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            name = name.Substring(0, name.Length - 4);
        }

        // Find the CSV handler that matches this name
        foreach (VERACsvHandler csvHandler in csvHandlers)
        {
            if (csvHandler.columnDefinition.fileType.name == name)
            {
                return csvHandler;
            }
        }

        // If no column definition is found, return null
        return null;
    }


    #endregion


    #region ENTRY LOGGING AND SUBMISSION


    // Creates a CSV entry for the given file type
    public void CreateCsvEntry(string fileTypeName, int eventId, params object[] values)
    {
        if (!collecting)
            return;

        VERACsvHandler csvHandler = FindCsvHandlerByFileName(fileTypeName);
        if (csvHandler == null)
        {
            Debug.LogError("[VERA Logger]: No file type could be found associated with provided name \"" +
              fileTypeName + "\"; cannot log CSV entry to the file as desired.");
            return;
        }

        csvHandler.CreateEntry(eventId, values);
    }


    // Submits all CSVs that are currently being recorded
    public void SubmitAllCSVs()
    {
        foreach (VERACsvHandler csvHandler in csvHandlers)
        {
            csvHandler.StartCoroutine(csvHandler.SubmitFileCoroutine());
        }
    }


    // Submits the file for a given file type by name
    public void SubmitCsvFile(string fileTypeName, bool triggerUploadProgress = false)
    {
        VERACsvHandler csvHandler = FindCsvHandlerByFileName(fileTypeName);

        if (csvHandler == null)
        {
            Debug.LogError("[VERA Logger]: No file type could be found associated with provided name \"" +
              fileTypeName + "\"; cannot submit the CSV as desired.");
            return;
        }

        if (triggerUploadProgress)
        {
            StartCoroutine(SubmitFileWithProgressCoroutine(csvHandler));
        }
        else
        {
            csvHandler.StartCoroutine(csvHandler.SubmitFileCoroutine());
        }
    }


    // Submits file from given CSV handler, and triggers events (to show progress)
    public IEnumerator SubmitFileWithProgressCoroutine(VERACsvHandler csvHandler)
    {
        onBeginFileUpload?.Invoke();
        trackedUploadRequest = csvHandler.activeWebRequest;
        yield return csvHandler.StartCoroutine(csvHandler.SubmitFileCoroutine());
        onFileUploadExited?.Invoke();
    }


    // Gets the upload progress of the tracked upload request
    public float UploadProgress
    {
        get
        {
            if (trackedUploadRequest == null || trackedUploadRequest.isDone) return -1;
            return trackedUploadRequest.uploadProgress;
        }
    }


    // Gets the upload file size of the tracked upload request in bytes
    public int uploadFileSizeBytes
    {
        get
        {
            if (trackedUploadRequest == null || trackedUploadRequest.isDone) return -1;
            return trackedUploadRequest.uploadHandler.data.Length;
        }
    }


    // Called when a CSV file is fully uploaded
    // Append the CSV file's name to the uploaded records so we know to not upload it again
    public void OnCsvFullyUploaded(string csvFilePath)
    {
        // Append the uploaded file name to the "uploadedCSVs.txt" file as a new line
        string uploadRecordFilePath = Path.Combine(dataPath, "uploadedCSVs.txt");
        var uploaded = File.ReadAllLines(uploadRecordFilePath);
        if (!Array.Exists(uploaded, element => element == Path.GetFileName(csvFilePath)))
        {
            File.AppendAllText(uploadRecordFilePath,
            Path.GetFileName(csvFilePath) + Environment.NewLine);
        }
    }


    #endregion


    #region EXISTING FILES SUBMISSION


    private void UploadExistingUnuploadedFiles(string alreadyUploadedTxtPath, string existingFilesFolderPath, string extensionToUpload)
    {
        // Create directories if necessary
        if (!Directory.Exists(existingFilesFolderPath))
        {
            Debug.Log($"[VERA Logger] Directory [{existingFilesFolderPath}] does not exist, creating it");
            Directory.CreateDirectory(existingFilesFolderPath);
        }
        if (!File.Exists(alreadyUploadedTxtPath))
        {
            Debug.Log($"File [{alreadyUploadedTxtPath}] does not exist, creating it");
            FileStream f = File.Create(alreadyUploadedTxtPath);
            f.Close();
        }

        // Get existing and uploaded files
        string[] existingFiles = Directory.GetFiles(existingFilesFolderPath, $"*{extensionToUpload}");
        string[] alreadyUploadedFiles = File.ReadAllLines(alreadyUploadedTxtPath);

        // For each existing file, if it is not already uploaded, upload it
        foreach (string file in existingFiles)
        {
            if (file != "" && !Array.Exists(alreadyUploadedFiles, element => element == Path.GetFileName(file)))
            {
                Debug.Log("[VERA Logger] Found an unuploaded file \"" + file + "\"; uploading file...");

                switch (extensionToUpload)
                {
                    case ".csv":
                        StartCoroutine(SubmitExistingCSVCoroutine(file));
                        break;
                    case ".png":
                        SubmitImageFile(file);
                        break;
                    default:
                        SubmitGenericFile(file);
                        break;
                }
            }
        }
    }


    // Submits an existing CSV (from a previous session) based on its file path
    private IEnumerator SubmitExistingCSVCoroutine(string csvFilePath)
    {
        var basename = Path.GetFileName(csvFilePath);
        if (basename.StartsWith("partial_"))
        {
            // CSV processing for partial sync currently in progress; for now, do not submit partial saved CSVs.
            yield break;
            /*
            // Expected pattern: partial_{participant_UUID}_latest.csv
            file_participant_UDID = basename.Substring("partial_".Length);
            int idx = file_participant_UDID.IndexOf("_latest");
            if (idx >= 0)
            {
              file_participant_UDID = file_participant_UDID.Substring(0, idx);
            }
            else
            {
              Debug.LogError("Unexpected partial CSV file name format: " + basename);
              yield break;
            }
            */
        }
        else if (basename.StartsWith("final_unsynced_"))
        {
            // CSV processing for partial sync currently in progress; for now, do not submit partial saved CSVs.
            yield break;
            /*
            // Expected pattern: final_unsynced_{participant_UUID}.csv
            file_participant_UDID = basename.Substring("final_unsynced_".Length);
            int idx = file_participant_UDID.LastIndexOf(".csv");
            if (idx >= 0)
            {
              file_participant_UDID = file_participant_UDID.Substring(0, idx);
            }
            else
            {
              Debug.LogError("Unexpected final CSV file name format: " + basename);
              yield break;
            }
            */
        }

        // Get the IDs associated with this file
        string host = VERAHost.hostUrl;
        string file_participant_UDID;
        string fileTypeId;
        if (csvFilePath.Length > 0 && csvFilePath.Contains("-"))
        {
            // Files are in format expId-siteId-partId-fileTypeId.csv
            // To get participant ID, it will be the third element separated by -'s; fileTypeId will be the fourth.
            string[] split = basename.Split('-');

            if (split.Length == 4)
            {
                file_participant_UDID = split[2];
                fileTypeId = split[3].Split('.')[0];
            }
            else
            {
                Debug.LogError("VERA: Invalid file name");
                yield break;
            }
        }
        else
        {
            Debug.LogError("VERA: Invalid file name");
            yield break;
        }

        string url = $"{host}/api/participants/{file_participant_UDID}/filetypes/{fileTypeId}/files";
        byte[] fileData = null;

        // Read the file's data
        yield return ReadBinaryDataFile(csvFilePath, (result) => fileData = result);
        if (fileData == null)
        {
            Debug.Log("No file data to submit");
            yield break;
        }

        // Set up the request
        WWWForm form = new WWWForm();
        form.AddField("experiment_UUID", experimentUUID);
        form.AddField("participant_UUID", file_participant_UDID);
        form.AddField("site_UUID", siteUUID);
        form.AddBinaryData("fileUpload", fileData, experimentUUID + "-" + siteUUID + "-" + file_participant_UDID + "-" + fileTypeId + ".csv", "text/csv");

        // Send the request
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        yield return request.SendWebRequest();

        // Check success
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[VERA Logger] Successfully uploaded existing file \"" + csvFilePath + "\".");
            OnCsvFullyUploaded(csvFilePath);
        }
        else
        {
            Debug.LogError("[VERA Logger] Failed to upload existing file \"" + csvFilePath + "\".");
        }
    }


    #endregion


    #region SESSION MANAGEMENT


    // Finalizes the current experiment session. Should be called when the experiment is "complete".
    public void FinalizeSession()
    {
        if (sessionFinalized)
            return;

        sessionFinalized = true;
        Debug.Log("[VERA Logger] Session finalized; marking COMPLETE and doing final sync.");
        StartCoroutine(periodicSyncHandler.UploadAllPending());
        // Finalize all syncs
        periodicSyncHandler.StartCoroutine(periodicSyncHandler.FinalSyncAndCompleteStatus());

        // Update participant progress, if they are not already in a finalized state
        if (!activeParticipant.IsInFinalizedState())
            activeParticipant.SetParticipantProgress(VERAParticipantManager.ParticipantProgressState.COMPLETE);
    }


    #endregion


    #region QUIT HANDLING


    // Called when the application wants to quit; if we need to finalize, do so; otherwise, allow quit.
    private bool WantsToQuitHandler()
    {
        if (!sessionFinalized)
        {
            Debug.Log("[VERA Logger] Application is trying to quit. Triggering final sync and status update...");
            StartCoroutine(HandleQuitCleanup());
            return false; // cancel quit until cleanup is done
        }

        return true; // allow quit
    }


    // Called on quit, triggers final sync and then quits
    private IEnumerator HandleQuitCleanup()
    {
        yield return periodicSyncHandler.FinalSyncAndCompleteStatus();

        sessionFinalized = true;
        Debug.Log("[VERA Logger] Quit cleanup complete. Proceeding with quit...");
        Application.Quit();
    }


    // OnApplicationQuit, check if participant needs to be marked as incomplete
    private void OnApplicationQuit()
    {
        // If participant is still in progress, mark as incomplete
        if (!sessionFinalized && !activeParticipant.IsInFinalizedState())
        {
            Debug.LogWarning("[VERA Logger] App is quitting before participant is finalized. Marking participant as INCOMPLETE.");
            activeParticipant.SetParticipantProgress(VERAParticipantManager.ParticipantProgressState.INCOMPLETE);
        }
    }


    #endregion


    #region OTHER HELPERS


    // Submits a generic file to a participant, unassociated with a file type
    public void SubmitGenericFile(string filePath, string timestamp = null, byte[] fileData = null, bool moveFileToUploadDirectory = true)
    {
        genericFileHelper.StartCoroutine(genericFileHelper.SubmitGenericFileCoroutine(filePath, timestamp, fileData, moveFileToUploadDirectory));
    }


    // Submits an image file to a participant, unassociated with a file type
    public void SubmitImageFile(string filePath, string timestamp = null, byte[] fileData = null)
    {
        genericFileHelper.StartCoroutine(genericFileHelper.SubmitImageFileCoroutine(filePath, timestamp, fileData));
    }


    // Reads a binary data file and invokes the callback with the read data
    public IEnumerator ReadBinaryDataFile(string filePath, Action<byte[]> callback)
    {
        bool fileReadSuccess = false;
        byte[] fileData = null;

        // Retry mechanism to handle sharing violation
        for (int i = 0; i < 3; i++)
        {
            try
            {
                //Debug.Log("Reading file: " + filePath);
                fileData = File.ReadAllBytes(filePath);
                fileReadSuccess = true;
                break;
            }
            catch (IOException ex)
            {
                Debug.LogWarning($"[VERA Logger] Attempt {ex.Message} {i + 1}: Failed to read file due to sharing violation. Retrying...");
            }

            if (!fileReadSuccess)
            {
                yield return new WaitForSeconds(0.1f); // Add a small delay before retrying
            }
        }

        if (!fileReadSuccess)
        {
            Debug.LogError("[VERA Logger] Failed to read the file after multiple attempts.");
            yield break;
        }

        if (fileData.Length > 50 * 1024 * 1024)
        {
            Debug.LogError("[VERA Logger] File \"" + filePath + "\" size exceeds the limit of 50MB. File will not be uploaded.");
            yield break;
        }

        callback?.Invoke(fileData);
    }


    #endregion


}

[System.Serializable]
public class VERAAuthInfo
{
    public bool authenticated = false;
    public string authToken = String.Empty;
    public string userId = String.Empty;
    public string userName = String.Empty;
    public string activeExperiment = String.Empty;
    public string activeSite = String.Empty;
}
