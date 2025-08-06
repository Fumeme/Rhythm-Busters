using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TeamManager : MonoBehaviour
{
    public List<TeamSlot> teamSlots; // List of team slots in the scene
    public Button startButton; // Button to start the fight
    public bool isTeamReady = false; // Flag to check if the team is ready

    private void Start()
    {
        // Initialize the team slots
        teamSlots = new List<TeamSlot>(FindObjectsOfType<TeamSlot>());

        foreach (TeamSlot slot in teamSlots)
        {
            // Subscribe to the onSlotOccupied event for each team slot
            slot.onSlotOccupied += isSlotReady;
            slot.onSlotUnoccupied += isSlotReady;
        }

        // Ensure we have a TeamTransferManager in the scene
        if (TeamTransferManager.Instance == null)
        {
            GameObject transferManager = new GameObject("TeamTransferManager");
            transferManager.AddComponent<TeamTransferManager>();
        }

        TeamStatusCheck(); // Check if the team is ready
    }

    public void isSlotReady(TeamSlot slot)
    {
        slot.CheckAndNotify(); // Check if the slot is occupied
        if (slot.isoccupied)
        {
            Debug.Log("Slot is occupied: " + slot.name);
        }
        else
        {
            Debug.Log("Slot is unoccupied: " + slot.name);
        }
        TeamStatusCheck(); // Check the team status
    }

    // This method checks if at least 1 team slot is occupied
    public bool IsTeamReady()
    {
        foreach (TeamSlot slot in teamSlots)
        {
            slot.CheckAndNotify();
            if (slot.IsOccupied())
            {
                return true; // At least one slot is occupied
            }
        }
        return false; // No slots are occupied
    }

    // This method enables the start button and makes it opaque if the team is ready and semi-transparent if not
    public void TeamStatusCheck()
    {
        isTeamReady = IsTeamReady(); // Check if the team is ready
        Image startButtonImage = startButton.GetComponent<Image>();
        if (isTeamReady)
        {
            startButton.interactable = true;
            startButtonImage.color = new Color(startButtonImage.color.r, startButtonImage.color.g, startButtonImage.color.b, 1f);
            Debug.Log("Team is ready");
        }
        else
        {
            Debug.Log("Team is not ready");
            startButton.interactable = false;
            startButtonImage.color = new Color(startButtonImage.color.r, startButtonImage.color.g, startButtonImage.color.b, .075f);
        }
    }

    public void StartFight()
    {
        if (isTeamReady)
        {
            Debug.Log("Starting fight...");

            // Clear any previously selected characters
            TeamTransferManager.Instance.ClearTeam();

            // Store the selected characters and their lanes in the TeamTransferManager
            foreach (TeamSlot slot in teamSlots)
            {
                if (slot.IsOccupied())
                {
                    RosterCharacter rosterCharacter = slot.transform.GetComponentInChildren<RosterCharacter>();
                    if (rosterCharacter != null && rosterCharacter.characterStats != null)
                    {
                        TeamTransferManager.Instance.AddCharacterToTeam(
                            rosterCharacter.characterStats,
                            slot.laneID
                        );

                        Debug.Log($"Added {rosterCharacter.characterStats.CharacterName} to transfer list in lane {slot.laneID}");
                    }
                }
            }

            // Load the combat scene
            StartCoroutine(WaitForSceneLoad("TestCombat"));
        }
        else
        {
            Debug.Log("Team is not ready to start the fight.");
        }
    }

    IEnumerator WaitForSceneLoad(string sceneName)
    {
        Debug.Log("Loading scene: " + sceneName);
        // Wait for the scene to load
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            Debug.Log("Loading...");
            yield return null; // Wait for the next frame
        }

        // Set the new scene as active
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newScene);

        // Unload the previous scene
        SceneManager.UnloadSceneAsync("Pre-battle Menu");
    }
}