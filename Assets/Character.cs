using System;
using System.Collections.Generic;
using UnityEngine;
using static Skill;

public class Character : MonoBehaviour
{
    public CharacterStats stats;
    public LanesManager.LaneID laneID;  // Updated to use LaneID directly
    public List<SkillDefinition> skillDefinitions = new List<SkillDefinition>();
    [SerializeField] public List<Skill> Skills = new List<Skill>();

    public enum DamageSourceType
    {
        None, // Default or unspecified
        Act,
        Ult,
        Talent,
        Strike, // For basic attacks
        DoT,    // Damage over Time
        Other   // For any other miscellaneous damage
    }
    public void ApplyStatus(StatusEffect statusEffect)
    {
        // NEW: Check if the character can receive a new status effect based on their state
        if (!stats.CanReceiveStatus())
        {
            Debug.LogWarning($"Cannot apply {statusEffect.StatusName} to {stats.CharacterName} because they are in {stats.State} state (not Normal).");
            return; // Exit if character cannot receive new statuses
        }

        // Check if the status is an overtime effect
        if (statusEffect.HasOverTimeEffect())
        {
            Debug.Log($"Applying overtime effect: {statusEffect.StatusName} to {stats.CharacterName}.");
        }

        // Check if a matching StatusEffect already exists
        StatusEffect existingEffect = null;
        foreach (StatusEffect effect in stats.Effects)
        {
            if (effect.StatusName == statusEffect.StatusName)
            {
                existingEffect = effect;
                break;
            }
        }

        if (existingEffect != null)
        {
            // Add stacks to the existing StatusEffect
            // It's generally better to have a method in StatusEffect for adding stacks
            // that handles internal logic (like refreshing duration, capping stacks).
            // For now, directly adding is fine if your StatusEffect class supports it.
            foreach (StatusEffect.Stack stack in statusEffect.Stacks)
            {
                existingEffect.Stacks.Add(stack);
            }
            Debug.Log($"Added stacks to existing status: {statusEffect.StatusName} on {stats.CharacterName}. Total stacks: {existingEffect.Stacks.Count}");
        }
        else
        {
            // Add the new StatusEffect to the list
            stats.Effects.Add(statusEffect);
            Debug.Log($"Applied new status: {statusEffect.StatusName} to {stats.CharacterName}.");
        }
        stats.CalculateStats(); // Recalculate stats immediately when a new status is applied
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        // Check if the status effect exists in the list
        StatusEffect existingEffect = null;
        foreach (StatusEffect effect in stats.Effects)
        {
            if (effect.StatusName == statusEffect.StatusName)
            {
                existingEffect = effect;
                break;
            }
        }
        if (existingEffect != null)
        {
            // Remove the status effect from the list
            stats.Effects.Remove(existingEffect);
            Debug.Log($"Removed status effect: {statusEffect.StatusName} from {stats.CharacterName}.");
            stats.CalculateStats(); // Recalculate stats immediately when a status is removed
        }
        else
        {
            Debug.LogWarning($"Status effect: {statusEffect.StatusName} not found on {stats.CharacterName}.");
        }
    }


    public enum Affiliation
    {
        Player = 1,
        Enemy = -1
    }

