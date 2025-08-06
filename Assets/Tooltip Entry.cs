using UnityEngine;

/// <summary>
/// Represents a tooltip entry for a specific gameplay concept
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "ToolTipEntry", menuName = "Character System/ToolTipEntry", order = 1)]
public class ToolTipEntry : ScriptableObject
{
    // String-based tooltip name instead of enum
    public string tooltipName;

    // Icon to display alongside the tooltip text
    public Sprite icon;

    [TextArea(3, 10)] public string description;
}