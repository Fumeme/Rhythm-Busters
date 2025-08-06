using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatusEffect//: MonoBehaviour
{

    public string StatusName = "Arson";
    [SerializeField] private Sprite icon;
    [TextArea] public string Description;
    public Character owner;
    public bool HasOverTimeEffect()
    {
        foreach (StatModifier.StatEffect effect in statEffects)
        {
            if (effect.dotType != StatModifier.StatEffect.DotType.None)
            {
                Debug.Log($"found Dot of: {effect.statName} - type: {effect.dotType}");
                return true;
            }
        }
        return false;
    }

    [SerializeField] int posativetiers = 1;
    [SerializeField] int negativetiers = 0;
    [SerializeField] int neutraltiers = 0;
    [SerializeField] public List<Stack> Stacks = new List<Stack>();
    [SerializeField] bool hasDuration = true;


    public List<StatModifier.StatEffect> statEffects = new List<StatModifier.StatEffect>();
    public StatusEffect(StatModifier statModifier_, Stack stack, Character owner_)
    {
        /// get all the stat effects from the statModifier
        statEffects = statModifier_.statEffects;

        StatusName = statModifier_.StatusName;
        Description = statModifier_.Description;
        hasDuration = statModifier_.Duration > 0;
        Stacks.Add(stack);
        negativetiers = GetTiers(EffectSign.Negative);
        posativetiers = GetTiers(EffectSign.Positive);
        neutraltiers = GetTiers(EffectSign.NoSign);
        this.owner = owner_;
    }


    public int GetTiers(EffectSign sign)
    {
        // Initialize the temporary variables to store tiers for each sign
        int posativetiers_temp = 0;
        int negativetiers_temp = 0;
        int neutraltiers_temp = 0;

        // Loop through all the stacks
        foreach (Stack stack in Stacks)
        {
            // Only add the tier if the stack's sign matches the provided 'sign'
            if (stack.sign == EffectSign.Positive)
            {
                posativetiers_temp += stack.Tier;
            }
            else if (stack.sign == EffectSign.Negative)
            {
                negativetiers_temp += stack.Tier;
            }
            else if (stack.sign == EffectSign.NoSign)
            {
                neutraltiers_temp += stack.Tier;
            }
            else if (stack.sign != EffectSign.ALL)
            {
                Debug.LogError($"Unknown sign {stack.sign} in {StatusName}");
                continue;
            }


        }
        negativetiers = negativetiers_temp;
        posativetiers = posativetiers_temp;
        neutraltiers = neutraltiers_temp;

        // Return the tier count for the specific sign requested
        if (sign == EffectSign.Positive)
        {
            return posativetiers_temp;
        }
        else if (sign == EffectSign.Negative)
        {
            return negativetiers_temp;
        }
        else if (sign == EffectSign.NoSign)
        {
            return neutraltiers_temp;
        }
        else if (sign == EffectSign.ALL)
        {
            return posativetiers_temp + negativetiers_temp + neutraltiers_temp;
        }

        // If no specific sign, return 0 (which shouldn't happen in normal usage)
        Debug.LogError($"No sign found for {sign} in {StatusName}");
        return 0;
    }



    public void Tick(int amount = 1)
    {
        // List to hold stacks that need to be removed
        List<Stack> stacksToRemove = new List<Stack>();

        // Loop through all stacks
        foreach (Stack stack in Stacks)
        {
            // Decrease the timer by the specified amount
            stack.Timer -= amount;

            // Check if the stack has expired
            if (stack.Tier <= 0 || stack.Timer <= 0) // Also check if the timer has expired
            {
                stacksToRemove.Add(stack);
            }
        }

        // Remove expired stacks after the loop to avoid modifying the collection during iteration
        foreach (Stack stack in stacksToRemove)
        {
            Stacks.Remove(stack);
        }

        if (Stacks.Count <= 0)
        {
            // If all stacks are removed, destroy the status effect
            Debug.Log($"Status effect {StatusName} on {owner.stats.CharacterName} has expired.");
            owner.RemoveStatusEffect(this);
            return;
        }

        // Update tiers after removing expired stacks
        negativetiers = GetTiers(EffectSign.Negative);
        posativetiers = GetTiers(EffectSign.Positive);
        neutraltiers = GetTiers(EffectSign.NoSign);
    }
    public void TickDoTEffect()
    {
        if (owner == null || statEffects == null || statEffects.Count == 0)
        {
            Debug.LogError($"DotEffect {StatusName} is not properly initialized.");
            return;
        }
        Debug.Log($"Ticking {StatusName} on {owner.stats.CharacterName}");
        float effectValue = 0;
        foreach (var effect in statEffects)
        {
            Debug.Log($"ticking dot Effect for: {effect.statName} - type: {effect.dotType}");

            effectValue = GetTiers(EffectSign.ALL) * owner.stats.MaxHealth * effect.InitialMultiplier;

            if (effect.dotType == StatModifier.StatEffect.DotType.None)
            {
                continue;
            }
            if (effect.dotType == StatModifier.StatEffect.DotType.Damage)
            {

                owner.TakeDamage(effectValue,Character.DamageSourceType.DoT, true);
                Debug.LogError($"---DOT--- Dealt {effectValue} damage to {owner.stats.CharacterName}");

            }
            if (effect.dotType == StatModifier.StatEffect.DotType.Heal)
            {

                owner.Heal(effectValue);
                //Debug.LogError($"---HOT--- Healed {effectValue} health to {owner.stats.CharacterName}");

                continue;
            }
            if (effect.dotType == StatModifier.StatEffect.DotType.Drain)
            {

                owner.TakeDrain(GetTiers(EffectSign.NoSign));

                Debug.LogError($"---DrOT--- Drained {effectValue} Sta to {owner.stats.CharacterName}");
                continue;
            }
            if (effect.dotType == StatModifier.StatEffect.DotType.Recovery)
            {

                owner.RecoverStamina(GetTiers(EffectSign.NoSign));
                Debug.LogError($"---ROT--- Recovered {effectValue} Sta to {owner.stats.CharacterName}");
                continue;

            }
        }
    }

    /// <summary>
    /// base formula: (5% * [Total Tiers] ) + (1% + 2% + ... + [Total Tiers]% )
    /// </summary>
    /// <returns>
    /// multiplier (float) to apply to the relavant stat
    /// </returns>
    public float CalculateModifier(string statName_ = "MaxHP")
    {
        foreach (StatModifier.StatEffect effect in statEffects)
        {
            if (effect.statName.ToUpper() == statName_.ToUpper())
            {
                float InitialMultiplier = effect.InitialMultiplier + 1; // Initial multiplier for all tiers
                float BaseMultiplier = effect.baseMultiplier; // Base multiplier for all tiers
                float TierBonusIncrement = effect.tierBonusIncrement; // Incremental bonus per tier


                // Cache tier counts to avoid redundant calculations
                int positiveTiers = GetTiers(EffectSign.Positive);
                int negativeTiers = GetTiers(EffectSign.Negative);
                int neutralTiers = GetTiers(EffectSign.NoSign);
                // Base multipliers
                float basePositiveMultiplier = (effect.baseMultiplier * positiveTiers) + effect.InitialMultiplier;
                float baseNegativeMultiplier = (effect.baseMultiplier * negativeTiers) + effect.InitialMultiplier;
                float baseNeutralMultiplier = (effect.baseMultiplier * neutralTiers) + effect.InitialMultiplier;
                // Calculate tier bonuses
                float positiveTierBonus = 0f;
                float negativeTierPenalty = 0f;
                // Neutral tiers calculation
                if (neutralTiers > 0)
                {
                    for (int i = 1; i <= neutralTiers; i++)
                    {
                        positiveTierBonus += (i * effect.tierBonusIncrement);
                    }
                    return baseNeutralMultiplier + positiveTierBonus + 1;
                }
                // Positive tiers bonus
                for (int i = 1; i <= positiveTiers; i++)
                {
                    positiveTierBonus += (i * effect.tierBonusIncrement);
                }
                // Negative tiers penalty
                for (int i = 1; i <= negativeTiers; i++)
                {
                    negativeTierPenalty += (i * effect.tierBonusIncrement);
                }
                // Combine effects, adjusting for sign
                return (basePositiveMultiplier + positiveTierBonus) -
                       (baseNegativeMultiplier + negativeTierPenalty) +
                       InitialMultiplier;
            }
        }
        //  Debug.LogError($"stat of {statName_} not found");
        return 1;

    }




    public void EnhanceStatus(int amount, EffectSign sign = EffectSign.Positive)
    {
        foreach (Stack stack in Stacks)
        {
            if (stack.sign == sign)
            {
                stack.Tier += amount;
            }
        }
    }

    public void ExtendStatus(int amount, EffectSign sign = EffectSign.Positive)
    {
        foreach (Stack stack in Stacks)
        {
            if (stack.sign == sign)
            {
                stack.Timer += amount;
            }
        }
    }

    public bool IsExpired()
    {
        if (Stacks == null || Stacks.Count == 0)
        {
            return true; // No stacks means the effect is expired
        }
        return Stacks.TrueForAll(stack => stack.Timer <= 0 || stack.Tier <= 0); // Check if all stacks are expired
    }


    // Enum for Positive, Negative, or Neutral
    public enum EffectSign
    {
        Negative = -1,
        NoSign = 0,
        Positive = 1,
        ALL
    }
    [System.Serializable]
    public class Stack
    {
        public Stack(int tier_, int timer_, EffectSign sign_)
        {
            Tier = Mathf.Max(tier_, 1);
            Timer = Mathf.Max(timer_, 0);
            sign = sign_;
        }
        public Stack()
        {
            Tier = 1;
            Timer = 5;
        }
        public EffectSign sign = EffectSign.Positive;
        public int Tier;           // Tiers in this stack
        public int Timer;        // Duration of this stack
    }

}
