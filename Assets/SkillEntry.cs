using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles tooltip display for a skill entry UI element
/// </summary>
public class SkillEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Skill Data")]
    public SkillDefinition skillData;

    [Header("UI Elements")]
    public TextMeshProUGUI skillNameText;
    public Image skillIconImage;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI skillTypeText;

    // Reference to this element's RectTransform
    private RectTransform rectTransform;

    private void Awake()
    {
        // Cache the RectTransform
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
    }

    /// <summary>
    /// Initializes the skill entry with data
    /// </summary>
    /// <param name="data">The skill definition data</param>
    public void Initialize(SkillDefinition data)
    {
        if (data != null)
        {
            skillData = data;
            skillNameText.text = skillData.SkillName;
            skillIconImage.sprite = skillData.SkillIcon;
            cooldownText.text = $"CD: {skillData.cooldown} + {skillData.warmup}";
            costText.text = $"Cost: {skillData.cost} Sta";
            descriptionText.text = skillData.description;
            skillTypeText.text = skillData.skillType.ToString();
        }
        else
        {
            Debug.LogError("Skill data is null");
        }
    }

    /// <summary>
    /// Shows tooltips when pointer enters the skill entry
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillData != null && !string.IsNullOrEmpty(skillData.description))
        {
            // Get tooltips for the description
            List<string> tooltips = TooltipManager.Instance.GetTooltipsFromDescription(skillData.description);

            // Show tooltips if any are found
            if (tooltips.Count > 0)
            {
                TooltipManager.Instance.ShowTooltip(tooltips, rectTransform);
            }
        }
    }

    /// <summary>
    /// Hides tooltips when pointer exits the skill entry
    /// This allows the tooltip to remain visible if the mouse moves directly from the skill to the tooltip
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // Check if we're moving to the tooltip panel (if not, hide the tooltip)
        TooltipManager.Instance.HideTooltip();
    }
}