    public Lane lane = null;
    [SerializeField] public CharacterDefinition CharacterDef;
    SpriteRenderer CharacterRender, WeaponRender;
    public void CharacterSetup()
    {
        if (CharacterDef == null)
        {
            Debug.LogError("CharacterDefinition is not assigned!");
            return;
        }

        // Assign stats
        stats.CharacterName = CharacterDef.CharacterName;
        stats.Class = CharacterDef.Class;
        stats.Critical = CharacterDef.Critical;

        stats.HealthRating = CharacterDef.HealthRating;
        stats.StaminaRating = CharacterDef.StaminaRating;
        stats.AccuracyRating = CharacterDef.AccuracyRating;
        stats.ArcaneRating = CharacterDef.ArcaneRating;
        stats.MightRating = CharacterDef.MightRating;
        stats.DefenseRating = CharacterDef.DefenseRating;
        stats.SpeedRating = CharacterDef.SpeedRating;

        // Get SpriteRenderers
        CharacterRender = GetComponent<SpriteRenderer>();
        if (CharacterRender == null)
        {
            Debug.LogError($"Character SpriteRenderer is missing for {stats.CharacterName}");
            return;
        }
        Transform child = transform.GetChild(0);
        if (child == null)
        {
            Debug.LogError("Child Transform is missing!");
            return;
        }
        WeaponRender = child.GetComponentInChildren<SpriteRenderer>();
        if (WeaponRender == null)
        {
            Debug.LogError($"Weapon SpriteRenderer is missing for {stats.CharacterName}");
        }

        // Assign Sprites
        CharacterRender.sprite = CharacterDef.CharacterSprite;
        WeaponRender.sprite = CharacterDef.WeaponSprite;

        // Assign other properties
        // stats.CharAffil = CharacterDef.Affiliation;

        // Assign skills
        if (CharacterDef.SkillDefinitions == null || CharacterDef.SkillDefinitions.Count <= 0)
        {
            Debug.LogError($"SkillDefinitions are missing or empty for {stats.CharacterName}");
            return;
        }
        skillDefinitions.AddRange(CharacterDef.SkillDefinitions);

    }


    private bool InitializeSkills()
    {
        if (skillDefinitions == null || skillDefinitions.Count <= 0)
        {
            Debug.LogError($"SkillDefinitions are missing or empty for {stats.CharacterName}");
            return false;
        }

        foreach (SkillDefinition skillDef in skillDefinitions)
        {
            // Directly create a new skill, which will automatically add the effects from the constructor
            Skill newSkill = new Skill(skillDef, this);

            // Add the skill to the Skills list
            Skills.Add(newSkill);
        }
        return true;
    }

    public void InitStats()
    {
        this.stats.SetBaseStats();

        this.calcStats();
        this.stats.currentHealth = this.stats.MaxHealth;
        this.stats.CurrentStamina = this.stats.MaxStamina;

    }

    private void calcStats()
    {
        if (stats == null) return;
        this.stats.CalculateStats();
    }
    private System.Collections.IEnumerator StatUpdateCRTN;

    void Start()
    {
        // 1. Setup character from CharacterDefinition (assigns ratings)
        CharacterSetup();

        // 2. Initialize and calculate stats for the first time
        // This will call SetBaseStats and CalculateStats
        InitStats();

        // 3. Initialize skills (which may depend on stats)
        InitializeSkills();

        // Remove the redundant calcStats() call here:
        // calcStats(); // REMOVE THIS LINE

        StatUpdateCRTN = statupdater();
        AssignLane(laneID);  // Updated to use laneID
        AfterNode += AfterNodeSetup;
        GetComponent<CharacterUIManager>().UpdatePos();
        GetComponent<CharacterUIManager>().UpdateUI();
        StartCoroutine(statupdater());

        // NEW: Register DistributeDmgDealt effects
        foreach (Skill skill in Skills)
        {
            foreach (SkillEffect effect in skill.Effects)
            {
                // This part of the code needs SkillEffect.EffectType and RegisterDamageDistributionEvent
                // Assuming SkillEffect.cs is also up-to-date with this logic
                // Make sure effect.Type is accessible (public enum) and RegisterDamageDistributionEvent exists
                // and takes `this` (Character instance) if it needs to register events on the Character.
                if (effect.Type == SkillEffect.EffectType.DistributeDmgDealt)
                {
                    effect.RegisterDamageDistributionEvent(this);
                }
            }
        }
    }

    void AfterNodeSetup(Character character)
    {
        this.CDRtoALL();
        stats.TickAllStatuses();
    }
    System.Collections.IEnumerator statupdater()
    {
        float waitTIme = 0.35f;

        while (true)
        {
            yield return new WaitForSeconds(waitTIme);
            calcStats();
        }

    }

    public void CDRtoALL(int amount = 1)
    {
        for (UInt32 i = 0; i < amount; i++)
        {
            foreach (Skill skill in Skills)
            {
                skill.ReduceCooldown(this);
            }
        }
    }

