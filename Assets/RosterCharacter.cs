using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RosterCharacter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public LanesManager.LaneID laneID = LanesManager.LaneID.NONE; // Lane ID for the character
    public Sprite characterSprite;
    public CharacterDefinition characterStats;
    public string characterName;

    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private bool isDragging;

    public CharacterInfoPanel infoPanel;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalParent = transform.parent;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = characterStats != null;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root); // Move to top UI layer

        TeamSlot teamSlot = eventData.pointerEnter.GetComponent<TeamSlot>();
        if (teamSlot != null)
        {
            transform.SetParent(teamSlot.transform);
            transform.localPosition = Vector3.zero; // Reset position
            teamSlot.CheckAndNotify(); // Check if the slot is occupied

        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
            transform.position = eventData.position;
    }

    public TeamSlot occupiedSlot; // Reference to the occupied slot
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        // TeamSlot teamSlot = GetComponentInParent<TeamSlot>(); // Get the parent team slot

        // Return to original position if not dropped on an object with the team slot component
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<TeamSlot>() == null)
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero; // Reset position
            if (occupiedSlot != null)
            {
                occupiedSlot.CheckAndNotify(); // Check if the slot is occupied
                occupiedSlot = null; // Clear the reference to the occupied slot
                laneID = LanesManager.LaneID.NONE;

            }
        }
        else
        {
                TeamSlot teamSlot = eventData.pointerEnter.GetComponent<TeamSlot>();
            // If dropped on a team slot, set the parent to that slot
            if (teamSlot != null)
            {
                transform.SetParent(teamSlot.transform);
                transform.localPosition = Vector3.zero; // Reset position
                teamSlot.CheckAndNotify(); // Check if the slot is occupied
                occupiedSlot = teamSlot; // Store the occupied slot reference

                laneID = teamSlot.laneID; // Assign the lane ID from the team slot
            }
        }

      //  teamSlot.CheckAndNotify(); // Check if the slot is occupied
    }


    void Start()
    {
        if (characterStats == null)
        {
            return;
        }

        // Assign the sprite to the Image component's sprite property

        Image imageComponent = GetComponent<Image>();

        characterSprite = characterStats.CharacterSprite;
        if (imageComponent != null)
        {
            imageComponent.sprite = characterSprite;

        }
        else
        {
            Debug.LogError("CharacterSprite Image component is not assigned.");
        }

        characterName = characterStats.CharacterName;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Only show info if not dragging
        if (!isDragging)
        {
            ShowCharacterInfo();
        }
    }

    private void ShowCharacterInfo()
    {
        infoPanel.ShowInfo(characterStats);
    }

    
}
