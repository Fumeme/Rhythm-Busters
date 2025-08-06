using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Encounter", menuName = "Game/Enemy Encounter")]
public class EnemyEncounter : ScriptableObject
{
    [System.Serializable]
    public class EnemyPosition
    {
        public CharacterDefinition enemyDefinition;
        public LanesManager.LaneID laneID;
    }

    public string encounterName;
    [TextArea(2, 5)]
    public string encounterDescription;

    [Header("Enemy Setup")]
    public List<EnemyPosition> enemies = new List<EnemyPosition>();

    // Method to apply this encounter to the TeamTransferManager
    public void ApplyEncounter()
    {
        if (TeamTransferManager.Instance == null)
        {
            Debug.LogError("Cannot apply encounter: TeamTransferManager not found!");
            return;
        }

        List<CharacterDefinition> enemyDefs = new List<CharacterDefinition>();
        List<LanesManager.LaneID> enemyLanes = new List<LanesManager.LaneID>();

        foreach (EnemyPosition enemy in enemies)
        {
            if (enemy.enemyDefinition != null)
            {
                enemyDefs.Add(enemy.enemyDefinition);
                enemyLanes.Add(enemy.laneID);
            }
        }

        TeamTransferManager.Instance.SetEnemyTeam(enemyDefs, enemyLanes);
        Debug.Log($"Applied enemy encounter: {encounterName}");
    }
}