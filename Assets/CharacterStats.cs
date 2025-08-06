using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    public enum ClassType
    {
        Attacker,
        Supporter,
        Buffer
    }

    public string CharacterName = "Triangle Man";
    public CharacterStats.ClassType Class = ClassType.Attacker;

    public float HealthRating;
    public float StaminaRating;
    public float SpeedRating;
    public float DefenseRating;
    public float MightRating;
    public float ArcaneRating;
    public float AccuracyRating;




    public float MaxHealth;
    public float currentHealth;
    public float MaxStamina;
    public float CurrentStamina;
    public float Defense;
    public float Speed;
    public float Might;
    public float Arcane;
    public float Critical;
    public int Accuracy;
    [SerializeField] private float SKILLPOWER;

    public enum CharacterState
    {
        Normal,
        Incapacitated, // Character is incapacitated and cannot act
        Exhausted     // Character is exhausted and cannot act
    }
    public CharacterState State = CharacterState.Normal;
    public void SetState(CharacterState newState)
    {
        if (State == newState)
        {
            // No change in state, do nothing
            return;
        }

        // --- State Transition Logic ---

        // Rule 1: Cannot change from Incapacitated to anything but Normal.
        if (State == CharacterState.Incapacitated && newState != CharacterState.Normal)
        {
            Debug.LogWarning($"Cannot change state from {State} to {newState}. Only transition to Normal is allowed from Incapacitated.");
            return;
        }

        // --- NEW: Cleanse status effects when leaving Normal state ---
        if (State == CharacterState.Normal && (newState == CharacterState.Exhausted || newState == CharacterState.Incapacitated))
        {
            CleansesAllStatuses();
            Debug.Log($"{CharacterName} left Normal state and all status effects were cleansed.");
        }

        State = newState;

        // Additional logic can be added here if needed when the state changes
        Debug.Log($"Character state changed to: {State}");
    }



    // --- NEW: Method to cleanse all status effects ---
    public void CleansesAllStatuses()
    {
        if (Effects.Count > 0)
        {
            // Create a copy to iterate, remove from original
            List<StatusEffect> effectsToClear = new List<StatusEffect>(Effects);
            foreach (StatusEffect status in effectsToClear)
            {
                if (Effects.Contains(status)) // Check if it's still there
                {
                    Effects.Remove(status);
                    // Optionally call an OnRemoved method on the status effect if it needs cleanup
                    // status.OnRemoved();
                    Debug.Log($"Cleansed status: {status.StatusName} from {CharacterName}.");
                }
            }
            CalculateStats(); // Recalculate stats after cleansing
        }
    }

    // --- NEW: Method to check if a character can receive a new status effect ---
    public bool CanReceiveStatus()
    {
        return State == CharacterState.Normal;
    }

    // --- NEW: Methods to check for damage/drain immunity ---
    public bool CanReceiveDamage()
    {
        return State != CharacterState.Incapacitated;
    }

    public bool CanReceiveDrain()
    {
        return State != CharacterState.Incapacitated && State != CharacterState.Exhausted;
    }
    public float getSkillpower(Lane.MainStat mainStat)
    {
        float SP = 0;
        if (mainStat == Lane.MainStat.Arcane)
        {
            SP = (Arcane + (Might * .35f));
        }
        else if (mainStat == Lane.MainStat.Might)
        {
            SP = (Might + (Arcane * .35f));

        }
        else
        {
            Debug.LogError("main stat that isn't might or arcane detected");
        }
        if (State == CharacterState.Exhausted)
        {
            SKILLPOWER = SP * 0.3f;
        }
        else
        {
            SKILLPOWER = SP * SkillPowerModifier;
        }
        return SKILLPOWER;
    }
    public enum StatType
    {
        MaxStamina, Endurance, MaxHealth, Speed, Arcane, Critical, Might, Accuracy
    }



    public float ActModifier = 1.0f,
         UltModifer = 1.0f,
         TalentModifier = 1.0f,
         SpeedModifier = 1.0f,
         MightModifer = 1.0f,
         ArcaneModifer = 1.0f,
         AccuracyModifier = 1.0f,
         DefenceModifier = 1.0f,
         MaxHPModifier = 1.0f,
         MaxStaModifier = 1.0f,
         RestModifier = 1.0f,
         StrikeModifier = 1.0f,
         StaConsumeModifier = 1.0f,
         DrainDealtModifier = 1.0f,
         DrainRecievedModifier = 1.0f,
         DmgRecievedModifier = 1.0f,
         DmgDealtModifier = 1.0f,
         RecoveryRecievedModifier = 1.0f,
         RecoveryDealtModifier = 1.0f,
         HealRecievedModifier = 1.0f,
         HealDealtModifier = 1.0f,
         SkillPowerModifier = 1.0f;

    CharacterStats BaseStats;
    public List<StatusEffect> Effects = new List<StatusEffect>();

    // Use the scaling factors determined earlier based on your target values:
    const float HP_SCALE = 100f;       // 300 rating * 100 = 30000 HP
    const float STAMINA_SCALE = 10f;   // 220 rating * 10 = 2200 Stamina
    const float DEFENSE_SCALE = 1f;    // 700 rating * 1 = 700 Defense
    const float SPEED_SCALE = 0.275f;    // 800 rating * 0.1 = 80 Speed
    const float M_A_SCALE = 1f;        // 900 rating * 1 = 900 Might/Arcane
    const float ACCURACY_SCALE = 10f;  // 140 rating * 10 = 1400 Accuracy

    /// <summary>
    /// Assigns a letter grade to the character's current stat.
    /// </summary>
    /// <returns>A string representing the grade (D, C, B, A, S, SS).</returns>
    public string GetStatGrade(float stat)
    {
        if (stat >= 1900)
        {
            return "SS";
        }
        else if (stat >= 1400)
        {
            return "S";
        }
        else if (stat >= 1000)
        {
            return "A";
        }
        else if (stat >= 600)
        {
            return "B";
        }
        else if (stat >= 300)
        {
            return "C";
        }
        else
        {
            return "D";
        }
    }
    public void SetBaseStats()
    {
        BaseStats = new CharacterStats()
        {
            HealthRating = this.HealthRating,
            StaminaRating = this.StaminaRating,
            SpeedRating = this.SpeedRating,
            DefenseRating = this.DefenseRating,
            MightRating = this.MightRating,
            ArcaneRating = this.ArcaneRating,
            AccuracyRating = this.AccuracyRating,
            Critical = this.Critical,
            CharacterName = this.CharacterName,
            Class = this.Class,

        };

        BaseStats.MaxHealth = HealthRating * HP_SCALE;
        BaseStats.MaxStamina = StaminaRating * STAMINA_SCALE;
        BaseStats.Defense = DefenseRating * DEFENSE_SCALE;
        BaseStats.Speed = SpeedRating * SPEED_SCALE;
        BaseStats.Might = MightRating * M_A_SCALE;
        BaseStats.Arcane = ArcaneRating * M_A_SCALE;
        BaseStats.Accuracy = Mathf.CeilToInt(AccuracyRating * ACCURACY_SCALE);

    }

    public void TickAllStatuses(int amount = 1)
    {
        // Create a copy of the list to iterate over.
        // This prevents the "Collection was modified" error if effects are removed/added during the loop.
        List<StatusEffect> currentEffects = new List<StatusEffect>(Effects);

        // Use a list to store effects that need to be removed after the loop
        List<StatusEffect> effectsToRemove = new List<StatusEffect>();

        foreach (StatusEffect status in currentEffects) // Iterate over the copy
        {
            if (status == null)
            {
                // If a null status somehow gets in, mark it for removal or just continue.
                // It's safer to remove null entries to prevent future errors.
                Debug.LogError("Null StatusEffect found in Effects list during TickAllStatuses. Removing.");
                effectsToRemove.Add(status);
                continue;
            }

            //Debug.Log($"ticking {status.StatusName} for {amount}");

            if (status.HasOverTimeEffect())
            {
                //Debug.Log($"ticking DoT of {status.StatusName}");

                // The original 'if (status == null)' check here is redundant because
                // we already checked 'status == null' at the beginning of the loop
                // and added it to effectsToRemove.
                // Removing it to simplify.

                if (status.owner == null || status.statEffects == null || status.statEffects.Count == 0)
                {
                    Debug.LogError($"DotEffect {status.StatusName} is not properly initialized or is missing owner/statEffects. Marking for removal.");
                    effectsToRemove.Add(status); // Mark for removal if not properly initialized
                    continue;
                }
                status.TickDoTEffect();
            }
            else
            {
                // Debug.Log($"ticking {status.StatusName}");
            }

            status.Tick(amount); // This method should update the status's internal duration

            // Assuming your StatusEffect class has a property like IsExpired
            // or a method to check if it's finished.
            // If status.Tick() removes the status internally, you might not need this.
            // However, typically, Tick() updates duration, and then you check for expiration.
            if (status.IsExpired()) // You will need to add an IsExpired method/property to StatusEffect.cs
            {
                effectsToRemove.Add(status);
                Debug.Log($"StatusEffect {status.StatusName} has expired. Marking for removal.");
            }
        }

        // Now, remove all expired/invalid effects from the original list *after* the loop
        foreach (StatusEffect expiredStatus in effectsToRemove)
        {
            if (Effects.Contains(expiredStatus)) // Check if it's still in the original list before removing
            {
                Effects.Remove(expiredStatus);
                // Optionally, call a Cleanup method on the status itself if it needs to unsubscribe from events etc.
                // expiredStatus.OnRemoved(); // Assuming an OnRemoved method exists on StatusEffect
                Debug.Log($"Removed expired status: {expiredStatus.StatusName}");
            }
        }

        CalculateStats(); // Calculate stats after all statuses have been ticked and removed
    }

    public void ResetModifiers()
    {         // Reset all stats to their base values

        MaxHPModifier = 1.0f;
        MaxStaModifier = 1.0f;
        SpeedModifier = 1.0f;
        MightModifer = 1.0f;
        ArcaneModifer = 1.0f;
        AccuracyModifier = 1.0f;
        DefenceModifier = 1.0f;
        RestModifier = 1.0f;
        StrikeModifier = 1.0f;
        StaConsumeModifier = 1.0f;
        DrainDealtModifier = 1.0f;
        DrainRecievedModifier = 1.0f;
        DmgRecievedModifier = 1.0f;
        DmgDealtModifier = 1.0f;
        RecoveryRecievedModifier = 1.0f;
        RecoveryDealtModifier = 1.0f;
        HealDealtModifier = 1.0f;
        HealRecievedModifier = 1.0f;
        SkillPowerModifier = 1.0f;
    }


    public void CalculateStats()
    {
        if (BaseStats == null)
        {
            Debug.LogError("BaseStats is null. Cannot calculate stats.");
            return;
        }
        ResetModifiers();


        // Apply status effects that modify specific stats
        if (Effects.Count > 0 && State.Equals(CharacterState.Normal))
        {
            foreach (StatusEffect status in Effects)
            {
                MaxHPModifier += status.CalculateModifier("MAXHP") - 1.0f;
                MaxStaModifier += status.CalculateModifier("MAXSTA") - 1.0f;
                SpeedModifier += status.CalculateModifier("SPEED") - 1.0f;
                MightModifer += status.CalculateModifier("MIGHT") - 1.0f;
                ArcaneModifer += status.CalculateModifier("ARCANE") - 1.0f;
                AccuracyModifier += status.CalculateModifier("ACCURACY") - 1.0f;
                DefenceModifier += status.CalculateModifier("DEFENCE") - 1.0f;
                RestModifier += status.CalculateModifier("REST") - 1.0f;
                StrikeModifier += status.CalculateModifier("STRIKE") - 1.0f;
                StaConsumeModifier += status.CalculateModifier("STACONSUME") - 1.0f;
                DrainDealtModifier += status.CalculateModifier("DRAINDEALT") - 1.0f;
                DrainRecievedModifier += status.CalculateModifier("DRAINRECIEVED") - 1.0f;
                DmgRecievedModifier += status.CalculateModifier("DMGRECIEVED") - 1.0f;
                DmgDealtModifier += status.CalculateModifier("DMGDEALT") - 1.0f;
                RecoveryRecievedModifier += status.CalculateModifier("RECOVERYRECIEVED") - 1.0f;
                RecoveryDealtModifier += status.CalculateModifier("RECOVERYDEALT") - 1.0f;
                HealDealtModifier += status.CalculateModifier("HEALDEALT") - 1.0f;
                HealRecievedModifier += status.CalculateModifier("HEALRECIEVED") - 1.0f;
                SkillPowerModifier += status.CalculateModifier("SKILLPOWER") - 1.0f;
                TalentModifier += status.CalculateModifier("TALENT") - 1.0f;
                UltModifer += status.CalculateModifier("ULT") - 1.0f;
            }
        }
        // Apply state-based modifiers
        if (State.Equals(CharacterState.Exhausted))
        {
            MaxHealth = BaseStats.MaxHealth * 0.75f; // Reduce max health by 25% when exhausted
            Defense = BaseStats.Defense * 0.75f; // Reduce defense by 25% when exhausted
            Speed = BaseStats.Speed * 0.25f; // Reduce speed by 75% when exhausted
        }
        else if (State.Equals(CharacterState.Incapacitated))
        {
            MaxHealth = BaseStats.MaxHealth * MaxHPModifier;
            Defense = BaseStats.Defense * DefenceModifier;
            Speed = BaseStats.Speed * 0.1f; // Reduce speed by 90% when incapacitated
        }
        else
        {
            MaxHealth = BaseStats.MaxHealth * MaxHPModifier;
            Defense = BaseStats.Defense * DefenceModifier;
            Speed = (BaseStats.Speed * SpeedModifier) + 5;
        }

        // Apply the calculated modifiers to the character stats
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        MaxStamina = BaseStats.MaxStamina * MaxStaModifier;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);

        Might = BaseStats.Might * MightModifer;
        Accuracy = Mathf.CeilToInt(BaseStats.Accuracy * AccuracyModifier);
        Arcane = BaseStats.Arcane * ArcaneModifer;


        // NEW CLAMPING LOGIC:
        // Clamp current health/stamina based on the *current state*.
        // This ensures that when a character is in 'Normal' state, their current health/stamina
        // does not exceed their MaxHealth/MaxStamina. When they transition *into* Normal
        // from Incapacitated/Exhausted, the Character.Heal/RecoverStamina methods already handle
        // the 150% cap and transition, but this ensures that if MaxHealth *changes*
        // due to effects *while* in Normal, the current value is correctly re-clamped.
        // It also ensures that if a character is *not* Incapacitated/Exhausted, their values are capped at Max.
        if (State != CharacterState.Incapacitated)
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
        }
        else // If Incapacitated, allow up to 150% for recovery purposes (matching Character.Heal)
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth * 1.5f);
        }

        if (State != CharacterState.Exhausted)
        {
            CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
        }
        else // If Exhausted, allow up to 150% for recovery purposes (matching Character.RecoverStamina)
        {
            CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina * 1.5f);
        }

        // Debug.Log($"finished modifying stats");
    }




    public Character.Affiliation CharAffil = Character.Affiliation.Player;


    public short Forward()
    {
        if (CharAffil == Character.Affiliation.Player)
        {
            return (short)1;
        }
        return (short)-1;

    }

}
