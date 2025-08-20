#if VERAConditionGroup_Distractions
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public static class VERAConditionGroup_Distractions
{
    public static class HandMaterialIndex
    {
        public static int DefaultValue { get { return 1; } }

        public static int Value
        {
            get
            {
                try
                {
                    string experimentId = UnityEngine.PlayerPrefs.GetString("VERA_ActiveExperiment", "");
                    string authToken = UnityEngine.PlayerPrefs.GetString("VERA_UserAuthToken", "");
                    if (string.IsNullOrEmpty(experimentId) || string.IsNullOrEmpty(authToken))
                    {
                        return DefaultValue;
                    }
                    string url = $"{VERAHost.hostUrl}/api/experiments/{experimentId}/conditions/Distractions/conditions/HandMaterialIndex";
                    using (var www = UnityWebRequest.Get(url))
                    {
                        www.SetRequestHeader("Authorization", $"Bearer {authToken}");
                        var operation = www.SendWebRequest();
                        while (!operation.isDone) { }
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            var response = JsonUtility.FromJson<ConditionResponse>(www.downloadHandler.text);
                            if (response.success && response.condition != null)
                            {
                                return ParseValue(response.condition.value);
                            }
                        }
                    }
                }
                catch
                {
                    // Fall back to default on any error
                }
                return DefaultValue;
            }
        }

        public static IEnumerator GetValueAsync(Action<int> callback)
        {
            string experimentId = UnityEngine.PlayerPrefs.GetString("VERA_ActiveExperiment", "");
            if (string.IsNullOrEmpty(experimentId))
            {
                callback?.Invoke(DefaultValue);
                yield break;
            }
            string authToken = UnityEngine.PlayerPrefs.GetString("VERA_UserAuthToken", "");
            if (string.IsNullOrEmpty(authToken))
            {
                callback?.Invoke(DefaultValue);
                yield break;
            }
            string url = $"{VERAHost.hostUrl}/api/experiments/{experimentId}/conditions/Distractions/conditions/HandMaterialIndex";
            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", $"Bearer {authToken}");
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<ConditionResponse>(www.downloadHandler.text);
                        if (response.success && response.condition != null)
                        {
                            callback?.Invoke(ParseValue(response.condition.value));
                        }
                        else
                        {
                            callback?.Invoke(DefaultValue);
                        }
                    }
                    catch
                    {
                        callback?.Invoke(DefaultValue);
                    }
                }
                else
                {
                    callback?.Invoke(DefaultValue);
                }
            }
        }

        private static int ParseValue(string value)
        {
            return int.Parse(value);
        }
    }

    public static class HatEnabled
    {
        public static bool DefaultValue { get { return true; } }

        public static bool Value
        {
            get
            {
                try
                {
                    string experimentId = UnityEngine.PlayerPrefs.GetString("VERA_ActiveExperiment", "");
                    string authToken = UnityEngine.PlayerPrefs.GetString("VERA_UserAuthToken", "");
                    if (string.IsNullOrEmpty(experimentId) || string.IsNullOrEmpty(authToken))
                    {
                        return DefaultValue;
                    }
                    string url = $"{VERAHost.hostUrl}/api/experiments/{experimentId}/conditions/Distractions/conditions/HatEnabled";
                    using (var www = UnityWebRequest.Get(url))
                    {
                        www.SetRequestHeader("Authorization", $"Bearer {authToken}");
                        var operation = www.SendWebRequest();
                        while (!operation.isDone) { }
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            var response = JsonUtility.FromJson<ConditionResponse>(www.downloadHandler.text);
                            if (response.success && response.condition != null)
                            {
                                return ParseValue(response.condition.value);
                            }
                        }
                    }
                }
                catch
                {
                    // Fall back to default on any error
                }
                return DefaultValue;
            }
        }

        public static IEnumerator GetValueAsync(Action<bool> callback)
        {
            string experimentId = UnityEngine.PlayerPrefs.GetString("VERA_ActiveExperiment", "");
            if (string.IsNullOrEmpty(experimentId))
            {
                callback?.Invoke(DefaultValue);
                yield break;
            }
            string authToken = UnityEngine.PlayerPrefs.GetString("VERA_UserAuthToken", "");
            if (string.IsNullOrEmpty(authToken))
            {
                callback?.Invoke(DefaultValue);
                yield break;
            }
            string url = $"{VERAHost.hostUrl}/api/experiments/{experimentId}/conditions/Distractions/conditions/HatEnabled";
            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", $"Bearer {authToken}");
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<ConditionResponse>(www.downloadHandler.text);
                        if (response.success && response.condition != null)
                        {
                            callback?.Invoke(ParseValue(response.condition.value));
                        }
                        else
                        {
                            callback?.Invoke(DefaultValue);
                        }
                    }
                    catch
                    {
                        callback?.Invoke(DefaultValue);
                    }
                }
                else
                {
                    callback?.Invoke(DefaultValue);
                }
            }
        }

        private static bool ParseValue(string value)
        {
            return bool.Parse(value);
        }
    }


    // Response classes for JSON parsing
    [System.Serializable]
    public class ConditionResponse
    {
        public bool success;
        public ConditionData condition;
    }

    [System.Serializable]
    public class ConditionData
    {
        public string name;
        public string value;
        public string _id;
    }

}
#endif
