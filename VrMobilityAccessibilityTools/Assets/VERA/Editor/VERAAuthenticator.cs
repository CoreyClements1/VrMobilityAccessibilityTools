using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

#if UNITY_EDITOR

[ExecuteInEditMode]
public static class VERAAuthenticator
{
    private static HttpListener listener;
    private static bool isRunning = false;

    private const string listenUrl = "http://localhost:8080/auth";
    private const string columnsFilePath = "Assets/VERA/FileTypes/Columns/Resources";


    #region AUTHENTICATION SERVER CALLS


    public static void StartAuthentication()
    {

        // Start the server
        StartServer();
        // Open the authentication URL in the default browser
        Application.OpenURL(VERAHost.hostUrl + "/Authenticate");
    }

    // Starts the server
    private static void StartServer()
    {
        if (listener == null)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(listenUrl + "/");
        }

        if (!listener.IsListening)
        {
            listener.Start();
            isRunning = true;
            listener.BeginGetContext(HandleAuthenticationRequest, listener);
        }
    }

    // Stops the server
    private static void StopServer()
    {
        if (listener != null && listener.IsListening)
        {
            isRunning = false;
            listener.Stop();
            listener.Close();
            listener = null;
        }
    }

    // Handles the authentication request
    private static void HandleAuthenticationRequest(IAsyncResult result)
    {
        if (!isRunning) return;

        var context = listener.EndGetContext(result);
        var request = context.Request;

        // Enable CORS
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        // Handle CORS Preflight
        if (request.HttpMethod == "OPTIONS")
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();
            listener.BeginGetContext(HandleAuthenticationRequest, listener);
            return;
        }

        // Process POST request
        if (request.HttpMethod == "POST")
        {
            using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
            {
                string read = reader.ReadToEnd();
                UnityTokenResponse response = JsonUtility.FromJson<UnityTokenResponse>(read);
                string token = response.token;
                string userId = response.user._id;
                string userName = response.user.firstName + " " + response.user.lastName;

                // Save
                EditorApplication.delayCall += () =>
                {
                    SaveAuthentication(token, userId, userName);
                    Debug.Log("[VERA Connection] You are successfully authenticated and connected to the VERA portal.\n");
                };
            }

            // Respond with success
            byte[] responseBytes = Encoding.UTF8.GetBytes("Token received");
            context.Response.ContentLength64 = responseBytes.Length;
            context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
            context.Response.Close();

            // Stop the server after receiving the token
            StopServer();
        }

        // Listen for the next request if still running
        if (isRunning)
        {
            listener.BeginGetContext(HandleAuthenticationRequest, listener);
        }
    }


    #endregion


    #region SAVING AUTHENTICATION


    // Saves various authentication parameters
    private static void SaveAuthentication(string token, string userId, string userName)
    {
        // Get current auth info, to not overwrite other existing info
        VERAAuthInfo currentAuthInfo = GetSavedAuthInfo();

        // Set info
        currentAuthInfo.authenticated = true;
        currentAuthInfo.authToken = token;
        currentAuthInfo.userId = userId;
        currentAuthInfo.userName = userName;

        // Push to StreamingAssets
        SetSavedAuthInfo(currentAuthInfo);
    }

    // Clears various authentication parameters
    private static void SaveDeauthentication()
    {
        // Set info to default (deauthenticated / no info)
        VERAAuthInfo deauthInfo = new VERAAuthInfo();
        VERAAuthInfo currentInfo = GetSavedAuthInfo();

        if (currentInfo != null)
        {
            deauthInfo.activeExperiment = currentInfo.activeExperiment;
            deauthInfo.activeSite = currentInfo.activeSite;
        }

        // Push to StreamingAssets
        SetSavedAuthInfo(deauthInfo);
    }

    // Clears various authentication parameters
    public static void ClearAuthentication()
    {
        // Save
        EditorApplication.delayCall += () =>
        {
            SaveDeauthentication();
        };
    }

    // Sets the saved authentication info (file in StreamingAssets) to a new authInfo
    private static void SetSavedAuthInfo(VERAAuthInfo authInfo)
    {
        // Convert to JSON
        string json = JsonUtility.ToJson(authInfo, true); // Pretty print for readability

        // File paths
        string directoryPath = Path.Combine(Application.dataPath, "Resources");
        string filePath = Path.Combine(directoryPath, "VERAAuthentication.json");

        // Ensure the directory exists
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Write to the file
        File.WriteAllText(filePath, json);

        // Update PlayerPrefs for editor use
        PlayerPrefs.SetString("VERA_AuthToken", authInfo.authToken);
        PlayerPrefs.SetString("VERA_UserId", authInfo.userId);
        PlayerPrefs.SetString("VERA_UserName", authInfo.userName);
        PlayerPrefs.SetString("VERA_ActiveExperiment", authInfo.activeExperiment);
        PlayerPrefs.SetString("VERA_ActiveSite", authInfo.activeSite);
        PlayerPrefs.SetInt("VERA_Authenticated", authInfo.authenticated ? 1 : 0);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    // Gets saved authentication info (file in StreamingAssets)
    private static VERAAuthInfo GetSavedAuthInfo()
    {
        // File paths
        string directoryPath = Path.Combine(Application.dataPath, "Resources");
        string filePath = Path.Combine(directoryPath, "VERAAuthentication.json");

        // Ensure the directory exists
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Ensure file exists
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<VERAAuthInfo>(json);
        }
        else
        {
            // File not found, authentication likely not set up yet
            return new VERAAuthInfo();
        }
    }


    #endregion


    #region USER CONNECTION


    // Gets whether the current user is connected to the VERA portal or not
    public static void IsUserConnected(Action<bool> onComplete)
    {
        // Check whether user is authenticated or not
        if (PlayerPrefs.GetInt("VERA_Authenticated", 0) == 0)
        {
            onComplete?.Invoke(false);
            return;
        }

        // To test connection, make a request to get this user's experiments
        string url = $"{VERAHost.hostUrl}/api/experiments/";

        // Create a UnityWebRequest with the POST method
        UnityWebRequest request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(new byte[0]); // Empty body

        // Set headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("VERA_AuthToken"));

        // Send the request
        var operation = request.SendWebRequest();

        // Use EditorApplication.update to check the request's progress
        EditorApplication.update += EditorUpdate;

        void EditorUpdate()
        {
            if (operation.isDone)
            {
                EditorApplication.update -= EditorUpdate;

                // Check for errors
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onComplete?.Invoke(false);
                    request.Dispose();
                    return;
                }

                onComplete?.Invoke(true);
                request.Dispose();
                return;
            }
        }
    }


    #endregion


    #region EXPERIMENT MANAGEMENT


    // Gets all experiments associated with a user
    public static void GetUserExperiments(Action<List<Experiment>> onComplete)
    {
        List<Experiment> ret = new List<Experiment>();

        string url = $"{VERAHost.hostUrl}/api/experiments/";

        // Create a UnityWebRequest with the POST method
        UnityWebRequest request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(new byte[0]); // Empty body

        // Set headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("VERA_AuthToken"));

        // Send the request
        var operation = request.SendWebRequest();

        // Use EditorApplication.update to check the request's progress
        EditorApplication.update += EditorUpdate;

        void EditorUpdate()
        {
            if (operation.isDone)
            {
                EditorApplication.update -= EditorUpdate;

                // Check for errors
                if (request.result != UnityWebRequest.Result.Success)
                {
                    if (request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogError("VERA: There was an issue connecting to the VERA portal, and you have been logged out. " +
                            "Please re-authenticate using the \"VERA -> Settings\" menu item.");
                    }
                    else if (request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError("VERA: You are not authenticated, and will not be able to run experiments. " +
                            "Use the \"VERA -> Settings\" menu bar item to authenticate.");
                    }
                    
                    ClearAuthentication();
                }
                else
                {
                    // Parse the response
                    string jsonResponse = request.downloadHandler.text;

                    // Deserialize JSON to GetExperimentsResponse
                    GetExperimentsResponse response = JsonUtility.FromJson<GetExperimentsResponse>(jsonResponse);

                    if (response != null && response.success)
                    {
                        // Access the list of experiments
                        foreach (Experiment exp in response.experiments)
                        {
                            if (exp.sites.Count == 0)
                            {
                                Site emptySite = new Site();
                                emptySite._id = "none";
                                emptySite.name = "none";
                                emptySite.parentExperiment = exp._id;
                                exp.sites.Add(emptySite);
                            }
                            ret.Add(exp);
                        }

                        onComplete?.Invoke(ret);
                    }
                    else
                    {
                        Debug.LogError("VERA: Received an unexpected response from the VERA portal when fetching experiments. " +
                            "Please try again later.");

                        onComplete?.Invoke(null);
                    }
                }

                // Dispose of the request
                request.Dispose();
            }
        }
    }

    // Changes the currently active experiment
    public static void ChangeActiveExperiment(string activeExperimentId)
    {
        // Get current auth info, to not overwrite other existing info
        VERAAuthInfo currentAuthInfo = GetSavedAuthInfo();

        // Set info
        currentAuthInfo.activeExperiment = activeExperimentId;

        // Push to file
        SetSavedAuthInfo(currentAuthInfo);

        // Update PlayerPrefs for use in editor
        PlayerPrefs.SetString("VERA_ActiveExperiment", activeExperimentId);

        // Update session state for dev tools sim participant to avoid inter-experiment conflicts
        SessionState.SetBool("VERA_SimParticipant", false);

        // Update all column definition assets to this new experiment
        UpdateColumnDefs();
    }

    // Changes the currently active site
    public static void ChangeActiveSite(string activeSiteId)
    {
        // Get current auth info, to not overwrite other existing info
        VERAAuthInfo currentAuthInfo = GetSavedAuthInfo();

        // Set info
        currentAuthInfo.activeSite = activeSiteId;

        // Push to file
        SetSavedAuthInfo(currentAuthInfo);

        // Update PlayerPrefs for use in editor
        PlayerPrefs.SetString("VERA_ActiveSite", activeSiteId);
    }


    #endregion


    #region FILE TYPE / COLUMN MANAGEMENT


    // Updates the column definition to the current experiment's column definition
    public static void UpdateColumnDefs()
    {
        // Clear old column definitions
        DeleteExistingColumnDefs();

        // If there is no active experiment, we cannot do anything with the columns
        if (PlayerPrefs.GetString("VERA_ActiveExperiment", null) == null || PlayerPrefs.GetString("VERA_ActiveExperiment", null) == "")
        {
            ClearFileTypeDefineSymbols();
            return;
        }

        // Start by getting all FileTypes for the experiment;
        // Then, filter only by those which are CSV's; each CSV will have an associated column definition.
        // Make a new column definition asset for each CSV FileType, based on the FileType's fetched definition.
        // These column def's will be used by the VERALogger to record data.

        // URL to get all FileTypes for this experiment
        string url = $"{VERAHost.hostUrl}/api/experiments/{PlayerPrefs.GetString("VERA_ActiveExperiment")}/filetypes";

        // Create a UnityWebRequest with the GET method
        UnityWebRequest request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(new byte[0]); // Empty body

        // Set headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("VERA_AuthToken"));

        // Send the request
        var operation = request.SendWebRequest();

        // Use EditorApplication.update to check the request's progress
        EditorApplication.update += EditorUpdate;

        void EditorUpdate()
        {
            if (operation.isDone)
            {
                EditorApplication.update -= EditorUpdate;
                // On error, can't make any column definitions
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("VERA - Unexpected response from server; could not get column definitions. " +
                            "Please try refreshing your experiments and trying again.");
                    return;
                }
                else
                {
                    // Parse the response
                    string jsonResponse = request.downloadHandler.text;
                    FileTypesResponse fileTypesResponse = JsonUtility.FromJson<FileTypesResponse>(jsonResponse);

                    if (fileTypesResponse == null || !fileTypesResponse.success)
                    {
                        Debug.LogError("VERA - Unexpected response from server; could not get column definitions. " +
                            "Please try refreshing your experiments and trying again.");
                        return;
                    }

                    // Loop through each file type and get column definition, if the file type is a CSV
                    List<FtFileType> fileTypes = fileTypesResponse.fileTypes;
                    List<VERAColumnDefinition> columnDefs = new List<VERAColumnDefinition>();
                    List<string> definitionsToAdd = new List<string>();
                    for (int i = 0; i < fileTypes.Count; i++)
                    {
                        if (fileTypes[i].extension == "csv" && fileTypes[i].columnDefinition != null)
                        {
                            // This file type is a CSV file with an associated column definition.
                            // Create the column definition asset for this filetype, for use by VERALogger
                            columnDefs.Add(ScriptableObject.CreateInstance<VERAColumnDefinition>());
                            int idx = columnDefs.Count - 1;
                            if (!Directory.Exists(columnsFilePath))
                            {
                                Directory.CreateDirectory(columnsFilePath);
                            }
                            AssetDatabase.CreateAsset(columnDefs[idx], columnsFilePath + "/VERA_" + fileTypes[i].name + "_ColumnDefinition.asset");

                            // Sort the columns based on order
                            List<FtColumn> sortedCols = fileTypes[i].columnDefinition.columns.OrderBy(col => col.order).ToList();

                            // Set columns
                            columnDefs[idx].columns.Clear();
                            foreach (FtColumn col in sortedCols)
                            {
                                VERAColumnDefinition.Column newCol = new VERAColumnDefinition.Column();
                                newCol.name = col.name;
                                newCol.description = col.description;
                                switch (col.dataType)
                                {
                                    case "String":
                                        newCol.type = VERAColumnDefinition.DataType.String;
                                        break;
                                    case "Integer":
                                        newCol.type = VERAColumnDefinition.DataType.Number;
                                        break;
                                    case "Transform":
                                        newCol.type = VERAColumnDefinition.DataType.Transform;
                                        break;
                                    case "Date":
                                        newCol.type = VERAColumnDefinition.DataType.Date;
                                        break;
                                    case "JSON":
                                        newCol.type = VERAColumnDefinition.DataType.JSON;
                                        break;
                                }
                                columnDefs[idx].columns.Add(newCol);
                            }

                            // Save column def
                            columnDefs[idx].fileType = new VERAColumnDefinition.FileType();
                            columnDefs[idx].fileType.fileTypeId = fileTypes[i]._id;
                            columnDefs[idx].fileType.name = fileTypes[i].name;
                            columnDefs[idx].fileType.description = fileTypes[i].description;

                            EditorUtility.SetDirty(columnDefs[idx]);
                            AssetDatabase.SaveAssets();

                            // Add define symbol for this column definition
                            definitionsToAdd.Add("VERAFile_" + fileTypes[i].name);
                        }
                    }

                    AssetDatabase.Refresh();

                    // Generate code for all file types
                    FileTypeGenerator.GenerateAllFileTypesCsCode();
                    ReplaceDefines(definitionsToAdd);
                }

                request.Dispose();
            }
        }
    }

    // Deletes all existing column definitions in the columns folder
    public static void DeleteExistingColumnDefs()
    {
        if (Directory.Exists(columnsFilePath))
        {
            string[] files = Directory.GetFiles(columnsFilePath);

            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException e)
                {
                    Debug.LogError($"IO Exception deleting file: {file}\n{e.Message}");
                }
            }
        }

        AssetDatabase.Refresh();
    }


    #endregion


    #region PREPROCESSORS / DEFINE SYMBOLS


    // Gets all define symbols
    private static List<string> GetDefineSymbols()
    {
        BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(activeBuildTarget);

        List<string> currentSymbols = PlayerSettings
            .GetScriptingDefineSymbolsForGroup(activeBuildTargetGroup).Split(';').ToList();

        return currentSymbols;
    }

    // Saves define symbols as per given list
    private static void SaveDefineSymbols(List<string> symbols)
    {
        BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(activeBuildTarget);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(activeBuildTargetGroup, string.Join(";", symbols));
    }

    // Replaces all define symbols with the given list;
    // does not replace any define symbols that do not need to be replaced.
    private static void ReplaceDefines(List<string> symbols)
    {
        HashSet<string> newSymbols = new HashSet<string>(symbols);
        List<string> oldVeraSymbols = GetDefineSymbols().Where(s => s.StartsWith("VERAFile_")).ToList();
        HashSet<string> oldSymbols = new HashSet<string>(GetDefineSymbols());

        List<string> stringsToAdd = newSymbols.Except(oldSymbols).ToList();
        List<string> stringsToDelete = oldSymbols.Except(newSymbols).ToList();

        if (stringsToDelete.Count > 0)
        {
            foreach (string s in stringsToDelete)
            {
                RemoveDefineSymbol(s);
            }
        }

        if (stringsToAdd.Count > 0)
        {
            foreach(string s in stringsToAdd)
            {
                AddDefineSymbol(s);
            }
        }
    }

    // Adds a define symbol to the Unity player's settings
    private static void AddDefineSymbol(string symbol)
    {
        List<string> currentSymbols = GetDefineSymbols();

        if (!currentSymbols.Contains(symbol))
        {
            currentSymbols.Add(symbol);
            SaveDefineSymbols(currentSymbols);
        }
    }

    // Removes a define symbol from the Unity player's settings
    private static void RemoveDefineSymbol(string symbol)
    {
        List<string> currentSymbols = GetDefineSymbols();

        if (currentSymbols.Contains(symbol))
        {
            currentSymbols.Remove(symbol);
            SaveDefineSymbols(currentSymbols);
        }
    }

    // Removes all VERA-related define symbols from the Unity player's settings
    public static void ClearFileTypeDefineSymbols()
    {
        List<string> currentSymbols = GetDefineSymbols();

        // Remove all that start with "VERAFile"
        currentSymbols.RemoveAll(symbol => symbol.StartsWith("VERAFile"));

        SaveDefineSymbols(currentSymbols);
    }


    #endregion


    #region OTHER HELPERS


    // String helper
    private static string PadBase64(string base64)
    {
        // Ensure the base64 string is properly padded
        return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
    }


    #endregion

    
}
#endif

