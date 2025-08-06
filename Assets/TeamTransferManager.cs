using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamTransferManager : MonoBehaviour
{
    // Singleton instance
    public static TeamTransferManager Instance { get; private set; }

    // Lists to store selected player characters and their lanes
    private List<CharacterDefinition> selectedCharacters = new List<CharacterDefinition>();
    private List<LanesManager.LaneID> selectedLanes = new List<LanesManager.LaneID>();

    // Lists to store enemy characters and their lanes
  [SerializeField]  private List<CharacterDefinition> enemyCharacters = new List<CharacterDefinition>();
  [SerializeField]  private List<LanesManager.LaneID> enemyLanes = new List<LanesManager.LaneID>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Methods for player team management
    public void AddCharacterToTeam(CharacterDefinition character, LanesManager.LaneID laneID)
    {
        selectedCharacters.Add(character);
        selectedLanes.Add(laneID);
        Debug.Log($"Added {character.CharacterName} to player team in lane {laneID}");
    }

    public void RemoveCharacterFromTeam(int index)
    {
        if (index >= 0 && index < selectedCharacters.Count)
        {
            Debug.Log($"Removed {selectedCharacters[index].CharacterName} from player team");
            selectedCharacters.RemoveAt(index);
            selectedLanes.RemoveAt(index);
        }
    }

    [Header("Enemy Team Setup")]

    [SerializeField] private TMP_Dropdown dropdown; // Reference to the dropdown UI element

    [SerializeField] private List<EnemyEncounter> enemyEncounters; // List of enemy encounters
    public void SetEnemyTeamFromDropdown(int index)
    {
        Debug.Log($"SetEnemyTeamFromDropdown called with index: {index}");

        // Get the selected index from the dropdown
        int selectedIndex = dropdown.value; // Assuming you have a dropdown component
        selectedIndex = index;
        Debug.Log($"Dropdown selected index: {selectedIndex}");

        // Validate the selected index
        if (selectedIndex < 0 || selectedIndex >= enemyEncounters.Count)
        {
            Debug.LogError($"Invalid dropdown selection for enemy team! Index: {selectedIndex}, EnemyEncounters Count: {enemyEncounters.Count}");
            return;
        }

        // Get the selected enemy encounter
        EnemyEncounter selectedEncounter = enemyEncounters[selectedIndex];
        Debug.Log($"Selected enemy encounter: {selectedEncounter.encounterName}");

        // Clear the current enemy team
        Debug.Log("Clearing current enemy team...");
        ClearEnemyTeam();

        // Add enemies from the selected encounter to the enemy team
        for (int i = 0; i < selectedEncounter.enemies.Count; i++)
        {
            CharacterDefinition enemyCharacter = selectedEncounter.enemies[i].enemyDefinition;
            LanesManager.LaneID enemyLane = selectedEncounter.enemies[i].laneID;

            if (enemyCharacter != null)
            {
                Debug.Log($"Adding enemy character: {enemyCharacter.CharacterName} to lane: {enemyLane}");
                AddCharacterToEnemyTeam(enemyCharacter, enemyLane);
            }
            else
            {
                Debug.LogWarning($"Enemy character at index {i} in encounter {selectedEncounter.encounterName} is null.");
            }
        }

        Debug.Log($"Enemy team successfully set from dropdown selection: {selectedEncounter.encounterName}");
    }

    public void ClearTeam()
    {
        selectedCharacters.Clear();
        selectedLanes.Clear();
        Debug.Log("Cleared player team");
    }

    public List<CharacterDefinition> GetSelectedCharacters()
    {
        return selectedCharacters;
    }

    public List<LanesManager.LaneID> GetSelectedLanes()
    {
        return selectedLanes;
    }

    // Methods for enemy team management
    public void AddCharacterToEnemyTeam(CharacterDefinition character, LanesManager.LaneID laneID)
    {
        enemyCharacters.Add(character);
        enemyLanes.Add(laneID);
        Debug.Log($"Added {character.CharacterName} to enemy team in lane {laneID}");
    }

    public void RemoveCharacterFromEnemyTeam(int index)
    {
        if (index >= 0 && index < enemyCharacters.Count)
        {
            Debug.Log($"Removed {enemyCharacters[index].CharacterName} from enemy team");
            enemyCharacters.RemoveAt(index);
            enemyLanes.RemoveAt(index);
        }
    }

    public void ClearEnemyTeam()
    {
        enemyCharacters.Clear();
        enemyLanes.Clear();
        Debug.Log("Cleared enemy team");
    }

    public List<CharacterDefinition> GetEnemyCharacters()
    {
        return enemyCharacters;
    }

    public List<LanesManager.LaneID> GetEnemyLanes()
    {
        return enemyLanes;
    }

    // Utility method to set a predefined enemy team (useful for encounters)
    public void SetEnemyTeam(List<CharacterDefinition> enemies, List<LanesManager.LaneID> lanes)
    {
        if (enemies.Count > lanes.Count)
        {
            Debug.LogError("Enemy team setup failed: Number of enemies does not match number of lanes!");
            return;
        }

        ClearEnemyTeam();

        for (int i = 0; i < enemies.Count; i++)
        {
            AddCharacterToEnemyTeam(enemies[i], lanes[i]);
        }

        Debug.Log($"Set up enemy team with {enemies.Count} characters");
    }
}