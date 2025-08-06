using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static LanesManager;

/// <summary>
/// Editor tool to help create and visualize node layouts.
/// This script should be attached to a game object in the scene.
/// </summary>
#if UNITY_EDITOR
public class NodeLayoutCreator : MonoBehaviour
{
    [Header("Layout Settings")]
    public string layoutName = "New Layout";
    public float patternDuration = 3f;

    [Header("Node Visualization")]
    public float nodeSize = 0.5f;
    public Color attackNodeColor = Color.red;
    public Color restNodeColor = Color.green;
    public Color actNodeColor = Color.blue;
    public Color ultNodeColor = Color.yellow;

    [Header("Grid Settings")]
    public float gridWidth = 10f;
    public float gridHeight = 15f;
    public int gridRowCount = 10;
    public float laneSpacing = 2f;

    [System.Serializable]
    public class NodePlacement
    {
        public Node.NodeType nodeType;
        public LaneID laneID;
        public int row; // Row in the grid (0 is at the bottom)

        public float GetVerticalOffset(int totalRows, float gridHeight)
        {
            // Convert row to vertical offset (0 is bottom, higher rows are higher offsets)
            return (float)row / (float)(totalRows - 1) * gridHeight;
        }
    }

    public List<NodePlacement> nodePlacements = new List<NodePlacement>();

    private LanesManager lanesManager;
    private NodeLayoutManager layoutManager;

    private void Start()
    {
        lanesManager = LanesManager.Instance;
        layoutManager = FindObjectOfType<NodeLayoutManager>();
    }

    /// <summary>
    /// Adds a node placement to the current layout
    /// </summary>
    public void AddNodePlacement(Node.NodeType type, LaneID lane, int row)
    {
        NodePlacement newPlacement = new NodePlacement();
        newPlacement.nodeType = type;
        newPlacement.laneID = lane;
        newPlacement.row = Mathf.Clamp(row, 0, gridRowCount - 1);

        nodePlacements.Add(newPlacement);
    }

    /// <summary>
    /// Clears all node placements
    /// </summary>
    public void ClearPlacements()
    {
        nodePlacements.Clear();
    }

    /// <summary>
    /// Adds the current layout to the NodeLayoutManager as a pattern
    /// </summary>
    public void SaveToLayoutManager()
    {
        if (layoutManager == null)
        {
            Debug.LogError("NodeLayoutManager not found in scene.");
            return;
        }

        // Create a new pattern 
        NodeLayoutManager.NodePattern newPattern = new NodeLayoutManager.NodePattern();
        newPattern.patternName = layoutName;
        newPattern.patternDuration = patternDuration;

        // Calculate vertical scaling factor to convert rows to actual offsets
        float verticalRange = gridHeight;

        // Convert each node placement to a node definition
        foreach (NodePlacement placement in nodePlacements)
        {
            NodeLayoutManager.NodeDefinition nodeDef = new NodeLayoutManager.NodeDefinition();
            nodeDef.nodeType = placement.nodeType;
            nodeDef.laneID = placement.laneID;

            // Convert row to vertical offset (this is the relative offset within the pattern)
            nodeDef.verticalOffset = placement.GetVerticalOffset(gridRowCount, verticalRange);

            newPattern.nodeDefinitions.Add(nodeDef);
        }

        // Add the pattern to the layout manager
        layoutManager.availablePatterns.Add(newPattern);

        Debug.Log($"Saved pattern '{layoutName}' with {nodePlacements.Count} nodes to NodeLayoutManager.");
    }

    /// <summary>
    /// Test the current layout by spawning it in the game
    /// </summary>
    public void TestLayout()
    {
        if (layoutManager == null)
        {
            Debug.LogError("NodeLayoutManager not found in scene.");
            return;
        }

        // Create a temporary pattern
        NodeLayoutManager.NodePattern tempPattern = new NodeLayoutManager.NodePattern();
        tempPattern.patternName = "Test Pattern";
        tempPattern.patternDuration = patternDuration;

        // Calculate vertical scaling factor 
        float verticalRange = gridHeight;

        // Convert each node placement to a node definition
        foreach (NodePlacement placement in nodePlacements)
        {
            NodeLayoutManager.NodeDefinition nodeDef = new NodeLayoutManager.NodeDefinition();
            nodeDef.nodeType = placement.nodeType;
            nodeDef.laneID = placement.laneID;

            // Correction: Only set the relative vertical offset here.
            // baseSpawnHeight will be added by NodeLayoutManager during actual spawning.
            nodeDef.verticalOffset = placement.GetVerticalOffset(gridRowCount, verticalRange);

            tempPattern.nodeDefinitions.Add(nodeDef);
        }

        // Temporarily add and test the pattern
        layoutManager.availablePatterns.Clear();
        layoutManager.availablePatterns.Add(tempPattern);
        layoutManager.StartNodeSpawning();

        // Automatically stop after a few seconds
        StartCoroutine(StopTestAfterDelay(tempPattern.patternDuration + 5f));
    }

