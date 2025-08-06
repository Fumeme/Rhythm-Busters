using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySetupUtility : MonoBehaviour
{
    [SerializeField] private EnemyEncounter defaultEncounter;
    [SerializeField] private List<EnemyEncounter> availableEncounters = new List<EnemyEncounter>();

    [Header("UI Elements")]
    [SerializeField] private Dropdown encounterDropdown;
    [SerializeField] private Button applyButton;
    [SerializeField] private Text encounterDescription;

    private void Start()
    {
        // Initialize dropdown with available encounters
        if (encounterDropdown != null)
        {
            encounterDropdown.ClearOptions();

            List<string> encounterNames = new List<string>();
            foreach (EnemyEncounter encounter in availableEncounters)
            {
                encounterNames.Add(encounter.encounterName);
            }

            encounterDropdown.AddOptions(encounterNames);
            encounterDropdown.onValueChanged.AddListener(OnEncounterSelected);
        }

        // Set up button listener
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplySelectedEncounter);
        }

        // Apply default encounter if specified
        if (defaultEncounter != null)
        {
            defaultEncounter.ApplyEncounter();
            UpdateEncounterDescription(defaultEncounter);
        }
    }

    private void OnEncounterSelected(int index)
    {
        if (index >= 0 && index < availableEncounters.Count)
        {
            UpdateEncounterDescription(availableEncounters[index]);
        }
    }

    private void UpdateEncounterDescription(EnemyEncounter encounter)
    {
        if (encounterDescription != null && encounter != null)
        {
            encounterDescription.text = encounter.encounterDescription;

            // Display enemy composition
            string enemyList = "\n\nEnemies:";
            foreach (EnemyEncounter.EnemyPosition enemy in encounter.enemies)
            {
                if (enemy.enemyDefinition != null)
                {
                    enemyList += $"\n• {enemy.enemyDefinition.CharacterName} (Lane {enemy.laneID})";
                }
            }

            encounterDescription.text += enemyList;
        }
    }

    private void ApplySelectedEncounter()
    {
        int selectedIndex = encounterDropdown.value;
        if (selectedIndex >= 0 && selectedIndex < availableEncounters.Count)
        {
            availableEncounters[selectedIndex].ApplyEncounter();
            Debug.Log($"Applied encounter: {availableEncounters[selectedIndex].encounterName}");
        }
    }

    // For programmatic use (e.g., from other scripts)
    public void ApplyEncounter(EnemyEncounter encounter)
    {
        if (encounter != null)
        {
            encounter.ApplyEncounter();
            UpdateEncounterDescription(encounter);
        }
    }

    // Simple method to create and apply an enemy encounter at runtime
    public void CreateAndApplyEncounter(List<CharacterDefinition> enemies, List<LanesManager.LaneID> lanes)
    {
        if (TeamTransferManager.Instance == null)
        {
            Debug.LogError("TeamTransferManager not found!");
            return;
        }

        TeamTransferManager.Instance.SetEnemyTeam(enemies, lanes);
    }
}