// JSON helper classes

[System.Serializable]
public class Experiment
{
    public string _id;
    public string name;
    public string createdBy;
    public List<string> users;
    public List<string> participants;
    public bool isMultiSite;
    public List<Site> sites = new List<Site>();
}

[System.Serializable]
public class Site
{
    public string _id;
    public string name;
    public string parentExperiment;
}

[System.Serializable]
public class GetExperimentsResponse
{
    public bool success;
    public List<Experiment> experiments;
    public List<string> ids;
}

[Serializable]
public class FileTypesResponse
{
    public bool success;
    public List<FtFileType> fileTypes;
}

[Serializable]
public class FtFileType
{
    public string _id;
    public string name;
    public string experimentId;
    public string extension;
    public string description;

    public FtColumnDefinition columnDefinition;
}

[Serializable]
public class FtColumnDefinition
{
    public string _id;
    public string fileTypeId;
    public List<FtColumn> columns;
}

[Serializable]
public class FtColumn
{
    public string _id;
    public string columnDefinitionId;
    public string dataType;
    public string name;
    public string description;
    public string transform;
    public int order;
}

[Serializable]
public class UnityTokenResponse
{
    public UserResponse user;
    public string token;
}

[Serializable]
public class UserResponse
{
    public string _id;
    public string firstName;
    public string lastName;
    public string email;
}
