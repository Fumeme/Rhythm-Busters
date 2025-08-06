
using System;
using UnityEngine;

public class TeamSlot : MonoBehaviour
{
    //this event is called when this slot is occupied
    public event Action<TeamSlot> onSlotOccupied;
    //this event is called when this slot becomes unoccupied
    public event Action<TeamSlot> onSlotUnoccupied;
    public LanesManager.LaneID laneID; // Lane ID for the character


    public bool isoccupied = false; // Flag to check if the slot is occupied
    public bool IsOccupied()
    {
        if (transform.childCount == 0)
        {
            isoccupied = false; // Slot is empty
        }
        else
        {
            Transform character = transform.GetChild(0); // Get the character in the slot
            RosterCharacter rosterCharacter = character.TryGetComponent<RosterCharacter>(out RosterCharacter characterComponent) ? characterComponent : null; // Get the RosterCharacter component
            isoccupied = characterComponent != null; // Slot is occupied if a character is present
        }
        return isoccupied;
    }

    public bool CheckAndNotify()
    {
        bool wasOccupied = isoccupied;
        bool currentlyOccupied = IsOccupied();

        if (currentlyOccupied && !wasOccupied)
        {
            onSlotOccupied?.Invoke(this); // Notify that the slot is occupied
            return true; // Slot is occupied
        }
        else if (!currentlyOccupied && wasOccupied)
        {
            onSlotUnoccupied?.Invoke(this); // Notify that the slot is unoccupied
            return false; // Slot is unoccupied
        }

        return currentlyOccupied;
    }


}