    public void AssignLane(LanesManager.LaneID laneID)
    {
        lane = LanesManager.Instance.GetLane(laneID, stats.CharAffil);

        Vector3 pos = LanesManager.Instance.GetPos(lane.laneID, stats.CharAffil);
        transform.position = pos;

        // Add character to the appropriate list
        if (stats.CharAffil == Character.Affiliation.Player)
        {
            LanesManager.Instance.EnemyCharacters.Remove(this);
            Debug.Log($"Assigning {stats.CharacterName} to PlayerCharacters on {laneID} lane.");
            if (!LanesManager.Instance.PlayerCharacters.Contains(this))
                LanesManager.Instance.PlayerCharacters.Add(this);
            else
                Debug.LogWarning($"{stats.CharacterName} is already in PlayerCharacters on {laneID} lane.");
        }
        else
        {
            LanesManager.Instance.PlayerCharacters.Remove(this);
            Debug.Log($"Assigning {stats.CharacterName} to EnemyCharacters on {laneID} lane.");
            if (!LanesManager.Instance.EnemyCharacters.Contains(this))
                LanesManager.Instance.EnemyCharacters.Add(this);
            else
                Debug.LogWarning($"{stats.CharacterName} is already in EnemyCharacters on {laneID} lane.");
        }

        GetComponent<CharacterUIManager>().UpdatePos();
        GetComponent<CharacterUIManager>().UpdateUI();
    }

    /// <summary>
    /// Finds a target character in a specified order (front-to-back or back-to-front).
    /// Skips incapacitated targets only when targeting opposing affiliations.
    /// </summary>
    /// <param name="targetAffil">The affiliation of the target (Player or Enemy).</param>
    /// <param name="reverseOrder">If true, search from LaneID.Fifth down to LaneID.First. Otherwise, LaneID.First up to LaneID.Fifth.</param>
    /// <returns>The first valid target found, or null if no target is found.</returns>
    public Character FindTargetInOrder(Affiliation targetAffil, bool reverseOrder = false)
    {
        // No need to create a 'targets' list here. We will directly query LanesManager.

        // Determine if we should skip incapacitated characters.
        // Skip if targeting the opposing side (e.g., player targeting enemy, or enemy targeting player).
        bool skipIncapacitated = (this.stats.CharAffil != targetAffil);

        List<Lane> lanes = LanesManager.Instance.GetLanesByAffiliation(targetAffil);

        if (lanes == null || lanes.Count == 0) // Changed to == 0 for clarity
        {
            Debug.LogError($"No lanes found for affiliation: {targetAffil}");
            return null;
        }

        // Sort lanes based on reverseOrder flag
        if (reverseOrder)
        {
            // Sort in descending order of LaneID (Fifth, Fourth, etc.)
            lanes.Sort((a, b) => b.laneID.CompareTo(a.laneID));
        }
        else
        {
            // Sort in ascending order of LaneID (First, Second, etc.)
            lanes.Sort((a, b) => a.laneID.CompareTo(b.laneID));
        }

        foreach (Lane lane in lanes)
        {
            if (lane == null)
            {
                Debug.LogWarning("Found a null lane in lanes list.");
                continue;
            }

            // MODIFIED: Use the updated GetTargetInLane with the skipIncapacitated flag
            Character target = GetTargetInLane(lane.laneID, targetAffil, skipIncapacitated);

            if (target != null)
            {
                Debug.Log($"Found target {target.stats.CharacterName} in lane {lane.laneID} for {targetAffil} (Skipping Incapacitated: {skipIncapacitated}).");
                return target;
            }
            else
            {
                Debug.Log($"No valid target found in lane {lane.laneID} for {targetAffil} (Skipping Incapacitated: {skipIncapacitated}).");
            }
        }
        // No valid target found
        Debug.Log($"No target found in any lane for affiliation {targetAffil} (Reverse Order: {reverseOrder}).");
        return null;
    }

