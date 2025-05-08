using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VERALogger;
using UnityEngine.Networking;
using System;

public class VERAParticipantManager : MonoBehaviour
{

    // VERAParticipantManager handles the creation, ID, state change, etc. of the active participant

    public string participantUUID { get; private set; }

    // Participant state management
    public enum ParticipantProgressState { RECRUITED, ACCEPTED, WAITLISTED, IN_EXPERIMENT, TERMINATED, INCOMPLETE, GHOSTED, COMPLETE };
    public ParticipantProgressState currentParticipantProgressState { get; private set; }
    private int changeProgressMaxRetries = 3;


    #region PARTICIPANT CREATION


    // Creates the participant via coroutine below
    public void CreateParticipant()
    {
        StartCoroutine(CreateParticipantCoroutine());
    }


    // Creates the participant and uploads to the site
    private IEnumerator CreateParticipantCoroutine()
    {
        // Create a new UUID
        participantUUID = Guid.NewGuid().ToString().Replace("-", "");

        // Set up the request
        string expId = VERALogger.Instance.experimentUUID;
        string siteId = VERALogger.Instance.siteUUID;
        string apiKey = VERALogger.Instance.apiKey;

        string host = VERAHost.hostUrl;
        string url = host + "/api/participants/logs/" + expId + "/" + siteId + "/" + participantUUID;
        Debug.Log("Creating participant at url " + url);

        byte[] emptyData = new byte[0];

        WWWForm form = new WWWForm();
        form.AddField("experiment_UUID", expId);
        form.AddField("participant_UUID", participantUUID);
        form.AddField("site_UUID", siteId);
        form.AddBinaryData("file", emptyData, expId + "-" + siteId + "-" + participantUUID + ".csv", "text/csv");

        // Send the request
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        yield return request.SendWebRequest();

        // Check success
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[VERA Participant] Successfully created a new participant; data will be recorded to this participant.");
        }
        else
        {
            Debug.LogError("[VERA Participant] Failed to create a new participant; data will not be recorded.");
        }
    }


    #endregion


    #region PARTICIPANT PROGRESS


    // Sets the participant's progress via coroutine below
    public void SetParticipantProgress(ParticipantProgressState state)
    {
        StartCoroutine(RetryableChangeProgress(state));
    }


    // Tries to change the participant's progress; will try multiple times
    private IEnumerator RetryableChangeProgress(ParticipantProgressState state)
    {
        currentParticipantProgressState = state;
        Debug.Log("[VERA Participant] Updating current participant's state to \"" + state.ToString() + "\"...");

        // Try multiple times to send the request, in case of failure
        int attempt = 0;
        while (attempt < changeProgressMaxRetries)
        {
            string expId = VERALogger.Instance.experimentUUID;
            string siteId = VERALogger.Instance.siteUUID;
            string apiKey = VERALogger.Instance.apiKey;

            // Send the request
            UnityWebRequest request = UnityWebRequest.Put(
              $"{VERAHost.hostUrl}/api/participants/progress/{expId}/{siteId}/{participantUUID}/{state.ToString()}",
              new byte[0]
            );
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // On success, notify completion
                Debug.Log($"[VERA Participant] Successfully set participant's state to {state}.");

                // If the state is COMPLETE, mark the session as finalized
                if (state == ParticipantProgressState.COMPLETE)
                    VERALogger.Instance.FinalizeSession();

                yield break;
            }
            else
            {
                // On failure, notify non-completion, wait, and try again
                attempt++;
                Debug.LogWarning($"[VERA Participant] Attempt {attempt}: failed to set participant's state to {state}: {request.error}");
                yield return new WaitForSeconds(1f);
            }
        }

        Debug.LogError($"[VERA Participant] Failed to set participant's state to {state} after {changeProgressMaxRetries} attempts.");
    }


    // Returns whether this participant is in a "finalized" state (i.e., no new data should be recorded)
    // Finalized states currently include complete, incomplete, and terminated
    public bool IsInFinalizedState()
    {
        return (currentParticipantProgressState == ParticipantProgressState.COMPLETE ||
            currentParticipantProgressState == ParticipantProgressState.INCOMPLETE ||
            currentParticipantProgressState == ParticipantProgressState.TERMINATED);
    }


    #endregion


}
