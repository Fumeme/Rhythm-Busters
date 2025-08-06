using System;
using System.Collections.Generic;
using UnityEngine;
using static Character;
using Vector3 = UnityEngine.Vector3;

public class LanesManager : MonoBehaviour
{

    public Lane PlayerLane1;
    public Lane PlayerLane2;
    public Lane PlayerLane3;
    public Lane PlayerLane4;
    public Lane PlayerLane5;

    public Lane EnemyLane1;
    public Lane EnemyLane2;
    public Lane EnemyLane3;
    public Lane EnemyLane4;
    public Lane EnemyLane5;

    public Transform PLaneTransform1;
    public Transform PLaneTransform2;
    public Transform PLaneTransfrom3;
    public Transform PLaneTransform4;
    public Transform PLaneTransform5;

    public Transform ELaneTransform1;
    public Transform ELaneTransform2;
    public Transform ELaneTransform3;
    public Transform ELaneTransform4;
    public Transform ELaneTransform5;


    public Vector3 GetPos(LaneID laneID, Character.Affiliation affiliation)
    {
        if (affiliation == Affiliation.Player)
        {


            switch (laneID)
            {
                case LanesManager.LaneID.First:
                    return PLaneTransform1.position;
                case LanesManager.LaneID.Second:
                    return PLaneTransform2.position;
                case LanesManager.LaneID.Third:
                    return PLaneTransfrom3.position;
                case LanesManager.LaneID.Fourth:
                    return PLaneTransform4.position;
                case LanesManager.LaneID.Fifth:
                    return PLaneTransform5.position;

                default: break;
            }
        }
        else
        {

            switch (laneID)
            {
                case LanesManager.LaneID.First:
                    return ELaneTransform1.position;
                case LanesManager.LaneID.Second:
                    return ELaneTransform2.position;
                case LanesManager.LaneID.Third:
                    return ELaneTransform3.position;
                case LanesManager.LaneID.Fourth:
                    return ELaneTransform4.position;
                case LanesManager.LaneID.Fifth:
                    return ELaneTransform5.position;

                default: break;
            }

        }
        return PLaneTransform1.position;
    }

    public List<Character> PlayerCharacters = new List<Character>(); // List for player characters
    public List<Character> EnemyCharacters = new List<Character>(); // List for enemy characters

    public enum LaneID { NONE = -1,First, Second, Third, Fourth, Fifth }
    public Character GetCharacter(LaneID laneID, Affiliation affiliation)
    {
        List<Character> Team = new List<Character>();
        if (affiliation == Affiliation.Player)
        {
            Team.AddRange(PlayerCharacters);
            //Debug.Log($"Checking PlayerCharacters, Count: {PlayerCharacters.Count}");
        }
        if (affiliation == Affiliation.Enemy)
        {
            Team.AddRange(EnemyCharacters);
            //Debug.Log($"Checking EnemyCharacters, Count: {EnemyCharacters.Count}");
        }
        foreach (Character character in Team)
        {
            if (character != null)
            {
                if (character.laneID == LaneID.NONE)
                {
                    Debug.LogWarning($"Character {character.stats.CharacterName} has laneID NONE, skipping.");
                    continue; // Skip characters with laneID NONE
                }
                //Debug.Log($"Checking character in lane {character.laneID}");
                if (character.laneID == laneID)
                {
                   // Debug.Log($"Found character in lane {laneID}");
                    return character;
                }
            }
            else
            {
                Debug.LogWarning("Null character found in team list!");
            }
        }
        Debug.Log($"No character found for laneID {laneID} in {affiliation} team");
        return null;
    }

    public event Action<Character> OnAllyAdded;
    public event Action<Character> OnAllyRemoved;

    // Call these when modifying lists
    public void AddPlayerCharacter(Character character)
    {
        PlayerCharacters.Add(character);
        OnAllyAdded?.Invoke(character);
    }

    public void RemovePlayerCharacter(Character character)
    {
        PlayerCharacters.Remove(character);
        OnAllyRemoved?.Invoke(character);
    }


    public static LanesManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            EnemyCharacters.Clear();
            PlayerCharacters.Clear();
        }
        else Destroy(gameObject);


    }


    private Vector3 GetLanePosition(LaneID laneID, Character.Affiliation affiliation)
    {
        // Calculate position for the character based on lane and affiliation
        Vector3 basePosition = GetPos(laneID, affiliation); // Replace with lane-specific position logic
        basePosition.x *= affiliation == Character.Affiliation.Player ? 1 : -1;
        return basePosition;
    }
    public Lane GetLane(LaneID laneID, Affiliation affiliation)
    {
        if (affiliation == Affiliation.Player)
        {


            switch (laneID)
            {
                case LanesManager.LaneID.First:
                    return PlayerLane1;
                case LanesManager.LaneID.Second:
                    return PlayerLane2;
                case LanesManager.LaneID.Third:
                    return PlayerLane3;
                case LanesManager.LaneID.Fourth:
                    return PlayerLane4;
                case LanesManager.LaneID.Fifth:
                    return PlayerLane5;

                default: 
                    Debug.LogError($"failed to get lane for {laneID}, {affiliation}");
                    break;
            }
        }
        else
        {

            switch (laneID)
            {
                case LanesManager.LaneID.First:
                    return EnemyLane1;
                case LanesManager.LaneID.Second:
                    return EnemyLane2;
                case LanesManager.LaneID.Third:
                    return EnemyLane3;
                case LanesManager.LaneID.Fourth:
                    return EnemyLane4;
                case LanesManager.LaneID.Fifth:
                    return EnemyLane5;

                default:
                    Debug.LogError($"failed to get lane for {laneID}, {affiliation}");

                    break;
            }

        }
        Debug.LogError($"using default player lane 1, {PlayerLane1}");
        return PlayerLane1;
    }

    public List<Lane> GetLanesByAffiliation(Affiliation enemyAffil)
    {
        List<Lane> result = new List<Lane>();
        if (enemyAffil == Affiliation.Enemy)
        {
            result = new List<Lane> { EnemyLane1, EnemyLane2, EnemyLane3, EnemyLane4, EnemyLane5 };
        }
        else
        {
            result = new List<Lane> { PlayerLane1, PlayerLane2, PlayerLane3, PlayerLane4, PlayerLane5 };
        }

        //Debug.Log($"Looking for lanes with affiliation: {enemyAffil}, Found: {result.Count} lanes.");
        return result;
    }

}