    /// <summary>
    /// Takes LaneID and Affiliation to find a character in a specified lane.
    /// Conditionally skips incapacitated characters based on the skipIncapacitated flag.
    /// </summary>
    /// <returns>A [Character] in a team from a specified lane</returns>
    public Character GetTargetInLane(LanesManager.LaneID targetLaneID, Affiliation targetAffiliation, bool skipIncapacitated)
    {
        // Directly use LanesManager to get the character
        Character character = LanesManager.Instance.GetCharacter(targetLaneID, targetAffiliation);

        if (character != null)
        {
            if (skipIncapacitated && character.stats.State == CharacterStats.CharacterState.Incapacitated)
            {
                Debug.Log($"Character {character.stats.CharacterName} in lane {targetLaneID} is Incapacitated and will be skipped as a target (skipIncapacitated is true).");
                return null; // Return null if incapacitated and skipping is enabled
            }
            Debug.Log($"Character {character.stats.CharacterName} found in lane {targetLaneID}. Incapacitated status: {character.stats.State == CharacterStats.CharacterState.Incapacitated}, Skip Incapacitated: {skipIncapacitated}");
            return character; // Return the character if valid, or if not skipping incapacitated
        }

        return null; // No character found in this lane
    }

    public void TakeDamage(float damage, DamageSourceType sourceType, bool ignoreDef = false)
    {
        // NEW: Check if the character can receive damage based on their state
        if (!stats.CanReceiveDamage())
        {
            Debug.Log($"{stats.CharacterName} ignored {damage} damage due to being in {stats.State} state.");
            return; // Character cannot receive damage
        }

        Debug.Log($"<color=orange>TakeDamage called on {stats.CharacterName}. Incoming damage: {damage}, Source: {sourceType}</color>");

        if (stats.currentHealth <= 0 && stats.State == CharacterStats.CharacterState.Incapacitated) // Added state check for redundancy/clarity
        {
            Debug.Log($"<color=red>{stats.CharacterName} is already incapacitated. Skipping damage.</color>");
            return; // Character already incapacitated
        }

        Debug.Log($"<color=orange>{stats.CharacterName}'s Defense: {stats.Defense}</color>");

        if (damage <= 0) return;
        float finalDamage = 0;
        if (!ignoreDef && stats.Defense > 0)
        {
            finalDamage = Mathf.Max(damage * 0.05f, damage - stats.Defense);

        }
        else
        {
            finalDamage = damage;
        }

        finalDamage *= stats.DmgRecievedModifier;

        stats.currentHealth -= finalDamage;
        // The clamping to MaxHealth happens in CharacterStats.CalculateStats()
        // but it's often good practice to clamp immediately after modifying current health.
        // However, given your CharacterStats.CalculateStats() has specific clamping for Incapacitated,
        // it might be better to let CalculateStats handle it entirely for consistency.
        // For now, I'll keep the immediate clamp to 0, but remove the upper bound,
        // and rely on CalculateStats for the upper bound.
        stats.currentHealth = Mathf.Max(stats.currentHealth, 0); // Clamp to prevent negative health

        Debug.Log($"<color=orange>Calculated Final Damage: {finalDamage}</color>");
        Debug.Log($"{stats.CharacterName} took {finalDamage} damage. Remaining health: {stats.currentHealth}");

        // Trigger OnTakeDamage event
        TriggerTakeDamage(finalDamage, sourceType);

        stats.CalculateStats(); // Ensure stats are recalculated after health change

        if (stats.currentHealth <= 0 && stats.State != CharacterStats.CharacterState.Incapacitated) // Only set if not already incapacitated
        {
            stats.SetState(CharacterStats.CharacterState.Incapacitated); // Set state here
            TriggerOnIncapacitated();
        }
    }

    public void TakeDrain(float drain)
    {
        // NEW: Check if the character can receive drain based on their state
        if (!stats.CanReceiveDrain())
        {
            Debug.Log($"{stats.CharacterName} ignored {drain} drain due to being in {stats.State} state.");
            return; // Character cannot receive drain
        }

        float actualDrain = drain * stats.DrainRecievedModifier;
        stats.CurrentStamina = Mathf.Max(stats.CurrentStamina - actualDrain, 0); // Clamp to prevent negative stamina

        Debug.LogError($"{stats.CharacterName} got drained {actualDrain}. Remaining stamina: {stats.CurrentStamina}"); // Changed to Debug.Log for consistency, keep Error if you want it to stand out

        TriggerTakeDrain(actualDrain);

        stats.CalculateStats(); // Ensure stats are recalculated after stamina change

        if (GetComponent<CharacterUIManager>() != null)
        {
            GetComponent<CharacterUIManager>().UpdateUI();
        }
        if (stats.CurrentStamina <= 0 && stats.State != CharacterStats.CharacterState.Exhausted) // Only set if not already exhausted
        {
            stats.SetState(CharacterStats.CharacterState.Exhausted); // Set state here
            TriggerOnExhausted();
        }
    }
    public void ConsumeStamina(float consumeAmount) // Renamed parameter for clarity
    {
        // Assuming ConsumeStamina is an intentional action (e.g., skill cost) and should always happen
        // regardless of Exhausted state, unless explicitly stated otherwise.
        // If you want to prevent consumption while Exhausted, add a check here similar to TakeDrain.

        float actualConsume = consumeAmount * stats.StaConsumeModifier;
        stats.CurrentStamina = Mathf.Max(stats.CurrentStamina - actualConsume, 0); // Clamp to prevent negative stamina

        if (GetComponent<CharacterUIManager>() != null)
        {
            GetComponent<CharacterUIManager>().UpdateUI();
        }
        // The state transition logic should ideally be handled within CharacterStats.CalculateStats()
        // or by calling stats.SetState directly based on currentStamina,
        // and ensuring you don't re-set the state if already in it.
        if (stats.CurrentStamina <= 0 && stats.State != CharacterStats.CharacterState.Exhausted) // Only set if not already exhausted
        {
            stats.SetState(CharacterStats.CharacterState.Exhausted); // Set state here
            TriggerOnExhausted();
        }

        TriggerConsumeStamina(actualConsume); 
        stats.CalculateStats(); // Ensure stats are recalculated after stamina change
    }

    public void Heal(float heal)
    {
        float actualHeal = stats.HealRecievedModifier * heal;
        float maxHealthToClamp = stats.MaxHealth; // Default to normal max health

        // Only allow overhealing if incapacitated
        if (stats.State == CharacterStats.CharacterState.Incapacitated)
        {
            maxHealthToClamp = stats.MaxHealth * 1.5f;
        }

        stats.currentHealth = Mathf.Clamp(actualHeal + stats.currentHealth, 0, maxHealthToClamp);

        Debug.Log($"{stats.CharacterName} healed for {actualHeal}. Current Health: {stats.currentHealth}");

        TriggerOnHealTaken(actualHeal);

        // Check if character can return from Incapacitated
        if (stats.State == CharacterStats.CharacterState.Incapacitated && stats.currentHealth >= stats.MaxHealth * 1.5f)
        {
            stats.SetState(CharacterStats.CharacterState.Normal);
            Debug.Log($"{stats.CharacterName} recovered from Incapacitated!");
            // You might want to trigger an additional event here, e.g., OnRecoveredFromIncapacitated
        }



        // Update UI after healing
        if (GetComponent<CharacterUIManager>() != null)
        {
            GetComponent<CharacterUIManager>().UpdateUI();
        }
    }
    public void RecoverStamina(float Recovery)
    {
        float actualRecovery = stats.RecoveryRecievedModifier * Recovery;
        float maxStaminaToClamp = stats.MaxStamina; // Default to normal max stamina

        // Only allow over-recovery if exhausted
        if (stats.State == CharacterStats.CharacterState.Exhausted)
        {
            maxStaminaToClamp = stats.MaxStamina * 1.5f;
        }

        stats.CurrentStamina = Mathf.Clamp(actualRecovery + stats.CurrentStamina, 0, maxStaminaToClamp);

        Debug.Log($"{stats.CharacterName} recovered for {actualRecovery}. Current Stamina: {stats.CurrentStamina}");

        TriggerTakeRecovery(actualRecovery);

        // Check if character can return from Exhausted
        if (stats.State == CharacterStats.CharacterState.Exhausted && stats.CurrentStamina >= stats.MaxStamina * 1.5f)
        {
            stats.SetState(CharacterStats.CharacterState.Normal);
            Debug.Log($"{stats.CharacterName} recovered from Exhausted!");
            // You might want to trigger an additional event here, e.g., OnRecoveredFromExhausted
        }


        // Update UI after recovery
        if (GetComponent<CharacterUIManager>() != null)
        {
            GetComponent<CharacterUIManager>().UpdateUI();
        }
    }