    private IEnumerator StopTestAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        layoutManager.StopNodeSpawning();
        yield return null;
    }

    // For editor visualization
    private void OnDrawGizmos()
    {
        DrawGrid();
        DrawNodePlacements();
    }

    /// <summary>
    /// Helper method to get the X position for a given LaneID.
    /// </summary>
    private float GetLaneXPosition(LaneID laneID)
    {
        switch (laneID)
        {
            case LaneID.First: return -2 * laneSpacing;
            case LaneID.Second: return -1 * laneSpacing;
            case LaneID.Third: return 0f;
            case LaneID.Fourth: return 1 * laneSpacing;
            case LaneID.Fifth: return 2 * laneSpacing;
            default: return 0f; // Should not happen with valid LaneIDs
        }
    }

    private void DrawGrid()
    {
        // Draw the grid background
        Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.3f);
        Gizmos.DrawCube(transform.position + new Vector3(0, gridHeight / 2, 0.1f),
                        new Vector3(gridWidth, gridHeight, 0.01f));

        // Draw horizontal grid lines
        Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        float rowHeight = gridHeight / (gridRowCount - 1);
        for (int i = 0; i < gridRowCount; i++)
        {
            float y = i * rowHeight;
            Vector3 startPos = transform.position + new Vector3(-gridWidth / 2, y, 0);
            Vector3 endPos = transform.position + new Vector3(gridWidth / 2, y, 0);
            Gizmos.DrawLine(startPos, endPos);
        }

        // Draw vertical lines on each lane
        Gizmos.color = new Color(0.5f, 0.5f, 0.8f, 0.5f);
        for (LaneID laneID = LaneID.First; laneID <= LaneID.Fifth; laneID++)
        {
            float x = GetLaneXPosition(laneID);
            Vector3 startPos = transform.position + new Vector3(x, 0, 0);
            Vector3 endPos = transform.position + new Vector3(x, gridHeight, 0);
            Gizmos.DrawLine(startPos, endPos);

#if UNITY_EDITOR
            Handles.color = Color.white;
            Handles.Label(startPos + new Vector3(0, -0.5f, 0), laneID.ToString());
#endif
        }
    }

    private void DrawNodePlacements()
    {
        float rowHeight = gridHeight / (gridRowCount - 1);

        foreach (NodePlacement placement in nodePlacements)
        {
            // Determine node color based on type
            Color nodeColor;
            switch (placement.nodeType)
            {
                case Node.NodeType.Attack: nodeColor = attackNodeColor; break;
                case Node.NodeType.Rest: nodeColor = restNodeColor; break;
                case Node.NodeType.Act: nodeColor = actNodeColor; break;
                case Node.NodeType.Ult: nodeColor = ultNodeColor; break;
                default: nodeColor = Color.gray; break;
            }

            // Get the X position for the node's lane
            float laneXPos = GetLaneXPosition(placement.laneID);
            float y = placement.row * rowHeight;

            // Draw the node
            Gizmos.color = nodeColor;
            Vector3 nodePos = transform.position + new Vector3(laneXPos, y, 0);
            Gizmos.DrawSphere(nodePos, nodeSize);

            // Draw node label
#if UNITY_EDITOR
            Handles.color = Color.white;
            Handles.Label(nodePos + new Vector3(nodeSize, nodeSize, 0), placement.nodeType.ToString());
#endif
        }
    }
}

/// <summary>
/// Custom editor for NodeLayoutCreator to provide a more user-friendly interface
/// </summary>
[CustomEditor(typeof(NodeLayoutCreator))]
public class NodeLayoutCreatorEditor : Editor
{
    private NodeLayoutCreator creator;
    private Node.NodeType selectedNodeType = Node.NodeType.Attack;
    private LanesManager.LaneID selectedLane = LanesManager.LaneID.First;
    private int selectedRow = 0;

    private void OnEnable()
    {
        creator = (NodeLayoutCreator)target;
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Add Node", EditorStyles.boldLabel);

        // Node type selection
        selectedNodeType = (Node.NodeType)EditorGUILayout.EnumPopup("Node Type:", selectedNodeType);

        // Lane selection
        selectedLane = (LanesManager.LaneID)EditorGUILayout.EnumPopup("Lane:", selectedLane);

        // Row selection
        selectedRow = EditorGUILayout.IntSlider("Row:", selectedRow, 0, creator.gridRowCount - 1);

        // Add node button
        if (GUILayout.Button("Add Node"))
        {
            creator.AddNodePlacement(selectedNodeType, selectedLane, selectedRow);
        }

        EditorGUILayout.Space(10);

        // Actions
        EditorGUILayout.LabelField("Layout Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Clear All Nodes"))
        {
            if (EditorUtility.DisplayDialog("Clear Nodes",
                "Are you sure you want to clear all node placements?", "Yes", "Cancel"))
            {
                creator.ClearPlacements();
            }
        }

        EditorGUILayout.Space(5);

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Test Layout"))
            {
                creator.TestLayout();
            }

            if (GUILayout.Button("Save Layout"))
            {
                creator.SaveToLayoutManager();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test and save layouts.", MessageType.Info);
        }

        // Show node count
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField($"Total Nodes: {creator.nodePlacements.Count}", EditorStyles.boldLabel);

        // List all node placements
        EditorGUILayout.LabelField("Node Placements:", EditorStyles.boldLabel);
        for (int i = 0; i < creator.nodePlacements.Count; i++)
        {
            NodeLayoutCreator.NodePlacement placement = creator.nodePlacements[i];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i + 1}. {placement.nodeType} - Lane: {placement.laneID}, Row: {placement.row}");

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                creator.nodePlacements.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif