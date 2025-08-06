using TMPro;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel that displays character information and skills
/// </summary>
public class CharacterInfoPanel : MonoBehaviour
{
    public CharacterDefinition characterStats;

    [Header("UI Elements")]
    public TextMeshProUGUI characterName;
    public Image characterPortrait;
    public TextMeshProUGUI healthValue;
    public TextMeshProUGUI staminaValue;
    public TextMeshProUGUI characterClass;
    public Transform skillsPanel;
    public GameObject skillPrefab;

    [Header("Animation")]
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.2f;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        HideInfo();
    }

    private void Start()
    {
        // Initialize the panel with the character's data
        if (skillsPanel == null)
        {
            Debug.LogError("Skills panel is not assigned");
            return;
        }

        if (characterStats != null)
        {
            ShowInfo(characterStats);
        }
    }

    /// <summary>
    /// Shows character information
    /// </summary>
    /// <param name="data">Character definition data</param>
    public void ShowInfo(CharacterDefinition data)
    {
        // Populate data
        characterName.text = data.CharacterName;
        characterPortrait.sprite = data.CharacterSprite;
        healthValue.text = "Max HP: \n" +  data.HealthRating.ToString();
        staminaValue.text = "Max Stamina: \n" + data.StaminaRating.ToString();
        characterClass.text = data.Class.ToString();

        // Clear existing skills
        foreach (Transform child in skillsPanel)
        {
            Destroy(child.gameObject);
        }

        // Populate skills
        foreach (var skill in data.SkillDefinitions)
        {
            GameObject skillEntry = Instantiate(skillPrefab, skillsPanel);
            skillEntry.GetComponent<SkillEntry>().Initialize(skill);
        }

        // Show panel
        StartCoroutine(FadePanel(1));
    }

    /// <summary>
    /// Hides character information
    /// </summary>
    public void HideInfo()
    {
        StartCoroutine(FadePanel(0));
    }

    /// <summary>
    /// Fades the panel alpha
    /// </summary>
    /// <param name="targetAlpha">Target alpha value</param>
    private System.Collections.IEnumerator FadePanel(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0;

        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = targetAlpha > 0.5f;
    }
}