    private bool IsSameLane(Node node)
    {
        if (node == null) Debug.LogError($"no node exists! node is null");
        return node.lane != null && node.lane.laneID == laneID;
    }

    // UPDATED OnNodeHit Method
    private void OnNodeHit(Node node)
    {
        if (node == null) return;

        // Check if the node is on the same lane as this character
        if (IsSameLane(node))
        {
            Debug.Log($"{this.stats.CharacterName} on lane {laneID} detected node hit with type: {node.type}. Attempting to activate skill.");

            // Before attempting skill activation, check if the character CAN activate nodes in their current state.
            if (!CanActivateNode(node.type))
            {
                Debug.LogWarning($"{stats.CharacterName} cannot activate {node.type} node due to current state ({stats.State}).");
                return;
            }

            Skill skillToActivate = null;

            if (node.type == Node.NodeType.Act)
            {
                // Find the first activatable 'Act' skill
                skillToActivate = Skills.Find(skill => skill.Type == Skill.SkillType.Act && skill.IsActivatable());
            }
            else if (node.type == Node.NodeType.Ult)
            {
                // Find the first activatable 'Ult' skill
                skillToActivate = Skills.Find(skill => skill.Type == Skill.SkillType.Ult && skill.IsActivatable());
            }

            if (skillToActivate != null)
            {
                // Now, use the skill's own trigger method. This will consume stamina, reset cooldown, and trigger effects.
                if (skillToActivate.CheckAndTriggerSkill())
                {
                    Debug.Log($"{stats.CharacterName} successfully used {skillToActivate.Name} ({skillToActivate.Type}) from node hit!");
                }
                else
                {
                    Debug.LogWarning($"{stats.CharacterName} found {skillToActivate.Name}, but it failed CheckAndTriggerSkill for node type {node.type}. Check logs from Skill.cs for details.");
                }
            }
            else
            {
                Debug.Log($"No activatable {node.type} skill found for {stats.CharacterName} on lane {laneID}.");
            }
        }
    }

    void OnEnable()
    {
        // Subscribe to the NodeHitEvent
        Node.NodeHitEvent += OnNodeHit;

        NodeHolder.NodeHolderHitEvent += OnNodeHolderHit;
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        Node.NodeHitEvent -= OnNodeHit;

        NodeHolder.NodeHolderHitEvent -= OnNodeHolderHit;
    }
    private void OnNodeHolderHit(NodeHolder nodeHolder)
    {
        // Check if the nodeHolder is on the same lane as this character
        if (nodeHolder.transform.childCount > 0)
        {
            Debug.Log($"{this.stats.CharacterName} on lane {laneID} detected nodeHolder hit");

            // No need to do anything specific here as TriggerOnNode is called globally
            // by the NodeHolder for all characters
        }
    }
    public bool CanActivateNode(Node.NodeType nodeType)
    {
        // Allow Rest node activation even when exhausted
        if (stats.State == CharacterStats.CharacterState.Exhausted && nodeType == Node.NodeType.Rest)
        {
            return true;
        }

        // If incapacitated, cannot activate any node (except potentially a specific "revive" type node if you add one)
        if (stats.State == CharacterStats.CharacterState.Incapacitated)
        {
            return false;
        }

        // If exhausted, but not a Rest node, cannot activate anything else
        if (stats.State == CharacterStats.CharacterState.Exhausted)
        {
            Debug.Log($"{stats.CharacterName} cannot activate {nodeType} node because they are Exhausted (only Rest is allowed).");
            return false;
        }

        // Normal state checks for Act/Ult skills
        foreach (Skill skill in Skills)
        {
            if (skill.IsActivatable())
            {
                if ((skill.Type == Skill.SkillType.Act && nodeType == Node.NodeType.Act) ||
                    (skill.Type == SkillType.Ult && nodeType == Node.NodeType.Ult))
                {
                    return true;
                }
            }
        }
        // If no specific Act/Ult skills are found, but the character is in a Normal state,
        // they might still be able to do something (e.g., a basic attack tied to a node, 
        // or just indicate that the node can be *interacted* with, even if no skill fires).
        // For 'Normal' state, if no skills are found for Act/Ult, and it's not a Rest node,
        // we'll return false here to prevent activation without a purpose.
        if (nodeType == Node.NodeType.Act || nodeType == Node.NodeType.Ult)
        {
            return false; // No activatable skill found for this node type
        }

        // Default for other node types (e.g., Strike, Rest when not exhausted) if no specific skill is needed
        return true;
    }

