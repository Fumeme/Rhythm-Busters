using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSceneInitializer : MonoBehaviour
{
    [SerializeField] private GameObject characterPrefab; // Prefab for the character in the combat scene
    [SerializeField] private GameObject enemyPrefab; // Prefab for enemy characters (can be same as character prefab)

    [Header("Enemy Team Configuration")]
    [Tooltip("Set to true to use manual enemy configuration instead of TeamTransferManager")]
    [SerializeField] private bool useManualEnemySetup = false;

    [Tooltip("Enemy character definitions to spawn when using manual setup")]
    [SerializeField] private List<CharacterDefinition> manualEnemyDefinitions;

    [Tooltip("Lane assignments for enemy characters when using manual setup")]
    [SerializeField] private List<LanesManager.LaneID> manualEnemyLanes;

    void Start()
    {
        StartCoroutine(InitializeAfterOneFrame());
    }

    IEnumerator InitializeAfterOneFrame()
    {
        // Wait for one frame to ensure everything is loaded
        yield return new WaitForFixedUpdate();

        // Initialize player team
        InitializePlayerTeam();

        // Initialize enemy team
        InitializeEnemyTeam();
    }

    private void InitializePlayerTeam()
    {
        // Check if TeamTransferManager exists
        if (TeamTransferManager.Instance == null)
        {
            Debug.LogWarning("TeamTransferManager not found! Using default player setup or skipping player setup.");
            return;
        }

        // Get the selected characters and their lanes
        List<CharacterDefinition> selectedCharacters = TeamTransferManager.Instance.GetSelectedCharacters();
        List<LanesManager.LaneID> selectedLanes = TeamTransferManager.Instance.GetSelectedLanes();

        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("No characters were selected in the team menu!");
            return;
        }

        // Make sure LanesManager is available
        if (LanesManager.Instance == null)
        {
            Debug.LogError("LanesManager not found in the scene!");
            return;
        }

        // Spawn the selected characters in their respective lanes
        for (int i = 0; i < selectedCharacters.Count; i++)
        {
            if (i >= selectedLanes.Count)
            {
                Debug.LogWarning($"No lane specified for character {i}. Skipping character.");
                continue;
            }

            SpawnCharacter(
                selectedCharacters[i],
                selectedLanes[i],
                Character.Affiliation.Player,
                characterPrefab
            );
        }
    }

    private void InitializeEnemyTeam()
    {
        // Make sure LanesManager is available
        if (LanesManager.Instance == null)
        {
            Debug.LogError("LanesManager not found in the scene!");
            return;
        }

        List<CharacterDefinition> enemyDefinitions;
        List<LanesManager.LaneID> enemyLanes;

        if (useManualEnemySetup)
        {
            // Use manually configured enemies from the inspector
            enemyDefinitions = manualEnemyDefinitions;
            enemyLanes = manualEnemyLanes;
        }
        else
        {
            // Use enemy data from TeamTransferManager if available
            if (TeamTransferManager.Instance == null)
            {
                Debug.LogWarning("TeamTransferManager not found for enemy setup! Consider enabling manual enemy setup.");
                return;
            }

            enemyDefinitions = TeamTransferManager.Instance.GetEnemyCharacters();
            enemyLanes = TeamTransferManager.Instance.GetEnemyLanes();
        }

        if (enemyDefinitions == null || enemyDefinitions.Count == 0)
        {
            Debug.LogWarning("No enemy characters defined!");
            return;
        }

        // Spawn the enemy characters in their respective lanes
        for (int i = 0; i < enemyDefinitions.Count; i++)
        {
            if (i >= enemyLanes.Count)
            {
                Debug.LogWarning($"No lane specified for enemy {i}. Skipping enemy.");
                continue;
            }

            SpawnCharacter(
                enemyDefinitions[i],
                enemyLanes[i],
                Character.Affiliation.Enemy,
                enemyPrefab != null ? enemyPrefab : characterPrefab
            );
        }
    }

    // Unified method to spawn a character with given parameters
    private void SpawnCharacter(CharacterDefinition charDef, LanesManager.LaneID laneID,
                               Character.Affiliation affiliation, GameObject prefab)
    {
        if (charDef == null || prefab == null)
        {
            Debug.LogError("Cannot spawn character: Character definition or prefab is null");
            return;
        }

        // Instantiate the character prefab
        GameObject charObj = Instantiate(prefab);

        // Get or add the Character component
        Character character = charObj.GetComponent<Character>();
        if (character == null)
        {
            character = charObj.AddComponent<Character>();
        }

        // Assign the character definition and lane
        character.CharacterDef = charDef;
        character.laneID = laneID;

        // Set affiliation
        character.stats.CharAffil = affiliation;

        // Setup the character
        character.CharacterSetup();

        // Initialize stats 
        character.InitStats();

        // Assign the character to its lane
        character.AssignLane(laneID);

        // Flip sprite if enemy
        if (affiliation == Character.Affiliation.Enemy)
        {
            SpriteRenderer sr = charObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.flipX = true;
            }
        }

        Debug.Log($"Initialized {affiliation} character {charDef.CharacterName} in lane {laneID}");
    }
}