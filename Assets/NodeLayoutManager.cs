using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LanesManager;

/// <summary>
/// Manages the creation and spawning of custom node layouts.
/// A node layout is a pattern of nodes that can be spawned across different lanes.
/// </summary>
public class NodeLayoutManager : MonoBehaviour
{
    [Serializable]
    public class NodeDefinition
    {
        public Node.NodeType nodeType;
        public LaneID laneID;
        public float verticalOffset; // Offset from the base position in the pattern (in local space)
    }

    [Serializable]
    public class NodePattern
    {
        public string patternName;
        public List<NodeDefinition> nodeDefinitions = new List<NodeDefinition>();
        public float patternDuration = 5f; // How long this pattern lasts before the next one starts
    }

    [Header("Node Setup")]
    public GameObject nodeHolderPrefab;
    public GameObject nodePrefab;

    [Header("Spawn Settings")]
    public float initialDelay = 2f;
    public float baseSpawnHeight = 10f; // Height above the screen where nodes spawn
    public float nodeSpeed = 5f;
    [Tooltip("Should match the row height in NodeLayoutCreator")]
    public float nodeHolderSpacing = 2f; // Vertical spacing between node holders

    [Header("Pattern Settings")]
    public List<NodePattern> availablePatterns = new List<NodePattern>();
    public bool randomizePatterns = false;
    public int currentPatternIndex = 0;

    [Header("Lane Settings")]
    [Tooltip("Should match the laneSpacing in NodeLayoutCreator")]
    public float laneSpacing = 2f; // Horizontal spacing between lanes

    [Header("Node Colors")]
    public Color attackNodeColor = Color.blue;
    public Color restNodeColor = Color.green;
    public Color actNodeColor = Color.red;
    public Color ultNodeColor = Color.yellow;

    private LanesManager lanesManager;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private NodeLayoutCreator layoutCreator;



    private void Start()
    {
        lanesManager = LanesManager.Instance;
        if (lanesManager == null)
        {
            Debug.LogError("LanesManager is not found. Please make sure it exists in the scene.");
        }

        // Find the layout creator to access its settings
        layoutCreator = FindObjectOfType<NodeLayoutCreator>();
        if (layoutCreator != null)
        {
            // Copy color settings from the layout creator
            attackNodeColor = layoutCreator.attackNodeColor;
            restNodeColor = layoutCreator.restNodeColor;
            actNodeColor = layoutCreator.actNodeColor;
            ultNodeColor = layoutCreator.ultNodeColor;

            // Copy spacing settings from layout creator
            laneSpacing = layoutCreator.laneSpacing;
            // Calculate node holder spacing based on grid height and row count
            nodeHolderSpacing = layoutCreator.gridHeight / (layoutCreator.gridRowCount - 1);
        }
        ValidatePatterns();
    }