    public event Action<Character> BeforeNode;
    public event Action<Character> AfterNode;
    public event Action<Character> BeforeCrit;
    public event Action<Character> AfterCrit;
    public event Action<Character> BeforeAct;
    public event Action<Character> AfterAct;
    public event Action<Character> BeforeUlt;
    public event Action<Character> AfterUlt;
    public event Action<Character> BeforeStrike;
    public event Action<Character> AfterStrike;
    public event Action<Character> BeforeRest;
    public event Action<Character> AfterRest;

    public event Action<float> OnTakeHeal;
    public event Action<float> OnDealHeal;

    public event Action<float, DamageSourceType> OnTakeDamage;
    public event Action<float> OnTakeCrit;
    public event Action<float> OnTakeNonCrit;
    public event Action<float, DamageSourceType> OnDealDamage; // MODIFIED: Added DamageSourceType
    public event Action<float> OnDealCrit;
    public event Action<float> OnDealNonCrit;

    public event Action<float> OnTakeDrain;
    public event Action<float> OnConsumeStamina;
    public event Action<float> OnDealDrain;
    public event Action<float> OnTakeRecovery;
    public event Action<float> OnDealRecovery;

    public event Action<Character> OnIncapacitated;
    public event Action<Character> OnExhausted;

    public event Action<float> OnHPThresholdReached;
    public event Action<float> OnStaminaThresholdReached;


    public bool Random(float chance = 1.0f) { return UnityEngine.Random.value >= chance; }
    public void TriggerBeforeNode()
    {
        BeforeNode?.Invoke(this);
    }
    public void TriggerOnNode()
    {
        AfterNode?.Invoke(this);
    }
    public void TriggerBeforeCrit()
    {
        BeforeCrit?.Invoke(this);
    }
    public void TriggerOnCrit()
    {
        AfterCrit?.Invoke(this);
    }
    public void TriggerBeforeAct()
    {
        Debug.Log($"{stats.CharacterName} is about to use an Act skill.");
        BeforeAct?.Invoke(this);
    }
    public void TriggerOnAct()
    {
        Debug.Log($"{stats.CharacterName} is using an Act skill.");
        UseAct();
        Debug.Log($"{stats.CharacterName} has used an Act skill.");

        AfterAct?.Invoke(this);
    }
    void UseAct()
    {
          Debug.Log($"{stats.CharacterName} is attempting to use an Act skill.");
        foreach (Skill skill in Skills)
        {
            Debug.Log($"Checking skill: {skill.Name} of type {skill.Type} for {stats.CharacterName}");
            if (skill.Type == SkillType.Act && skill.CheckAndTriggerSkill())
            {
                Debug.Log($"{stats.CharacterName} used the ACT skill: {skill.Name}");
                return;  // Exits the method after the first successful skill usage
            }
        }
        Debug.Log($"{stats.CharacterName} has no available Act skills to use.");
    }

