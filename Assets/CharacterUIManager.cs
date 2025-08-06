using UnityEngine;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{
    [SerializeField] private Slider HPPrefab;
    [SerializeField] private Slider StaPrefab;
    [SerializeField] private Slider CDPrefab;
    private Slider healthBar;
    private Slider staminaBar;
    public Slider cooldownBar;
    private Character character;
    private Camera mainCamera;
    Canvas canvas;

    private void Awake()
    {
        if (!TryGetComponent<Character>(out character)) return;

        mainCamera = Camera.main;
        canvas = FindObjectOfType<Canvas>();

        UiSetup(canvas);
    }

    private void UiSetup(Canvas canvas)
    {
        if (HPPrefab != null && StaPrefab != null && CDPrefab != null && canvas != null)
        {
            // Instantiate each bar as a child of the Canvas
            if (healthBar == null)
            {
                healthBar = Instantiate(HPPrefab, canvas.transform);
            }
            if (staminaBar == null)
            {
                staminaBar = Instantiate(StaPrefab, canvas.transform);
            }
            if (cooldownBar == null)
            {
                cooldownBar = Instantiate(CDPrefab, canvas.transform);
            }
        }

        // Set the maximum values based on character stats
        healthBar.maxValue = character.stats.MaxHealth;
        staminaBar.maxValue = character.stats.MaxStamina;
        cooldownBar.maxValue = character.GetComponent<AutoAttack>().attackInterval;
        cooldownBar.minValue = -1 * Mathf.Epsilon;

    }

    private void Start()
    {
        UpdateUI();
        UpdatePos();
    }

    void Update()
    {
        UiSetup(canvas);
    }
    private void LateUpdate()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (character == null) return;

        // Update bar values
        healthBar.value = character.stats.currentHealth;
        staminaBar.value = character.stats.CurrentStamina;
        cooldownBar.value = character.GetComponent<AutoAttack>().attackCooldown;

    }

    public void UpdatePos()
    {
        // Update bar positions based on character's screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);

        Vector3 offset = new Vector3(0, 50, 0); // Adjust this for desired bar height above character
        healthBar.transform.position = screenPos + offset;
        staminaBar.transform.position = screenPos + offset + new Vector3(0, -20, 0);
        cooldownBar.transform.position = screenPos + offset + new Vector3(30, -40, 0);
        Vector3 forward = new(1, (int)character.stats.Forward(), 1);

        healthBar.transform.localScale = forward;
        staminaBar.transform.localScale = forward;
        cooldownBar.transform.localScale = forward;
        if (forward.y < 0) cooldownBar.transform.position = new Vector3(cooldownBar.transform.position.x - 60, cooldownBar.transform.position.y, cooldownBar.transform.position.z);

    }
}
