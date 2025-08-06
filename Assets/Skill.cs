using System;
using System.Collections.Generic;
using UnityEngine; // Make sure this is included for Debug.Log

[System.Serializable]
public class Skill
{
    public enum SkillType { Talent, Ult, Act }
    public enum Targeting { EnemyFirst, AllyFirst, SameLane, EnemyInclusiveAdjacent, EnemyExclusiveAdjacent, AllyInclusiveAdjacent, AllyExclusiveAdjacent, EnemyFrontRow, EnemyBackRow, AllyFrontRow, AllyBackRow, EnemyCentre, AllyCentre, Self }

    [SerializeField] public string Name; // Visible in Inspector
    [SerializeField] public string Description; // Visible in Inspector
    [SerializeField] public SkillType Type; // Visible in Inspector
    [SerializeField] public float StaminaCost; // Visible in Inspector
    [SerializeField] public int Cooldown; // Visible in Inspector
    [SerializeField] public int CDTimer; // Visible in Inspector
    [SerializeField] public List<SkillEffect> Effects = new List<SkillEffect>(); // Visible in Inspector
    [SerializeField] public List<SkillCondition> Conditions = new List<SkillCondition>(); // Visible in Inspector

    public Character Owner { get; set; }

    public Skill(string name, string description, SkillType type, float staminaCost, int cooldown, Character owner, int warmup = 0)
    {
        Name = name;
        Description = description;
        Type = type;
        StaminaCost = staminaCost;
        Cooldown = cooldown;
        CDTimer = warmup;
        Owner = owner;
    }

    // Constructor to initialize from ScriptableObject
    public Skill(SkillDefinition definition, Character owner_)
    {
        if (definition == null) return;
        Name = definition.SkillName;
        Description = definition.description;
        Cooldown = definition.cooldown;
        CDTimer = definition.warmup;
        StaminaCost = definition.cost;
        Type = definition.skillType;
        Owner = owner_;

        this.RegisterSkillEff(definition);
    }

    private void RegisterSkillEff(SkillDefinition definition)
    {
        Effects.Clear();
        foreach (SkillEffectDefinition effectDef in definition.effects)
        {
            if (effectDef == null) Debug.LogError($"{Name} has null effect deff");
            Effects.Add(new SkillEffect(effectDef, Owner));
        }
    }

    public void RegisterEvents()
    {
        Owner.AfterNode += ReduceCooldown;
        // Specific skill types trigger when their corresponding node type is activated
        switch (Type)
        {
            case SkillType.Act:
                Owner.AfterAct += CheckAndTriggerSkillFromEvent; // Subscribe to OnAct event
                Debug.Log($"[Skill] Skill '{Name}' (Act) registered for OnAct event on {Owner.stats.CharacterName}.");
                break;
            case SkillType.Ult:
                Owner.AfterUlt += CheckAndTriggerSkillFromEvent; // Subscribe to OnUlt event
                Debug.Log($"[Skill] Skill '{Name}' (Ult) registered for OnUlt event on {Owner.stats.CharacterName}.");
                break;
            case SkillType.Talent:
                // Talent skills are handled by ReduceCooldown checking CDTimer <= 0
                Debug.Log($"[Skill] Skill '{Name}' (Talent) will check for trigger when cooldown is reduced.");
                break;
        }
    }

    private void CheckAndTriggerSkillFromEvent(Character character)
    {
        // This will call the main CheckAndTriggerSkill() method
        // You might want to pass 'Owner' or 'this.Owner' if triggeringCharacter is used in effects
        CheckAndTriggerSkill(character); // Assuming Owner is the triggering character for its own skill
    }

    public void ReduceCooldown(Character character)
    {
        CDTimer = Mathf.Max(CDTimer - 1, 0);
        // Debug.Log("Cooldown reduced"); // You can uncomment this if you want to see all cooldown reductions
        if (Type == SkillType.Talent && IsActivatable()) CheckAndTriggerSkill(); // Talents might trigger on cooldown, others on node activation
    }

    public bool CheckAndTriggerSkill(Character triggeringCharacter = null)
    {
        // *** ADDED DEBUG LOG HERE ***
        Debug.Log($"[Skill Trigger Attempt] Skill '{Name}' (Type: {Type}) called CheckAndTriggerSkill for {Owner.stats.CharacterName}.");

        if (IsActivatable())
        {
            UseSkill(triggeringCharacter);
            // *** ADDED DEBUG LOG HERE ***
            Debug.Log($"[Skill Trigger] Skill '{Name}' (Type: {Type}) successfully triggered by {Owner.stats.CharacterName}!");
            return true;
        }
        else
        {
            // *** ADDED DEBUG LOG HERE ***
            Debug.LogWarning($"[Skill Trigger] Skill '{Name}' (Type: {Type}) not activatable for {Owner.stats.CharacterName}. Conditions: Cooldown={CDTimer}, Stamina={Owner.stats.CurrentStamina}/{FinalSkillCost()}, AreConditionsMet={AreConditionsMet()}.");
            return false;
        }
    }

    public float FinalSkillCost()
    {
        return StaminaCost * Owner.stats.StaConsumeModifier;
    }

    public bool IsActivatable()
    {
        bool activatable = CDTimer <= 0 && Owner.stats.CurrentStamina >= FinalSkillCost() && AreConditionsMet();

        // *** ADDED DEBUG LOG HERE ***
        //Debug.Log($"[Skill Check] Skill '{Name}' (Type: {Type}) IsActivatable check for {Owner.stats.CharacterName}: " +
        //          $"CDTimer={CDTimer} (needs 0), " +
        //          $"Stamina={Owner.stats.currentStamina} (needs {FinalSkillCost()}), " +
        //          $"ConditionsMet={AreConditionsMet()}. " +
        //          $"Result: {activatable}");

        return activatable;
    }

    // This method is crucial for IsActivatable() and was missing in previous snippet, ensuring it's here.
    private bool AreConditionsMet()
    {
        if (Conditions == null || Conditions.Count <= 0)
        {
            return true; // No conditions, so they are met
        }
        return Conditions.TrueForAll(cond => cond.IsConditionMet());
    }

    private void UseSkill(Character triggeringCharacter = null)
    {
        Character.DamageSourceType skillDamageSource = Character.DamageSourceType.None;
        switch (Type)
        {
            case SkillType.Act:
                skillDamageSource = Character.DamageSourceType.Act;
                break;
            case SkillType.Ult:
                skillDamageSource = Character.DamageSourceType.Ult;
                break;
            case SkillType.Talent:
                skillDamageSource = Character.DamageSourceType.Talent;
                break;
            default:
                skillDamageSource = Character.DamageSourceType.Other;
                break;
        }

        foreach (SkillEffect effect in Effects)
        {
            List<Character> characters = effect.GetTargets(Owner, triggeringCharacter);
            effect.TriggerEffect(characters, Type, skillDamageSource);
        }
        CDTimer = Cooldown;
        Owner.ConsumeStamina(FinalSkillCost());

        switch (Type)
        {
            case SkillType.Talent:
                Owner.GetComponent<CharacterParticlesFX>().Emit(CharacterParticlesFX.ParticleColor.Blue);
                break;
            case SkillType.Ult:
                Owner.GetComponent<CharacterParticlesFX>().Emit(CharacterParticlesFX.ParticleColor.Purple);
                break;
            case SkillType.Act:
                Owner.GetComponent<CharacterParticlesFX>().Emit(CharacterParticlesFX.ParticleColor.Red);
                break;
            default:
                break;
        }

        Debug.Log($"{Name} skill used. Cooldown reset to {Cooldown}.");
    }

    public void AddEffect(SkillEffect effect) => Effects.Add(effect);
    public void AddCondition(SkillCondition condition) => Conditions.Add(condition);
}