    public void TriggerBeforeUlt()
    {
        Debug.Log($"{stats.CharacterName} is about to use an Ult skill.");
        BeforeUlt?.Invoke(this);
    }
    public void TriggerOnUlt()
    {
        Debug.Log($"{stats.CharacterName} is using an Ult skill.");
        UseUlt();
        Debug.Log($"{stats.CharacterName} has used an Ult skill.");
        AfterUlt?.Invoke(this);
    }

    void UseUlt()
    {
        Debug.Log($"{stats.CharacterName} is attempting to use an Ult skill.");
        foreach (Skill skill in Skills)
        {
            Debug.Log($"Checking skill: {skill.Name} of type {skill.Type} for {stats.CharacterName}");
            if (skill.Type == SkillType.Ult && skill.CheckAndTriggerSkill())
            {
                Debug.Log($"{stats.CharacterName} used the ULT skill: {skill.Name}");
                return;  // Exits the method after the first successful skill usage
            }
        }
        Debug.Log($"{stats.CharacterName} has no available ULT skills to use.");
    }
    public void TriggerBeforeStrike()
    {
        BeforeStrike?.Invoke(this);
    }
    public void TriggerOnStrike()
    {
        AfterStrike?.Invoke(this);
    }
    public void TriggerBeforeRest()
    {
        BeforeRest?.Invoke(this);
    }
    public void TriggerOnRest()
    {
        AfterRest?.Invoke(this);
    }

    public void TriggerOnHealGiven(float HealAmt)
    {
        OnDealHeal?.Invoke(HealAmt);
    }
    public void TriggerOnHealTaken(float HealAmt)
    {
        OnTakeHeal?.Invoke(HealAmt);
    }

    public void TriggerTakeDamage(float damage, DamageSourceType sourceType)
    {
        OnTakeDamage?.Invoke(damage, sourceType);
    }
    public void TriggerTakeCrit(float Damage)
    {
        OnTakeCrit?.Invoke(Damage);
    }
    public void TriggerTakeNonCrit(float Damage)
    {
        OnTakeNonCrit?.Invoke(Damage);
    }
    public void TriggerDealDamage(float damage, DamageSourceType sourceType)
    {
        // This is where you would trigger the event for damage dealt
        OnDealDamage?.Invoke(damage, sourceType);
    }
    public void TriggerDealCrit(float Damage)
    {
        OnDealCrit?.Invoke(Damage);
    }
    public void TriggerDealNonCrit(float Damage)
    {
        OnDealNonCrit?.Invoke(Damage);
    }

    public void TriggerTakeDrain(float Drain)
    {
        OnTakeDrain?.Invoke(Drain);
    }
    public void TriggerConsumeStamina(float Drain)
    {
        OnConsumeStamina?.Invoke(Drain);
    }
    public void TriggerDealDrain(float Drain)
    {
        OnDealDrain?.Invoke(Drain);
    }
    public void TriggerTakeRecovery(float Recovery)
    {
        OnTakeRecovery?.Invoke(Recovery);
    }
    public void TriggerDealRecovery(float Recovery)
    {
        OnDealRecovery?.Invoke(Recovery);
    }

    public void TriggerOnIncapacitated()
    {
        OnIncapacitated?.Invoke(this);
    }
    public void TriggerOnExhausted()
    {
        OnExhausted?.Invoke(this);
    }

    // Check and trigger threshold events
    public void CheckHPThreshold(float threshold)
    {
        // Corrected calculation for percentage threshold
        if (stats.MaxHealth > 0 && stats.currentHealth / stats.MaxHealth < threshold)
        {
            OnHPThresholdReached?.Invoke(threshold);
        }
    }

    public void CheckStaminaThreshold(float threshold)
    {
        // Corrected calculation for percentage threshold
        if (stats.MaxStamina > 0 && stats.CurrentStamina / stats.MaxStamina < threshold)
        {
            OnStaminaThresholdReached?.Invoke(threshold);
        }
    }

    public bool Talentready(Skill skill)
    {
        return (skill == null || skill.Type.Equals(Skill.SkillType.Talent) && skill.CDTimer <= 0);

    }
}