    /// <summary>
    /// Validates all patterns to ensure they reference valid lanes.
    /// </summary>
    private void ValidatePatterns()
    {
        foreach (NodePattern pattern in availablePatterns)
        {
            for (int i = pattern.nodeDefinitions.Count - 1; i >= 0; i--)
            {
                NodeDefinition nodeDef = pattern.nodeDefinitions[i];
                if (nodeDef.laneID == LaneID.NONE)
                {
                    Debug.LogWarning($"Invalid lane ID found in pattern '{pattern.patternName}'. Node will be removed.");
                    pattern.nodeDefinitions.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// Starts spawning nodes using the defined patterns.
    /// </summary>
    public void StartNodeSpawning()
    {
        if (isSpawning)
        {
            StopNodeSpawning();
        }

        if (availablePatterns.Count == 0)
        {
            Debug.LogError("No patterns defined. Cannot start spawning.");
            return;
        }

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnPatternSequence());
    }

    /// <summary>
    /// Stops the node spawning process.
    /// </summary>
    public void StopNodeSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        isSpawning = false;
    }

    /// <summary>
    /// Main coroutine that handles spawning patterns in sequence.
    /// </summary>
    private IEnumerator SpawnPatternSequence()
    {
        yield return new WaitForSeconds(initialDelay);

        while (isSpawning)
        {
            // Get the current pattern to spawn
            NodePattern currentPattern = GetNextPattern();

            if (currentPattern != null && currentPattern.nodeDefinitions.Count > 0)
            {
                yield return StartCoroutine(SpawnPattern(currentPattern));

                // Wait for the pattern duration before spawning the next pattern
                yield return new WaitForSeconds(currentPattern.patternDuration);
            }
            else
            {
                yield return new WaitForSeconds(2f); // Default wait if pattern is invalid
            }
        }
    }

    /// <summary>
    /// Gets the next pattern to spawn based on settings.
    /// </summary>
    private NodePattern GetNextPattern()
    {
        if (availablePatterns.Count == 0) return null;

        if (randomizePatterns)
        {
            return availablePatterns[UnityEngine.Random.Range(0, availablePatterns.Count)];
        }
        else
        {
            NodePattern pattern = availablePatterns[currentPatternIndex];
            currentPatternIndex = (currentPatternIndex + 1) % availablePatterns.Count;
            return pattern;
        }
    }

    /// <summary>
    /// Spawns a single pattern of nodes.
    /// </summary>
    private IEnumerator SpawnPattern(NodePattern pattern)
    {
        // Group nodes by vertical offset to create node holders
        Dictionary<float, List<NodeDefinition>> holderGroups = new Dictionary<float, List<NodeDefinition>>();

        foreach (NodeDefinition nodeDef in pattern.nodeDefinitions)
        {
            if (!holderGroups.ContainsKey(nodeDef.verticalOffset))
            {
                holderGroups[nodeDef.verticalOffset] = new List<NodeDefinition>();
            }
            holderGroups[nodeDef.verticalOffset].Add(nodeDef);
        }

        // Sort offsets to spawn holders in sequence from top to bottom
        List<float> sortedOffsets = new List<float>(holderGroups.Keys);
        sortedOffsets.Sort();

        // Track the previous vertical offset for spacing calculations
        float previousOffset = sortedOffsets.Count > 0 ? sortedOffsets[0] : 0; // Initialize with first offset or 0 if no nodes
        // Spawn each group as a separate NodeHolder
        for (int i = 0; i < sortedOffsets.Count; i++)
        {
            float currentOffset = sortedOffsets[i];

            // If it's not the very first holder, calculate the gap and spawn empty holders
            if (i > 0)
            {
                float gap = currentOffset - previousOffset;

                // Determine how many 'rows' (nodeHolderSpacing units) this gap represents
                // Use a small epsilon to account for floating point inaccuracies
                int emptyRowsToSpawn = Mathf.RoundToInt(gap / nodeHolderSpacing) - 1;

                for (int j = 0; j < emptyRowsToSpawn; j++)
                {
                    // Calculate the vertical position for this empty holder
                    float emptyHolderVerticalPos = previousOffset + (nodeHolderSpacing * (j + 1));
                    SpawnEmptyNodeHolder(baseSpawnHeight + emptyHolderVerticalPos);
                    // Wait for an amount of time that corresponds to one 'row' of movement
                    yield return new WaitForSeconds(nodeHolderSpacing / nodeSpeed);
                }
            }

            // Spawn the actual node holder with nodes
            SpawnNodeHolder(holderGroups[currentOffset], baseSpawnHeight + currentOffset);

            // Update previous offset for next iteration
            previousOffset = currentOffset;

            // Wait for the holder to spawn and move down for one 'row' unit
            yield return new WaitForSeconds(nodeHolderSpacing / nodeSpeed);
        }
    }

    /// <summary>
    /// Spawns an empty NodeHolder with no child nodes.
    /// </summary>
    private void SpawnEmptyNodeHolder(float spawnHeight)
    {
        // Create holder at the center of the screen
        GameObject holderObj = Instantiate(nodeHolderPrefab, new Vector3(0, spawnHeight, 0), Quaternion.identity);
        NodeHolder holder = holderObj.GetComponent<NodeHolder>();

        if (holder != null)
        {
            holder.speed = nodeSpeed;
            // Make the empty holder visually distinct
            SpriteRenderer spriteRenderer = holderObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0.15f;  // Very transparent
                spriteRenderer.color = color;
            }
        }
    }



    private void SpawnNodeHolder(List<NodeDefinition> nodeDefinitions, float spawnHeight)
    {
        // Create holder at the center of the screen
        GameObject holderObj = Instantiate(nodeHolderPrefab, new Vector3(0, spawnHeight, 0), Quaternion.identity);
        NodeHolder holder = holderObj != null ? holderObj.GetComponent<NodeHolder>() : null;

        if (holder != null)
        {
            holder.speed = nodeSpeed;

            // Create each node as a child of the holder
            foreach (NodeDefinition nodeDef in nodeDefinitions)
            {
                if (nodePrefab == null)
                {
                    Debug.LogError("nodePrefab is not assigned in NodeLayoutManager.");
                    continue;
                }

                GameObject nodeObj = Instantiate(nodePrefab, holderObj.transform);
                Node node = nodeObj != null ? nodeObj.GetComponent<Node>() : null;

                if (node != null)
                {
                    // Configure the node
                    node.type = nodeDef.nodeType;

                    // This part sets the individual node's lane, which is good.
                    // The issue was the holder.lane itself.
                    if (lanesManager != null)
                    {
                        node.lane = lanesManager.GetLane(nodeDef.laneID, Character.Affiliation.Player);
                       // Debug.Log($"[NodeLayoutManager] Assigned Node ({nodeObj.name}) to lane: {node.lane?.laneID} for node type: {nodeDef.nodeType}.");
                    }
                    else
                    {
                        // This else block had a duplicate error log. Correcting.
                        Debug.LogError("LanesManager is not found. Node will not be assigned to a lane.");
                        // The next line was causing NRE if LanesManager.Instance was null and node.lane was used.
                        // It's better to ensure LanesManager.Instance is valid before this part.
                        // For now, removing the problematic line that uses LanesManager.Instance again.
                        // If node.lane remains null, subsequent issues will arise, but that's a different fix.
                    }

                    if (node.lane == null) // This check should be sufficient after the above assignment attempt
                    {
                        Debug.LogError($"Lane {nodeDef.laneID} not found for node type {nodeDef.nodeType}. Node will not be assigned to a lane.");
                    }

                   // Debug.Log($"[NodeLayoutManager] Spawning node of type {nodeDef.nodeType} on lane {nodeDef.laneID} at vertical offset {nodeDef.verticalOffset}.");
                    // Position the node horizontally based on lane
                    PositionNodeBasedOnLane(nodeObj, nodeDef.laneID);
                    //Debug.Log($"[NodeLayoutManager] Node {nodeObj.name} positioned at lane {nodeDef.laneID} with vertical offset {nodeDef.verticalOffset}.");
                    // Apply node color based on type
                    ApplyNodeColor(nodeObj, nodeDef.nodeType);
                }
                else
                {
                    Debug.LogError("Node component missing on nodePrefab.");
                }
            }
        }
        else
        {
            Debug.LogError("NodeHolder component missing on nodeHolderPrefab.");
        }
    }

    /// <summary>
    /// Positions a node horizontally based on its lane.
    /// </summary>
    private void PositionNodeBasedOnLane(GameObject nodeObj, LaneID laneID)
    {
        // Calculate lane position using the same spacing logic as in the NodeLayoutCreator
        float laneXPos = 0f;

        switch (laneID)
        {
            case LaneID.First:
                laneXPos = -2 * laneSpacing;
                break;
            case LaneID.Second:
                laneXPos = -1 * laneSpacing;
                break;
            case LaneID.Third:
                laneXPos = 0f;
                break;
            case LaneID.Fourth:
                laneXPos = 1 * laneSpacing;
                break;
            case LaneID.Fifth:
                laneXPos = 2 * laneSpacing;
                break;
        }

        // Apply only the X position, keeping Y and Z at 0
        nodeObj.transform.localPosition = new Vector3(laneXPos, 0f, 0f);
    }

    /// <summary>
    /// Applies the appropriate color to the node based on its type.
    /// </summary>
    private void ApplyNodeColor(GameObject nodeObj, Node.NodeType nodeType)
    {
        SpriteRenderer renderer = nodeObj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            switch (nodeType)
            {
                case Node.NodeType.Attack:
                    renderer.color = attackNodeColor;
                    break;
                case Node.NodeType.Rest:
                    renderer.color = restNodeColor;
                    break;
                case Node.NodeType.Act:
                    renderer.color = actNodeColor;
                    break;
                case Node.NodeType.Ult:
                    renderer.color = ultNodeColor;
                    break;
                default:
                    renderer.color = Color.gray;
                    break;
            }
        }
    }

