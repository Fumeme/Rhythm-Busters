using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NodeHolder : MonoBehaviour
{
    public float speed = 5f; // Holder movement speed
    public float waitTimeBeforeDisappear = 0.75f; // Time to wait if holder is missed

    public static event Action<NodeHolder> NodeHolderHitEvent;

    [SerializeField] private bool isActive = true;
    [SerializeField] private bool isActivated = false;
    private List<Node> childNodes = new List<Node>();
    private LanesManager lanesManager;

    private void OnEnable()
    {
       // Debug.Log($"[NodeHolder] {gameObject.name} OnEnable called. isActive: {isActive}, isActivated: {isActivated}");
        this.isActive = true; this.isActivated = false; this.enabled = true;
    }
    private void Start()
    {
       // Debug.Log($"[NodeHolder] {gameObject.name} Awake: STARTING. Initial isActive: {isActive}, Initial isActivated: {isActivated}"); // L1 - This should appear

        lanesManager = LanesManager.Instance; // L2 - Potential NullReference if LanesManager.Instance is not set up correctly
       // Debug.Log($"[NodeHolder] {gameObject.name} Awake: LanesManager.Instance assigned. Is null? {lanesManager == null}"); // L3 - This tells us if LanesManager is the problem



      //  Debug.Log($"[NodeHolder] {gameObject.name} Awake: Before isActive/isActivated check. isActive: {isActive}, isActivated: {isActivated}"); // L6

        if (isActive) { this.enabled = true; isActivated = false; } // L7
       // Debug.Log($"[NodeHolder] {gameObject.name} Awake: After isActive/isActivated check. isActive: {isActive}, isActivated: {isActivated}"); // L8

        Vector3 currentPosition = transform.position;
        // L9 - Potential NullReference if lanesManager is null AND transform.position is accessed (though transform is always valid for a MonoBehaviour)
        if (lanesManager == null)
        {
          //  Debug.LogError($"[NodeHolder] {gameObject.name} ERROR: LanesManager is NULL in Awake! Cannot set X position. Ensure LanesManager is in the scene and initialized before NodeHolders.");
            // If the script stops here, it's likely a NullReference from an unhandled access to lanesManager later,
            // or if the game object itself gets destroyed by another script that depends on lanesManager.
        }
        else
        {
            transform.position = new Vector3(lanesManager.transform.position.x, currentPosition.y, currentPosition.z); // L10 - Needs lanesManager to not be null
          //  Debug.Log($"[NodeHolder] {gameObject.name} Awake: Set X position based on LanesManager. LanesManager position X: {lanesManager.transform.position.x}"); // L11
        }

       // Debug.Log($"[NodeHolder] {gameObject.name} Awake: Starting childNodes processing. Child count: {transform.childCount}"); // L12 - This is the last log before the loop
        foreach (Transform child in transform) // L13 - Loop starts here
        {
            Node node = child.GetComponent<Node>(); // L14
            if (node != null)
            {
                childNodes.Add(node); // L15
              //  Debug.Log($"[NodeHolder] {gameObject.name} Awake: Added child node: {node.name}. ChildNodes count: {childNodes.Count}"); // L16
                node.SetActive(false); // L19 - This calls Node's SetActive (assuming it's a method in Node.cs), if SetActive has issues, it could fail here.
                Vector3 localPos = child.localPosition;
                localPos.y = 0f;
                localPos.z = 0f;
                child.localPosition = localPos; // L20
               // Debug.Log($"[NodeHolder] {gameObject.name} Awake: Configured child node {node.name}."); // L21
            }
            else
            {
               // Debug.LogWarning($"[NodeHolder] {gameObject.name} Awake: Child '{child.name}' does not have a Node component."); // L22
            }
        }
       // Debug.Log($"[NodeHolder] {gameObject.name} Awake: Finished childNodes processing. Final childNodes count: {childNodes.Count}"); // L23

        if (childNodes.Count == 1) // L24
        {
            SpriteRenderer spriteRenderer = GetComponentsInChildren<SpriteRenderer>()[0]; // L25 - Potential error if no SpriteRenderer found, or if GetComponentsInChildren returns an empty array.
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color; // L26
                color.a = 0.25f; // L27
                spriteRenderer.color = color; // L28
               // Debug.Log($"[NodeHolder] {gameObject.name} Awake: Adjusted sprite color for single node."); // L29
            }
            else
            {
               // Debug.LogWarning($"[NodeHolder] {gameObject.name} Awake: No SpriteRenderer found on children for single node adjustment."); // L30
            }
        }
        else if (childNodes.Count < 1) // L31
        {
          //  Debug.LogWarning($"[NodeHolder] {gameObject.name} Awake: No child nodes found. Returning early from Awake."); // L32
            return; // No nodes to process, exit early
        }

        isActivated = false; // L33
        isActive = true;     // L34

      //  Debug.Log($"[NodeHolder] {gameObject.name} Awake: Nearing END. Final isActive: {isActive}, Final isActivated: {isActivated}"); // L35 - The target log
    }
    void FixedUpdate()
    {
        if (isActive)
        {
            transform.Translate(speed * Time.deltaTime * Vector3.down);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
       // Debug.Log($"[NodeHolder] {gameObject.name} OnTriggerEnter2D triggered by: {other.gameObject.name} (Tag: {other.tag}). Current isActive: {isActive}, Current isActivated: {isActivated}");

        if (!isActive || isActivated)
        {
           // Debug.Log($"[NodeHolder] {gameObject.name} OnTriggerEnter2D: Not active ({!isActive}) or already activated ({isActivated}). Skipping further processing.");
            return;
        }
        if (other.GetComponent<NodeHolder>() != null)
        {
           // Debug.Log($"[NodeHolder] {gameObject.name} Collided with another NodeHolder. Ignoring.");
            return;
        }

        if (other.CompareTag("Hitter") || other.CompareTag("Activator"))
        {
            //Debug.Log($"[NodeHolder] {gameObject.name} Hitter/Activator detected! Triggering Node Activation.");
            TriggerNodeActivation();
            return;
        }

    }

    private void TriggerNodeActivation()
    {

       // Debug.Log($"[NodeHolder] {gameObject.name} TriggerNodeActivation called for lane: {lane.laneID}. Setting isActivated to TRUE.");
        isActivated = true; // This is the line that caused subsequent triggers to be skipped, which is probably fine.

        NodeHolderHitEvent?.Invoke(this);
        //Debug.Log($"[NodeHolder] {gameObject.name} NodeHolderHitEvent invoked (if subscribers exist).");

        // *** ADD THIS LOG ***
       // Debug.Log($"[NodeHolder] {gameObject.name} TriggerNodeActivation: About to process {childNodes.Count} child nodes.");

        foreach (Node node in childNodes)
        {
            //Debug.Log($"[NodeHolder] {gameObject.name} TriggerNodeActivation: Processing node: {node.name} of type {node.type}."); // *** ADD THIS LOG ***
            ProcessNode(node);
        }

       // Debug.Log($"[NodeHolder] {gameObject.name} TriggerNodeActivation: Finished processing child nodes. Calling TriggerGlobalNodeEffects."); // *** ADD THIS LOG ***
        TriggerGlobalNodeEffects();

        if (GetComponent<ParticleSystem>() != null)
        {
            GetComponent<ParticleSystem>().Play();
            Debug.Log($"[NodeHolder] {gameObject.name} TriggerNodeActivation: Playing ParticleSystem."); // *** ADD THIS LOG ***
        }

        //Debug.Log($"[NodeHolder] {gameObject.name} TriggerNodeActivation: Destroying GameObject in 0.1s."); // *** ADD THIS LOG ***
        Destroy(gameObject, 1.5f);
        Destroy(GetComponent<Collider2D>()); // *** ADD THIS LOG ***
    }

    private void ProcessNode(Node node)
    {
       // Debug.Log($"[NodeHolder] {gameObject.name} Processing node of type {node.type} for lane {lane.laneID}.");

        if (LanesManager.Instance == null)
        {
            Debug.LogError("[NodeHolder] LanesManager.Instance is null during ProcessNode. Cannot get characters.");
            return;
        }

        Character playerCharacter = LanesManager.Instance.PlayerCharacters.Find(c => c.laneID == node.lane.laneID);
        Character enemyCharacter = LanesManager.Instance.EnemyCharacters.Find(c => c.laneID == node.lane.laneID);

       // Debug.Log($"[NodeHolder] {gameObject.name} ProcessNode: Retrieved characters for lane {lane.laneID}. Player: {(playerCharacter != null ? playerCharacter.stats.CharacterName : "None")}, Enemy: {(enemyCharacter != null ? enemyCharacter.stats.CharacterName : "None")}"); // *** ADD THIS LOG ***

        if (playerCharacter != null || enemyCharacter != null)
        {
           // Debug.Log($"[NodeHolder] {gameObject.name} ProcessNode: Characters found. Checking CanActivateNode."); // *** ADD THIS LOG ***
            bool playerCanActivate = playerCharacter != null && playerCharacter.CanActivateNode(node.type);
            bool enemyCanActivate = enemyCharacter != null && enemyCharacter.CanActivateNode(node.type);

            if (playerCanActivate || enemyCanActivate)
            {
                //Debug.Log($"[NodeHolder] {gameObject.name} ProcessNode: At least one character CAN activate. Calling DetermineNodeActivator."); // *** ADD THIS LOG ***
                DetermineNodeActivator(node, playerCharacter, enemyCharacter);

                Destroy(node.gameObject, 0.5f); // Destroy the node after processing
            }
            else
            {
                Debug.Log($"[NodeHolder] {gameObject.name} ProcessNode: Neither player nor enemy character can activate node type {node.type} in lane {node.lane.laneID}.");
            }
        }
        else
        {
            Debug.Log($"[NodeHolder] {gameObject.name} ProcessNode: No characters found in lane {node.lane.laneID} for node type {node.type}. Skipping activation for this node."); // *** ADD THIS LOG ***
        }

    }

    private void DetermineNodeActivator(Node node, Character playerCharacter, Character enemyCharacter)
    {
       // Debug.Log($"[NodeHolder] {gameObject.name} DetermineNodeActivator called for node type: {node.type}");
        if (playerCharacter == null && enemyCharacter == null) return;

        bool playerCanActivate = playerCharacter != null && playerCharacter.CanActivateNode(node.type);
        bool enemyCanActivate = enemyCharacter != null && enemyCharacter.CanActivateNode(node.type);

        if (playerCanActivate && !enemyCanActivate)
        {
            //Debug.Log($"[NodeHolder] {gameObject.name} Only Player can activate. Activating for {playerCharacter.stats.CharacterName}.");
            node.ActivateNodeForCharacter(playerCharacter);
            return;
        }
        else if (!playerCanActivate && enemyCanActivate)
        {
           // Debug.Log($"[NodeHolder] {gameObject.name} Only Enemy can activate. Activating for {enemyCharacter.stats.CharacterName}.");
            node.ActivateNodeForCharacter(enemyCharacter);
            return;
        }
        else if (playerCanActivate && enemyCanActivate)
        {
           // Debug.Log($"[NodeHolder] {gameObject.name} Both Player and Enemy can activate. Comparing stats.");
            // Ensure lane is not null before accessing its mainStat
            float playerStat = node.lane.mainStat == Lane.MainStat.Arcane ? playerCharacter.stats.Arcane : playerCharacter.stats.Might;
            float enemyStat = node.lane.mainStat == Lane.MainStat.Arcane ? enemyCharacter.stats.Arcane : enemyCharacter.stats.Might;

            float higherStat = Mathf.Max(playerStat, enemyStat);
            float lowerStat = Mathf.Min(playerStat, enemyStat);

            Character favoredCharacter = playerStat > enemyStat ? playerCharacter : enemyCharacter;
            Character unfavoredCharacter = favoredCharacter == playerCharacter ? enemyCharacter : playerCharacter;

            float favoredAccuracy = favoredCharacter == playerCharacter ? favoredCharacter.stats.Accuracy : unfavoredCharacter.stats.Accuracy; // Fixed potential error here
            const uint accuracyScaling = 1250;

            float activationChance = (higherStat / lowerStat) * (favoredAccuracy / accuracyScaling);

            if (UnityEngine.Random.value <= activationChance)
            {
               // Debug.LogWarning($"[NodeHolder] {favoredCharacter.stats.CharacterName} has won the {node.type} node with the chance of {activationChance * 100.0}%");
                node.ActivateNodeForCharacter(favoredCharacter);
            }
            else
            {
               // Debug.LogWarning($"[NodeHolder] {unfavoredCharacter.stats.CharacterName} has won the {node.type} node because {favoredCharacter.stats.CharacterName} failed to get the node");
                node.ActivateNodeForCharacter(unfavoredCharacter);
            }
        }
        else
        {
            Debug.LogWarning("[NodeHolder] Neither character can activate this node (after initial check).");
        }
    }

    private void TriggerGlobalNodeEffects()
    {
       // Debug.Log("[NodeHolder] TriggerGlobalNodeEffects called. Checking LanesManager.Instance...");

        if (LanesManager.Instance == null)
        {
            Debug.LogError("[NodeHolder] ERROR: LanesManager.Instance is NULL! Cannot trigger global node effects.");
            return;
        }

      //  Debug.Log("[NodeHolder] LanesManager.Instance is NOT null. Attempting to access PlayerCharacters and EnemyCharacters.");

        // Trigger onNode for all characters in combat
        if (LanesManager.Instance.PlayerCharacters != null)
        {
            foreach (Character character in LanesManager.Instance.PlayerCharacters)
            {
                if (character != null)
                {
                    character.TriggerBeforeNode();
                    character.TriggerOnNode();
                }
                else
                {
                    Debug.LogWarning("[NodeHolder] A null character found in PlayerCharacters list.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[NodeHolder] LanesManager.Instance.PlayerCharacters list is NULL.");
        }


        if (LanesManager.Instance.EnemyCharacters != null)
        {
            foreach (Character character in LanesManager.Instance.EnemyCharacters)
            {
                if (character != null)
                {
                    character.TriggerBeforeNode();
                    character.TriggerOnNode();
                }
                else
                {
                    Debug.LogWarning("[NodeHolder] A null character found in EnemyCharacters list.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[NodeHolder] LanesManager.Instance.EnemyCharacters list is NULL.");
        }
    }
}