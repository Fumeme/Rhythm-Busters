using System.Collections;
using UnityEngine;
using System;
using static Lane;


public class Node : MonoBehaviour
{
    public enum NodeType
    {
        Attack,
        Rest,
        Act,
        Ult
    }

    public NodeType type;
    public Lane lane;

    private SpriteRenderer spriteRenderer;
    private bool isActive = true;

    // This event is still used for backward compatibility
    public static event Action<Node> NodeHitEvent;

    private bool isActivated = false;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get the NodeLayoutManager to access color settings
        NodeLayoutManager layoutManager = FindObjectOfType<NodeLayoutManager>();
        if (layoutManager != null)
        {
            // Apply the appropriate color from the layout manager
            ApplyColorFromManager(layoutManager);
        }
        else
        {
            // Fallback colors if no manager is found
            NodeLayoutCreator creator = FindObjectOfType<NodeLayoutCreator>();
            if (creator != null)
            {
                ApplyColorFromCreator(creator);
            }
            else
            {
                // Default colors if neither is found
                ApplyDefaultColor();
            }
        }
    }

    private void ApplyColorFromManager(NodeLayoutManager manager)
    {
        if (spriteRenderer != null)
        {
            switch (type)
            {
                case NodeType.Attack:
                    spriteRenderer.color = manager.attackNodeColor;
                    break;
                case NodeType.Rest:
                    spriteRenderer.color = manager.restNodeColor;
                    break;
                case NodeType.Act:
                    spriteRenderer.color = manager.actNodeColor;
                    break;
                case NodeType.Ult:
                    spriteRenderer.color = manager.ultNodeColor;
                    break;
                default:
                    spriteRenderer.color = Color.gray;
                    break;
            }
        }
    }

    private void ApplyColorFromCreator(NodeLayoutCreator creator)
    {
        if (spriteRenderer != null)
        {
            switch (type)
            {
                case NodeType.Attack:
                    spriteRenderer.color = creator.attackNodeColor;
                    break;
                case NodeType.Rest:
                    spriteRenderer.color = creator.restNodeColor;
                    break;
                case NodeType.Act:
                    spriteRenderer.color = creator.actNodeColor;
                    break;
                case NodeType.Ult:
                    spriteRenderer.color = creator.ultNodeColor;
                    break;
                default:
                    spriteRenderer.color = Color.gray;
                    break;
            }
        }
    }

    private void ApplyDefaultColor()
    {
        if (spriteRenderer != null)
        {
            switch (type)
            {
                case NodeType.Attack:
                    spriteRenderer.color = Color.red;
                    break;
                case NodeType.Rest:
                    spriteRenderer.color = Color.green;
                    break;
                case NodeType.Act:
                    spriteRenderer.color = Color.blue;
                    break;
                case NodeType.Ult:
                    spriteRenderer.color = Color.yellow;
                    break;
                default:
                    spriteRenderer.color = Color.gray;
                    break;
            }
        }
    }

    public void ActivateNodeForCharacter(Character character)
    {
        Debug.Log(character.stats.CharacterName + " is trying to activate node of type " + type);
        if (isActivated)
        {
            Debug.LogWarning($"{character.stats.CharacterName} tried to activate {type} node, but it is already activated.");
            return; // Prevent double activation
        }
        Debug.Log($"{character.stats.CharacterName} is activating the {type} node on lane {lane.GetID()}");
        TriggerAction(character);
        Debug.Log($"{character.stats.CharacterName} used the {type} node");
        isActivated = true;
    }

    private void TriggerAction(Character character)
    {
        Debug.Log($"Node of {type} on lane {lane.GetID()} has been triggered!");
        character.TriggerBeforeNode();

        switch (type)
        {
            case NodeType.Attack:
                Debug.Log($"{character.stats.CharacterName} is performing an attack action on node of type {type} on lane {lane.laneID}");
                character.TriggerBeforeStrike();
                character.GetComponent<AutoAttack>().StartCoroutine(
                    character.GetComponent<AutoAttack>().PerformAttack());
                character.TriggerOnStrike();
                break;
            case NodeType.Rest:
                Debug.Log($"{character.stats.CharacterName} is performing a rest action on node of type {type} on lane {lane.laneID}");
                character.TriggerBeforeRest();
                character.Heal(25 + character.stats.Arcane * 0.05f);
                character.RecoverStamina(5 + character.stats.Arcane * 0.025f);
                character.TriggerOnRest();
                break;
            case NodeType.Act:
                Debug.Log($"{character.stats.CharacterName} is performing an act action on node of type {type} on lane {lane.laneID}");
                character.TriggerBeforeAct();
                character.TriggerOnAct();
                break;
            case NodeType.Ult:
                Debug.Log($"{character.stats.CharacterName} is performing an ultimate action on node of type {type} on lane {lane.laneID}");
                character.TriggerBeforeUlt();
                character.TriggerOnUlt();
                break;
            default:
                Debug.LogWarning($"Node type {type} is not recognized. No action will be performed.");
                break;

        }
        Debug.LogError($"{character.stats.CharacterName} has triggered on node");

        // Note: We don't call TriggerOnNode() here as it's now handled by the NodeHolder for all characters

        if (character.GetComponent<CharacterParticlesFX>() != null)
        {
            character.GetComponent<CharacterParticlesFX>().NodeFX(transform.position);
        }
    }


    public void SetActive(bool active)
    {
        isActive = active;
    }
}