    /// <summary>
    /// Creates a new pattern with the specified name and adds it to available patterns.
    /// </summary>
    public NodePattern CreatePattern(string patternName)
    {
        NodePattern newPattern = new NodePattern();
        newPattern.patternName = patternName;
        availablePatterns.Add(newPattern);
        return newPattern;
    }

    /// <summary>
    /// Adds a node definition to the specified pattern.
    /// </summary>
    public void AddNodeToPattern(NodePattern pattern, Node.NodeType nodeType, LaneID laneID, float verticalOffset)
    {
        if (pattern == null) return;

        NodeDefinition newNode = new NodeDefinition();
        newNode.nodeType = nodeType;
        newNode.laneID = laneID;
        newNode.verticalOffset = verticalOffset;

        pattern.nodeDefinitions.Add(newNode);
    }

    /// <summary>
    /// Clears all patterns from the available patterns list.
    /// </summary>
    public void ClearPatterns()
    {
        availablePatterns.Clear();
        currentPatternIndex = 0;
    }

    /// <summary>
    /// Creates some default patterns for testing.
    /// </summary>
    public void CreateDefaultPatterns()
    {
        ClearPatterns();

        // Calculate the row spacing based on grid properties
        float rowSpacing = nodeHolderSpacing;

        // Pattern 1: Simple alternating Attack nodes with empty spaces
        NodePattern pattern1 = CreatePattern("Alternating Attacks");
        AddNodeToPattern(pattern1, Node.NodeType.Attack, LaneID.First, 0);
        AddNodeToPattern(pattern1, Node.NodeType.Attack, LaneID.Third, rowSpacing * 2);  // 2 rows spacing
        AddNodeToPattern(pattern1, Node.NodeType.Attack, LaneID.Fifth, rowSpacing * 4);  // 2 more rows spacing
        pattern1.patternDuration = 5f;

        // Pattern 2: Rest and Act combination
        NodePattern pattern2 = CreatePattern("Rest and Act");
        AddNodeToPattern(pattern2, Node.NodeType.Rest, LaneID.Second, 0);
        AddNodeToPattern(pattern2, Node.NodeType.Rest, LaneID.Fourth, 0);
        AddNodeToPattern(pattern2, Node.NodeType.Act, LaneID.First, rowSpacing * 3);    // 3 rows spacing
        AddNodeToPattern(pattern2, Node.NodeType.Act, LaneID.Third, rowSpacing * 3);
        AddNodeToPattern(pattern2, Node.NodeType.Act, LaneID.Fifth, rowSpacing * 3);
        pattern2.patternDuration = 6f;

        // Pattern 3: Ultimate focus with rhythm gaps
        NodePattern pattern3 = CreatePattern("Ultimate Focus");
        AddNodeToPattern(pattern3, Node.NodeType.Ult, LaneID.Third, 0);
        AddNodeToPattern(pattern3, Node.NodeType.Attack, LaneID.Second, rowSpacing * 2); // 2 rows gap
        AddNodeToPattern(pattern3, Node.NodeType.Attack, LaneID.Fourth, rowSpacing * 2);
        pattern3.patternDuration = 5f;

        // Pattern 4: Spaced out pattern with deliberate gaps
        NodePattern pattern4 = CreatePattern("Spaced Rhythm");
        AddNodeToPattern(pattern4, Node.NodeType.Attack, LaneID.First, 0);
        AddNodeToPattern(pattern4, Node.NodeType.Rest, LaneID.Third, 0);
        AddNodeToPattern(pattern4, Node.NodeType.Attack, LaneID.Fifth, 0);
        // Large gap here (empty holders will be spawned)
        AddNodeToPattern(pattern4, Node.NodeType.Act, LaneID.Second, rowSpacing * 3);  // 3 rows gap
        AddNodeToPattern(pattern4, Node.NodeType.Act, LaneID.Fourth, rowSpacing * 3);
        // Another gap
        AddNodeToPattern(pattern4, Node.NodeType.Ult, LaneID.Third, rowSpacing * 6);    // 3 more rows gap
        pattern4.patternDuration = 8f;
    }
}