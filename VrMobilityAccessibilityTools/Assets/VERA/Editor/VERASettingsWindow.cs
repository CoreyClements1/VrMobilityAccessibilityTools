#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class VERASettingsWindow : EditorWindow
{

    private string noExpFound = "[No active experiment]";

    private int selectedExperimentIndex;
    private int selectedSiteIndex;
    private List<Experiment> experimentList = null;
    private string timeExperimentsLastRefreshed = string.Empty;

    [MenuItem("VERA/Settings")]
    public static void ShowWindow()
    {
        // Show existing window instance or create a new one
        VERASettingsWindow window = GetWindow<VERASettingsWindow>("VERA Settings");
        window.Show();
    }


    #region ON GUI


    private void OnGUI()
    {
        GUILayout.Label("VERA Settings", EditorStyles.boldLabel);

        // Display differently according to whether user is authenticated or not
        if (PlayerPrefs.GetInt("VERA_Authenticated") == 1)
        {
            if (experimentList == null)
                LoadSettings();

            // Display the welcome message with the user's name
            string userName = PlayerPrefs.GetString("VERA_UserName", "User");
            GUILayout.Label($"Welcome {userName}!", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);

            // Display de-authenticate button
            if (GUILayout.Button("Log Out"))
            {
                VERAAuthenticator.ClearAuthentication();
            }

            GUILayout.Space(10); // Add space between sections

            GUILayout.Label("Your Experiment", EditorStyles.boldLabel);

            // Add the experiment selection description
            GUILayout.Label("Use the dropdown below to select from your experiments. Your Unity project can only be linked to a single experiment at a time.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(5);

            GUILayout.Label("If you don't see your experiment in the dropdown, use the button below to refresh, and look for your experiment again.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(5);

            // Dropdown menu for experiment selection
            string[] options = new string[experimentList.Count];
            for (int i = 0; i < experimentList.Count; i++)
            {
                options[i] = experimentList[i].name;
            }
            int newSelectedExperimentIndex = EditorGUILayout.Popup("Select Experiment", selectedExperimentIndex, options);

            // Check if the dropdown index has changed
            if (newSelectedExperimentIndex != selectedExperimentIndex)
            {
                selectedExperimentIndex = newSelectedExperimentIndex;
                VERAAuthenticator.ChangeActiveExperiment(experimentList[selectedExperimentIndex]._id);
                selectedSiteIndex = 0;
                VERAAuthenticator.ChangeActiveSite(experimentList[selectedExperimentIndex].sites[selectedSiteIndex]._id);
                SaveSettings();
            }

            // Display multi-site options, if applicable
            if (selectedExperimentIndex < experimentList.Count && experimentList[selectedExperimentIndex] != null && experimentList[selectedExperimentIndex].isMultiSite)
            {
                // Dropdown menu for site selection
                List<Site> siteList = experimentList[selectedExperimentIndex].sites;
                string[] siteOptions = new string[siteList.Count];
                for (int i = 0; i < siteList.Count; i++)
                {
                    siteOptions[i] = siteList[i].name;
                }
                int newSelectedSiteIndex = EditorGUILayout.Popup("Select Site", selectedSiteIndex, siteOptions);

                // Check if the site index has changed
                if (newSelectedSiteIndex != selectedSiteIndex)
                {
                    selectedSiteIndex = newSelectedSiteIndex;
                    VERAAuthenticator.ChangeActiveSite(experimentList[selectedExperimentIndex].sites[selectedSiteIndex]._id);
                    SaveSettings();
                }
            }

            // Display refresh experiments button
            if (GUILayout.Button("Refresh Experiments"))
            {
                RefreshExperiments();
            }

            // Display last updated time
            GUILayout.Label("Experiments last updated on " + timeExperimentsLastRefreshed + ".", EditorStyles.wordWrappedLabel);

            GUILayout.Space(10);

            DisplayConnectionStatusOptions();
        }
        else
        {
            // Notify the user is not authenticated
            GUILayout.Label("You are not yet authenticated. Click the button below to authenticate, and be able to use VERA's tools." +
                "\nMake sure you are connected to the internet before authenticating.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);

            // Display authentication button
            if (GUILayout.Button("Authenticate"))
            {
                experimentList = null;
                VERAAuthenticator.StartAuthentication();
            }

            GUILayout.Space(10);

            DisplayConnectionStatusOptions();
        }
    }


    #endregion


    #region REFRESH EXPERIMENTS


    // Refreshes the displayed experiments
    private void RefreshExperiments()
    {
        // When refreshed, get all user experiments from web
        VERAAuthenticator.GetUserExperiments((result) =>
        {
            string oldActiveId;
            string oldActiveSiteId;

            // Store old active id, to set active experiment to it (if applicable)
            oldActiveId = PlayerPrefs.GetString("VERA_ActiveExperiment");
            oldActiveSiteId = PlayerPrefs.GetString("VERA_ActiveSite");

            // Set the list, and add an option for no active experiment
            experimentList = result;
            if (experimentList == null || experimentList.Count == 0)
            {
                experimentList = new List<Experiment>();
                Experiment noExp = new Experiment();
                noExp.isMultiSite = false;
                noExp.name = noExpFound;
                noExp._id = "N/A";
                Site emptySite = new Site();
                emptySite._id = "none";
                emptySite.name = "none";
                emptySite.parentExperiment = noExp._id;
                noExp.sites.Add(emptySite);
                experimentList.Add(noExp);
            }

            // Find the index of the element with Value matching oldActiveId
            selectedExperimentIndex = -1;
            for (int i = 0; i < experimentList.Count; i++)
            {
                if (experimentList[i]._id == oldActiveId)
                {
                    selectedExperimentIndex = i;
                    break;
                }
            }

            if (selectedExperimentIndex == -1)
            {
                // Element not found, update active experiment to a default value (first element; can be [No active experiment])
                selectedExperimentIndex = 0;

                // Check if the selected experiment is real
                if (!experimentList[selectedExperimentIndex].name.Equals(noExpFound))
                {
                    // If it's real, check if it's the demo experiment
                    if (experimentList[selectedExperimentIndex].name.Equals("Demo Experiment"))
                    {
                        // If a column definition does not exist by this experiment, create a new one based on the demo.
                        string templatePath = "Assets/VERA/Columns/DemoColumnDefinition.asset";
                        string newDefsPath = "Assets/VERA/Columns/" + experimentList[selectedExperimentIndex]._id + "_ColumnDefinition.asset";

                        var existingAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(newDefsPath);
                        if (existingAsset == null)
                        {
                            AssetDatabase.CopyAsset(templatePath, newDefsPath);

                            AssetDatabase.Refresh();
                            AssetDatabase.SaveAssets();
                        }
                    }

                    // Update the active experiment to the real experiment
                    VERAAuthenticator.ChangeActiveExperiment(experimentList[selectedExperimentIndex]._id);
                }
                // If the selected experiment is not real, change the active experiment to nothing
                else
                {
                    VERAAuthenticator.ChangeActiveExperiment(null);
                }
            }
            else
            {
                VERAAuthenticator.ChangeActiveExperiment(experimentList[selectedExperimentIndex]._id);
            }

            // Find the index of the old selected site
            selectedSiteIndex = -1;
            List<Site> siteList = experimentList[selectedExperimentIndex].sites;
            for (int i = 0; i < siteList.Count; i++)
            {
                if (siteList[i]._id == oldActiveSiteId)
                {
                    selectedSiteIndex = i;
                    break;
                }
            }

            if (selectedSiteIndex == -1)
            {
                // Element not found, update active site to a default value (first element; can be empty site)
                selectedSiteIndex = 0;
            }

            VERAAuthenticator.ChangeActiveSite(experimentList[selectedExperimentIndex].sites[selectedSiteIndex]._id);

            timeExperimentsLastRefreshed = DateTime.Now.ToString("MMMM dd, h:mm:ss tt");
            SaveSettings();
        });
    }


    #endregion


    #region SAVE / LOAD SETTINGS


    // Saves VERA settings to PlayerPrefs (for persistent data)
    private void SaveSettings()
    {
        // Save selectedExperimentIndex
        PlayerPrefs.SetInt("VERA_SelectedExperimentIndex", selectedExperimentIndex);
        PlayerPrefs.SetInt("VERA_SelectedSiteIndex", selectedSiteIndex);

        // Save experimentList as a JSON string
        if (experimentList != null)
        {
            string json = JsonUtility.ToJson(new SerializableList<Experiment>(experimentList));
            PlayerPrefs.SetString("VERA_ExperimentList", json);
        }
    }

    // Loads VERA settings from PlayerPrefs (for persistent data)
    private void LoadSettings()
    {
        // Load selectedExperimentIndex and selectedSiteIndex
        selectedExperimentIndex = PlayerPrefs.GetInt("VERA_SelectedExperimentIndex", 0);
        selectedSiteIndex = PlayerPrefs.GetInt("VERA_SelectedSiteIndex", 0);

        // Load experimentList from JSON string
        string json = PlayerPrefs.GetString("VERA_ExperimentList", null);
        if (!string.IsNullOrEmpty(json))
        {
            experimentList = JsonUtility.FromJson<SerializableList<Experiment>>(json).ToList();
        }
        else
        {
            Experiment noExp = new Experiment();
            noExp.isMultiSite = false;
            noExp.name = noExpFound;
            noExp._id = "N/A";
            Site emptySite = new Site();
            emptySite._id = "none";
            emptySite.name = "none";
            emptySite.parentExperiment = noExp._id;
            noExp.sites.Add(emptySite);
            experimentList = new List<Experiment> { noExp };
        }

        RefreshExperiments();
    }


    #endregion


    #region CONNECTION STATUS


    // Called when the Unity editor first launches; if connection updates are enabled, test the connection.
    [InitializeOnLoadMethod]
    private static void OnEditorLoad()
    {
        if (PlayerPrefs.GetInt("VERA_DisplayConnectionNotifs", 1) == 1)
        {
            TestUserConnection(true);
        }
    }

    // Displays the options for enabling / disabling connection status nofitications
    private void DisplayConnectionStatusOptions()
    {
        // Notify the user of connection status feature
        GUILayout.Label("Connection Notifications", EditorStyles.boldLabel);
        GUILayout.Label("VERA will occassionally provide periodic console logs notifying you of your connection " +
            "status to the VERA portal. Use the toggle below to enable / disable this feature.", EditorStyles.wordWrappedLabel);

        // Get saved preference for connection status
        bool currentConnectionNotifPref = (PlayerPrefs.GetInt("VERA_DisplayConnectionNotifs", 1) == 1);
        bool toggledConnectionNotifPref = EditorGUILayout.Toggle("Enable Connection Notifications", currentConnectionNotifPref);

        // Update saved preference to playerprefs if it has changed
        if (toggledConnectionNotifPref != currentConnectionNotifPref)
        {
            if (toggledConnectionNotifPref)
                PlayerPrefs.SetInt("VERA_DisplayConnectionNotifs", 1);
            else
                PlayerPrefs.SetInt("VERA_DisplayConnectionNotifs", 0);
        }

        // Display a button to test the user's connection manually
        if (GUILayout.Button("Am I Connected?"))
        {
            TestUserConnection(false);
        }
    }

    // Tests the user's connection, and prints the result in the console
    private static void TestUserConnection(bool canUserDisable)
    {
        string authSuccess = "[VERA Connection] You are successfully connected to the VERA portal.";
        string unauthError = "[VERA Connection] You are not connected to the VERA portal, and will not be able " +
                            "to run experiments. Use the \"VERA -> Settings\" menu bar item to connect.";
        if (canUserDisable)
        {
            string disablableMessage = "\nYou can disable this message in the \"VERA -> Settings\" window.";
            authSuccess += disablableMessage;
            unauthError += disablableMessage;
        }

        // Ensure we are connected / token is not expired
        VERAAuthenticator.IsUserConnected((isConnected) =>
        {
            if (isConnected)
            {
                Debug.Log(authSuccess);
            }
            else
            {
                Debug.LogError(unauthError);
                VERAAuthenticator.ClearAuthentication();
            }
        });
    }


    #endregion


    // Serializable wrapper for List (for saving data as json)
    [System.Serializable]
    public class SerializableList<T>
    {
        public List<T> Items;

        public SerializableList() => Items = new List<T>();
        public SerializableList(List<T> items) => Items = items;

        public List<T> ToList() => Items ?? new List<T>();
    }


}
#endif