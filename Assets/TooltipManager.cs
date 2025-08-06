using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the display of tooltips with descriptive information about gameplay elements
/// </summary>
public class TooltipManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Singleton instance
    public static TooltipManager Instance { get; private set; }

    [Header("Tooltip Settings")]
    public GameObject tooltipPanel;
    public GameObject tooltipEntryPrefab;
    public Vector3 tooltipOffset = new Vector3(50, -20, 0);
    public float entrySpacing = 5f; // Space between tooltip entries
    public int maxVisibleEntries = 5; // Maximum number of visible entries before scrolling
    public float maxPanelWidth = 300f; // Maximum width for the tooltip panel

    [Header("Tooltip Library")]
    public List<ToolTipEntry> tooltipLibrary = new List<ToolTipEntry>();

    // Reference to tooltip container
    private RectTransform tooltipContainer;
    private ScrollRect scrollRect;

    // Track if mouse is currently over tooltip panel
    private bool isMouseOverTooltip = false;

    // Reference to the UI element that triggered the tooltip
    private RectTransform currentTriggerElement;

    // Current tooltip keywords being displayed
    private List<string> currentTooltipKeywords = new List<string>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // Ensure tooltip panel has the necessary components
        EnsureTooltipComponents();

        // Start with tooltip hidden
        HideTooltip();
    }

    /// <summary>
    /// Ensures the tooltip panel has all necessary components
    /// </summary>
    private void EnsureTooltipComponents()
    {
        // Make sure tooltip panel has a RectTransform
        RectTransform panelRect = tooltipPanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            panelRect = tooltipPanel.AddComponent<RectTransform>();
        }

        // Check for ScrollRect
        scrollRect = tooltipPanel.GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            scrollRect = tooltipPanel.AddComponent<ScrollRect>();

            // Create a content container if needed
            if (scrollRect.content == null)
            {
                GameObject contentObj = new GameObject("TooltipContent");
                contentObj.transform.SetParent(tooltipPanel.transform, false);

                RectTransform contentRect = contentObj.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.sizeDelta = Vector2.zero;

                // Add vertical layout group
                VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.spacing = entrySpacing;
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.childControlWidth = true;
                layout.childControlHeight = true;

                // Add content size fitter
                ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                scrollRect.content = contentRect;
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.scrollSensitivity = 15f;

                tooltipContainer = contentRect;
            }
        }
        else
        {
            tooltipContainer = scrollRect.content;
        }

        // Make sure the panel has an image component for background
        Image panelImage = tooltipPanel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = tooltipPanel.AddComponent<Image>();
            panelImage.color = new Color(0.0f, 0.0f, 0.1f, 0.2f);
        }
    }

    /// <summary>
    /// Shows tooltips for specified keywords at a position relative to a target UI element
    /// </summary>
    /// <param name="keywords">List of tooltip keywords to display</param>
    /// <param name="targetRect">RectTransform of the UI element the tooltip relates to</param>
    /// <param name="position">Optional: Custom position override (null to use targetRect for positioning)</param>
    public void ShowTooltip(List<string> keywords, RectTransform targetRect, Vector3? position = null)
    {
        if (keywords == null || keywords.Count <= 0 || targetRect == null)
        {
            Debug.LogWarning("TooltipManager: No keywords or targetRect provided. Hiding tooltip.");
            HideTooltip();
            return;
        }

        // Store current tooltip data
        currentTooltipKeywords = new List<string>(keywords);
        currentTriggerElement = targetRect;

        // Clear any existing tooltip entries
        ClearTooltipEntries();

        // Get screen dimensions
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        // Get distinct keywords to avoid duplicates
        List<string> distinctKeywords = keywords.Distinct().ToList();

        // Create tooltip entries for each keyword
        for (int i = 0; i < distinctKeywords.Count; i++)
        {
            var keyword = distinctKeywords[i];

            // Find the matching tooltip entry in our library (case-insensitive comparison)
            ToolTipEntry entry = tooltipLibrary.FirstOrDefault(t =>
                string.Equals(t.tooltipName, keyword, System.StringComparison.OrdinalIgnoreCase));

            if (entry != null)
            {
                // Instantiate the tooltip entry
                GameObject tooltipEntryObject = Instantiate(tooltipEntryPrefab, tooltipContainer);
                tooltipEntryObject.name = "Tooltip: " + keyword;

                // Set the tooltip icon if available
                Image iconImage = tooltipEntryObject.transform.GetChild(0).Find("Image").GetComponent<Image>();
                if (iconImage != null && entry.icon != null)
                {
                    iconImage.sprite = entry.icon;
                    iconImage.gameObject.SetActive(true);
                }
                else if (iconImage != null)
                {
                    iconImage.enabled = false; // Hide icon if not available
                }

                // Set the tooltip text
                TextMeshProUGUI tooltipText = tooltipEntryObject.GetComponentInChildren<TextMeshProUGUI>();
                if (tooltipText != null)
                {
                    tooltipText.text = $"<b>{entry.tooltipName}:</b> {entry.description}";
                }

                // Add layout element to control size
                LayoutElement layoutElement = tooltipEntryObject.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = tooltipEntryObject.AddComponent<LayoutElement>();
                }
                layoutElement.preferredWidth = maxPanelWidth;
                //uses the combined height of all entries to calculate the preferred height
                layoutElement.preferredHeight = (tooltipEntryObject.GetComponent<RectTransform>().sizeDelta.y);

                layoutElement.flexibleWidth = 0;
            }
        }

        // Wait for layout to update
        Canvas.ForceUpdateCanvases();

        // Calculate preferred height based on entries
        float entryHeight = (tooltipEntryPrefab.GetComponent<RectTransform>().sizeDelta.y + entrySpacing);
        float preferredHeight = Mathf.Min(distinctKeywords.Count * entryHeight + 20f, maxVisibleEntries * entryHeight + 20f);

        // Set the tooltip panel size
        RectTransform panelRect = tooltipPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(maxPanelWidth, preferredHeight);

        // Calculate the position for the tooltip panel
        Vector3 tooltipPosition;

        if (position.HasValue)
        {
            // Use the provided custom position
            tooltipPosition = position.Value;
        }
        else
        {
            // Position next to the target rect (preferably to the right)
            // Get the corners of the target rect in screen space
            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);

            // Determine if we have enough room to the right
            bool placeToRight = (corners[2].x + panelRect.sizeDelta.x + tooltipOffset.x) < screenSize.x;

            if (placeToRight)
            {
                // Place to the right of the target
                tooltipPosition = new Vector3(
                    corners[2].x + tooltipOffset.x, // Right edge + offset
                    (corners[1].y + corners[2].y) * 0.5f, // Vertical center
                    0
                );
            }
            else
            {
                // Place to the left of the target
                tooltipPosition = new Vector3(
                    corners[0].x - panelRect.sizeDelta.x - tooltipOffset.x, // Left edge - width - offset
                    (corners[0].y + corners[1].y) * 0.5f, // Vertical center
                    0
                );
            }

            // Adjust vertical position to ensure the tooltip is fully visible
            float topEdge = tooltipPosition.y + (panelRect.sizeDelta.y * 0.5f);
            float bottomEdge = tooltipPosition.y - (panelRect.sizeDelta.y * 0.5f);

            if (topEdge > screenSize.y)
            {
                // If tooltip would go off the top of the screen, push it down
                tooltipPosition.y -= (topEdge - screenSize.y);
            }
            else if (bottomEdge < 0)
            {
                // If tooltip would go off the bottom of the screen, push it up
                tooltipPosition.y -= bottomEdge;
            }
        }

        // Keep tooltip on screen (additional safety check)
        tooltipPosition.x = Mathf.Clamp(tooltipPosition.x, panelRect.sizeDelta.x * 0.5f, screenSize.x - panelRect.sizeDelta.x * 0.5f);
        tooltipPosition.y = Mathf.Clamp(tooltipPosition.y, panelRect.sizeDelta.y * 0.5f, screenSize.y - panelRect.sizeDelta.y * 0.5f);

        tooltipPanel.transform.position = tooltipPosition;

        // Reset scroll position to top
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        // Show the tooltip
        tooltipPanel.SetActive(true);
    }

    /// <summary>
    /// Hides the tooltip panel if the mouse is not over it
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipPanel != null && !isMouseOverTooltip)
        {
            tooltipPanel.SetActive(false);
            currentTooltipKeywords.Clear();
            currentTriggerElement = null;
        }
    }

    /// <summary>
    /// Force hide the tooltip panel regardless of mouse position
    /// </summary>
    public void ForceHideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            isMouseOverTooltip = false;
            currentTooltipKeywords.Clear();
            currentTriggerElement = null;
        }
    }

    /// <summary>
    /// Clears all tooltip entries from the panel
    /// </summary>
    private void ClearTooltipEntries()
    {
        if (tooltipContainer == null)
        {
            // If tooltip container doesn't exist yet, try to find or create it
            EnsureTooltipComponents();
        }

        foreach (Transform child in tooltipContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Called when pointer enters the tooltip panel
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOverTooltip = true;
    }

    /// <summary>
    /// Called when pointer exits the tooltip panel
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOverTooltip = false;

        // Only hide if we're not over the original trigger element
        if (currentTriggerElement != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(currentTriggerElement, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
            Rect rect = currentTriggerElement.rect;

            if (!rect.Contains(localPoint))
            {
                ForceHideTooltip();
            }
        }
        else
        {
            HideTooltip();
        }
    }

    /// <summary>
    /// Parses a description string and returns a list of tooltip keywords found
    /// </summary>
    /// <param name="description">The text to parse for keywords</param>
    /// <returns>List of tooltip keywords found</returns>
    public List<string> GetTooltipsFromDescription(string description)
    {
        List<string> tooltips = new List<string>();

        if (string.IsNullOrEmpty(description))
        {
            return tooltips;
        }

        // Split the description into words and check each word for keywords
        string[] words = description.Split(' ', '.', ',', '!', '?', ':', ';');
        foreach (string word in words)
        {
            // Skip empty words
            if (string.IsNullOrWhiteSpace(word))
            {
                continue;
            }

            // Clean the word (remove whitespace and convert to uppercase)
            string cleanWord = word.Trim().ToUpper();

            // Check if the word is a keyword (case-insensitive)
            if (tooltipLibrary.Any(t => string.Equals(t.tooltipName, cleanWord, System.StringComparison.OrdinalIgnoreCase)))
            {
                // Add the tooltip name to the list if it matches
                Debug.Log($"Found tooltip keyword: {cleanWord} in {word}");
                tooltips.Add(cleanWord);
            }
        }

        return tooltips.Distinct().ToList();
    }
}