using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static Character;
using static Skill;



[Serializable]
public class SkillEffect
{
    [SerializeField] private SkillEffectDefinition effectDef;
    private Character owner,
        Enemy1, Enemy2, Enemy3, Enemy4, Enemy5,
        Player1, Player2, Player3, Player4, Player5;



    public enum EffectType
    {
        Damage, Heal,
        Drain, Recover,
        Buff, Debuff,
        CDRALL, CDRONE,
        Enhance, Extend,
        DistributeDmgDealt, // NEW: Added DistributeDmgDealt effect type
        StrikeTrigger, // NEW: Added StrikeTrigger effect type
    }
    public string Name;
    public EffectType Type;
    public float Power;           // Determines the potency of the effect (e.g., damage amount, healing)
    public CharacterStats AttackerStats;
    public Lane Lane;
    public int Duration;        // Duration for buffs or debuffs, in nodes

    public Character Effecttarget;

    // NEW: Field to specify the required damage source type for DistributeDmgDealt
    public Character.DamageSourceType RequiredDamageSource;


    public SkillEffect()
    {
        Power = 5.0f;
        Duration = 5;
        RequiredDamageSource = Character.DamageSourceType.None; // Default value

        SetupLanes();
    }
    public enum Targeting
    {
        EnemyFirst, AllyFirst, SameLane,
        EnemyInclusiveAdjacent, EnemyExclusiveAdjacent, AllyInclusiveAdjacent, AllyExclusiveAdjacent,
        EnemyFrontRow, EnemyBackRow, AllyFrontRow, AllyBackRow, EnemyCentre, AllyCentre, Self, AllAllies, AllEnemies,
        TriggeringAlly
    }
    public Targeting TargetingType; // New targeting property for each effect
    StatusEffect.EffectSign StatusSign;
    List<StatModifier> StatusEffectToApply;
    public List<CharacterStats.ClassType> TargettingFilter;
    public SkillEffect(EffectType type, float power, CharacterStats attackerStats_, Lane lane, string name = "do thing", Targeting targeting = Targeting.EnemyFirst, int duration = 5)
    {
        Name = name;
        Type = type;
        Power = power;
        AttackerStats = attackerStats_;
        Lane = lane;
        Duration = duration;
        TargetingType = targeting;
        RequiredDamageSource = Character.DamageSourceType.None; // Default value


        SetupLanes();
    }
    public List<StatusScalingEntry> statusScalings;
    public SkillEffect(SkillEffectDefinition effectDef, Character owner_, StatusEffectScaling scaling = null)
    {
        this.owner = owner_;
        Name = effectDef.SkillEffectName;
        Power = effectDef.power;
        Type = effectDef.effectType;
        AttackerStats = owner_.stats;
        Lane = owner_.lane;
        Duration = effectDef.duration;
        TargetingType = effectDef.targetting;
        StatusSign = effectDef.sign;
        StatusEffectToApply = effectDef.StatusesToApply;
        TargettingFilter = effectDef.TargettingFilter;
        RequiredDamageSource = effectDef.requiredDamageSource; // NEW: Initialize RequiredDamageSource
        if (scaling != null)
        {
            statusScalings = scaling.statusScalings;
        }

        SetupLanes();
    }

    private void SetupLanes()
    {
        if (LanesManager.Instance == null)
        {
            Debug.LogError("LanesManager.Instance is null in SkillEffect.SetupLanes()");
            Player1 = Player2 = Player3 = Player4 = Player5 = null;
            Enemy1 = Enemy2 = Enemy3 = Enemy4 = Enemy5 = null;
            return;
        }

        Player1 = LanesManager.Instance.GetCharacter(LanesManager.LaneID.First, Affiliation.Player);
        Player2 = LanesManager.Instance.GetCharacter(LanesManager.LaneID.Second, Affiliation.Player);
        Player3 = LanesManager.Instance.GetCharacter(LanesManager.LaneID.Third, Affiliation.Player);
        Player4 = LanesManager.Instance.GetCharacter(LanesManager.LaneID.Fourth, Affiliation.Player);
        Player5 = LanesManager.Instance.GetCharacter(LanesManager.LaneID.Fifth, Affiliation.Player);

        Enemy1 = LanesManager.Instance.GetCharacter(LanesManager.LaneID.First, Affiliation.Enemy);
        Enemy2 = LanesManager.Instance.GetCharacter(LanesManager.LaneID.Second, Affiliation.Enemy);
        Enemy3 = LanesManager.Instance.GetCharacter(LanesManager.LaneID.Third, Affiliation.Enemy);
        Enemy4 = LanesManager.Instance.GetCharacter(LanesManager.LaneID.Fourth, Affiliation.Enemy);
        Enemy5 = LanesManager.Instance.GetCharacter(LanesManager.LaneID.Fifth, Affiliation.Enemy);
    }



    // Method to get the appropriate targets for this effect based on TargetingType
    public List<Character> GetTargets(Character owner, Character TriggeringCharacter = null)
    {
        Affiliation owneraffil;
        if (owner.stats.CharAffil == Affiliation.Player)
        {
            owneraffil = Affiliation.Player;
        }
        else
        {
            owneraffil = Affiliation.Enemy;
        }

        List<Character> Enemies, Allies;


        if (owneraffil == Affiliation.Player)
        {
            Allies = LanesManager.Instance.PlayerCharacters;
            Enemies = LanesManager.Instance.EnemyCharacters;
        }
        else
        {
            Enemies = LanesManager.Instance.PlayerCharacters;
            Allies = LanesManager.Instance.EnemyCharacters;

        }



        SetupLanes();

        List<Character> targets = new List<Character>();
        switch (TargetingType)
        {
            case Targeting.TriggeringAlly:
                if (TriggeringCharacter != null)
                {
                    targets.Add(TriggeringCharacter);
                }
                break;
            case Targeting.AllyFirst:
                targets.Add(owner.FindTargetInOrder(owner.stats.CharAffil));
                break;
            case Targeting.EnemyFirst:
                targets.Add(owner.FindTargetInOrder((Affiliation)((int)owner.stats.CharAffil * -1)));
                break;
            case Targeting.SameLane:
                Affiliation targetAffiliation = (owneraffil == Affiliation.Player) ? Affiliation.Enemy : Affiliation.Player;
                targets.Add(owner.GetTargetInLane(owner.laneID, targetAffiliation, true)); // true to skip incapacitated
                break;
            case Targeting.EnemyInclusiveAdjacent:
                break;
            case Targeting.EnemyExclusiveAdjacent:
                break;
            case Targeting.AllyInclusiveAdjacent:
                break;
            case Targeting.AllyExclusiveAdjacent:
                break;
            case Targeting.EnemyFrontRow:
                if (owneraffil.Equals(Affiliation.Enemy))
                {
                    targets.Add(Player1);
                    targets.Add(Player2);
                }
                else
                {
                    targets.Add(Enemy1);
                    targets.Add(Enemy2);
                }
                break;
            case Targeting.EnemyBackRow:
                if (owneraffil.Equals(Affiliation.Enemy))
                {
                    targets.Add(Player3);
                    targets.Add(Player4);
                    targets.Add(Player5);
                }
                break;
            case Targeting.AllyFrontRow:
                if (owneraffil.Equals(Affiliation.Player))
                {
                    targets.Add(Player1);
                    targets.Add(Player2);
                }
                else
                {
                    targets.Add(Enemy1);
                    targets.Add(Enemy2);
                }
                break;
            case Targeting.AllyBackRow:
                if (owneraffil.Equals(Affiliation.Player))
                {
                    targets.Add(Player3);
                    targets.Add(Player4);
                    targets.Add(Player5);
                }
                else
                {
                    targets.Add(Enemy3);
                    targets.Add(Enemy4);
                    targets.Add(Enemy5);
                }
                break;
            case Targeting.EnemyCentre:
                if (owneraffil.Equals(Affiliation.Enemy))
                {
                    targets.Add(Player1);
                    targets.Add(Player2);
                    targets.Add(Player4);
                }
                else
                {
                    targets.Add(Enemy1);
                    targets.Add(Enemy2);
                    targets.Add(Enemy4);
                }
                break;
            case Targeting.AllyCentre:
                if (owneraffil.Equals(Affiliation.Player))
                {
                    targets.Add(Player1);
                    targets.Add(Player2);
                    targets.Add(Player4);
                }
                else
                {
                    targets.Add(Enemy1);
                    targets.Add(Enemy2);
                    targets.Add(Enemy4);
                }
                break;
            case Targeting.Self:
                targets.Add(owner);
                break;
            case Targeting.AllEnemies:
                targets.AddRange(Enemies);
                break;
            case Targeting.AllAllies:
                targets.AddRange(Allies);
                break;
            default:
                UnityEngine.Debug.LogError($"no targetting err");
                break;
        }

        if (TargettingFilter.Count <= 0 || TargettingFilter == null)
        {
            return targets.Where(t => t != null)
.Distinct().ToList();
        }

        return targets
.Where(t => t != null && TargettingFilter.Contains(t.stats.Class))
.Distinct().ToList();

    }


    // MODIFIED: Added DamageSourceType parameter
    public void TriggerEffect(List<Character> targets, Skill.SkillType skillType = SkillType.Talent, Character.DamageSourceType sourceOverride = Character.DamageSourceType.None)
    {
        UnityEngine.Debug.LogError($"{owner.stats.CharacterName} is doing {Name} of action type {Type} to {TargetingType}");

        foreach (Character target in targets)
        {
            if (target != null)
            {
                UnityEngine.Debug.LogError($"{target.stats.CharacterName} is targetted for {skillType} effect ");
                Character.DamageSourceType currentSource = sourceOverride;

                // Determine the actual source type if not overridden
                if (currentSource == Character.DamageSourceType.None)
                {
                    switch (skillType)
                    {
                        case SkillType.Ult:
                            currentSource = Character.DamageSourceType.Ult;
                            break;
                        case SkillType.Act:
                            currentSource = Character.DamageSourceType.Act;
                            break;
                        case SkillType.Talent:
                            currentSource = Character.DamageSourceType.Talent;
                            break;
                        default:
                            currentSource = Character.DamageSourceType.Other; // Default to Other if no specific type is set
                            break;
                    }
                }

                switch (Type)
                {
                    case EffectType.Damage:
                        DealDamage(target, Lane, AttackerStats,
                                   (skillType == SkillType.Ult) ? owner.stats.UltModifer :
                                   (skillType == SkillType.Act) ? owner.stats.ActModifier :
                                   owner.stats.TalentModifier,
                                   owner.stats.DmgDealtModifier,
                                   currentSource, // Pass the determined source type
                                   statusScalings);
                        break;
                    case EffectType.Heal:
                        HealTarget(target, AttackerStats, owner.stats.HealDealtModifier);
                        break;
                    case EffectType.Drain:
                        DrainTarget(target, AttackerStats, owner.stats.DrainDealtModifier);
                        break;
                    case EffectType.Recover:
                        RecoverTarget(target, AttackerStats, owner.stats.RecoveryDealtModifier);
                        break;
                    case EffectType.Buff:
                        ApplyStatusEffect(target, StatusEffect.EffectSign.Positive);
                        break;
                    case EffectType.Debuff:
                        ApplyStatusEffect(target, StatusEffect.EffectSign.Negative);
                        break;
                    case EffectType.CDRALL: target.CDRtoALL(Mathf.CeilToInt(Power)); break;
                    case EffectType.CDRONE: UnityEngine.Debug.LogError($"Cooldown Reduction Not Implemented to {target.stats.CharacterName}"); break;
                    case EffectType.Enhance:
                        EnchanceStatuses(new List<Character> { target });
                        break;
                    case EffectType.Extend:
                        ExtendStatuses(new List<Character> { target });
                        break;
                    case EffectType.DistributeDmgDealt:
                        Debug.LogWarning("DistributeDmgDealt effect is registered via event, not triggered directly."); // NEW
                        break;
                    case EffectType.StrikeTrigger:
                        // owner triggers their strike from the auto attackript
                        AutoAttack strike = owner.GetComponent<AutoAttack>();
                        strike.PerformAttack(Power / 100f); // Use the attack modifier from the character stats
                        break;


                    default: break;
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"{owner.stats.CharacterName} tried to affect target but is null");
            }
        }
    }

    public void EnchanceStatuses(List<Character> targets)
    {
        foreach (Character target in targets)
        {
            if (target == null) continue;
            foreach (StatusEffect status in target.stats.Effects)
            {

                StatusEffect.EffectSign sign = StatusSign;

                if (Power < 0)
                {
                    sign = StatusEffect.EffectSign.Negative;
                }
                else if (Power > 0)
                {
                    sign = StatusEffect.EffectSign.Positive;
                }

                status.EnhanceStatus(Mathf.CeilToInt(Power), sign);
                UnityEngine.Debug.LogError($"{Name} effect enhanced {status.StatusName} on {target.stats.CharacterName}");

            }
        }
    }
    public void ExtendStatuses(List<Character> targets)
    {
        foreach (Character target in targets)
        {
            if (target == null) continue;
            foreach (StatusEffect status in target.stats.Effects)
            {
                StatusEffect.EffectSign sign = StatusSign;
                if (Power < 0)
                {
                    sign = StatusEffect.EffectSign.Negative;
                }
                else if (Power > 0)
                {
                    sign = StatusEffect.EffectSign.Positive;
                }
                status.ExtendStatus(Mathf.CeilToInt(Power), sign);
                UnityEngine.Debug.LogError($"{Name} effect extended {status.StatusName} on {target.stats.CharacterName}");

            }
        }
    }

    [System.Serializable]
    public class StatusScalingEntry
    {
        public string statusName;
        public float scalingFactor;
        public StatusEffect.EffectSign effectSign;
    }

    // MODIFIED: Added DamageSourceType parameter
    private void DealDamage(Character target, Lane lane, CharacterStats attackerStats, float SkillTypeBonusModifer,
                            float DmgBonusModifier, Character.DamageSourceType sourceType, List<StatusScalingEntry> statusScalings = null)
    {
        if (!lane)
        {
            lane = owner.lane;
        }
        if (!target) return;

        // Determine the base damage based on the lane's main stat
        float baseDamage = Mathf.Max(0, attackerStats.getSkillpower(lane.mainStat));


        // Apply scaling based on status effect tiers and effect sign
        float scalingDamage = 0;
        if (statusScalings != null)
        {
            foreach (var scaling in statusScalings)
            {
                string statusName = scaling.statusName;
                float scalingFactor = scaling.scalingFactor;/// %bonus per tier
                StatusEffect.EffectSign effectSign = scaling.effectSign;


                int attackerTiers = 0;

                foreach (StatusEffect effect in attackerStats.Effects)
                {
                    if (effect.StatusName.ToUpper() == statusName.ToUpper())
                    {
                        attackerTiers += effect.GetTiers(effectSign);
                    }
                }

                // Add scaling damage for each relevant tier
                scalingDamage = (attackerTiers * scalingFactor) / 100f;
            }
        }

        // Combine base damage and scaling, then apply modifiers
        UnityEngine.Debug.LogError($"{Name} effect is calculating {baseDamage} * (1 + {scalingDamage} ) * ( {SkillTypeBonusModifer} + {DmgBonusModifier} -1) ");
        float calculatedDamage = Mathf.Max(((baseDamage * (1 + scalingDamage) * Power / 100) * (SkillTypeBonusModifer + DmgBonusModifier - 1)), baseDamage * 0.1f);

        UnityEngine.Debug.LogError($"{Name} effect is dealing {calculatedDamage} damage to {target.stats.CharacterName} from {sourceType} source.");
        target.TakeDamage(calculatedDamage, sourceType); // CORRECTED: Pass sourceType here
        owner.TriggerDealDamage(calculatedDamage, sourceType); // Pass the source type when triggering the event
    }


    private void HealTarget(Character target, CharacterStats stats, float Modifier)
    {
        if (!target) return;

        float baseHealing = Mathf.Max(0, stats.getSkillpower(owner.lane.mainStat) * Power / 100);

        float calculatedHeal = Mathf.Max(0, baseHealing) * Modifier;
        UnityEngine.Debug.LogError($"{Name} effect is healing {calculatedHeal} damage to {target.stats.CharacterName}");
        target.Heal(calculatedHeal);  // Heals based on the Power level
    }

    private void RecoverTarget(Character target, CharacterStats stats, float Modifier)
    {
        if (!target) return;
        float calculatedRecover = Mathf.Max(0, stats.getSkillpower(owner.lane.mainStat) * Power / 100) * Modifier;
        UnityEngine.Debug.LogError($"{Name} effect is recovering {calculatedRecover} stamina to {target.stats.CharacterName}");
        target.RecoverStamina(calculatedRecover);
    }

    private void DrainTarget(Character target, CharacterStats stats, float Modifier)
    {
        if (!target) return;
        float calculatedDrain = Mathf.Max(0, stats.getSkillpower(owner.lane.mainStat) * Power / 100) * Modifier;
        UnityEngine.Debug.LogError($"{Name} effect is draining {calculatedDrain} stamina to {target.stats.CharacterName}");
        target.TakeDrain(calculatedDrain);
    }

    private void ApplyStatusEffect(Character target, StatusEffect.EffectSign sign)
    {
        if (target == null)
        {
            Debug.LogError("Target is null in ApplyStatusEffect.");
            return;
        }

        if (StatusEffectToApply == null || StatusEffectToApply.Count <= 0)
        {
            Debug.LogError("StatusEffectToApply is null or empty in ApplyStatusEffect.");
            return;
        }

        foreach (StatModifier status in StatusEffectToApply)
        {
            if (status == null)
            {
                Debug.LogError("StatModifier is null in ApplyStatusEffect.");
                continue;
            }

            StatusEffect.Stack stack = new StatusEffect.Stack(status.TierCount, status.Duration, status.sign);
            StatusEffect statusReadyToApply = new StatusEffect(status, stack, target);

            target.ApplyStatus(statusReadyToApply);
        }
    }

    // NEW: Method to register the damage distribution event
    public void RegisterDamageDistributionEvent(Character characterOwner)
    {
        if (Type == EffectType.DistributeDmgDealt)
        {
            owner = characterOwner; // Ensure the owner is set for this effect instance
            // MODIFIED: Subscribe to the new event signature
            characterOwner.OnDealDamage += DistributeDamage;
            UnityEngine.Debug.Log($"{Name} effect registered to OnDealDamage event for {characterOwner.stats.CharacterName}.");
        }
    }

    // MODIFIED: Event handler for OnDealDamage event now accepts DamageSourceType
    private void DistributeDamage(float damageDealt, Character.DamageSourceType sourceType)
    {
        Debug.LogError($"[DistributeDamage] Called for {owner?.stats?.CharacterName ?? "null owner"} with {damageDealt} damage, sourceType: {sourceType}");

        if (owner == null)
        {
            Debug.LogError("[DistributeDamage] SkillEffect owner is null when attempting to distribute damage.");
            return;
        }

        // NEW: Check if the damage source matches the required source
        if (RequiredDamageSource != Character.DamageSourceType.None && RequiredDamageSource != sourceType)
        {
            Debug.LogWarning($"[DistributeDamage] DistributeDmgDealt effect ({Name}) skipped. Required source: {RequiredDamageSource}, Actual source: {sourceType}");
            return;
        }

        // Calculate the damage to distribute based on the Power (percentage)
        float distributedDamage = damageDealt * (Power / 100f);
        Debug.LogError($"[DistributeDamage] {owner.stats.CharacterName} is distributing {distributedDamage} damage ({Power}% of {damageDealt})");

        // Get targets using the existing GetTargets method, respecting TargetingType
        List<Character> targetsToDistribute = GetTargets(owner);

        if (targetsToDistribute == null)
        {
            Debug.LogWarning($"[DistributeDamage] targetsToDistribute is null for {Name}.");
            return;
        }
        if (targetsToDistribute.Count == 0)
        {
            Debug.LogWarning($"[DistributeDamage] No targets found to distribute {Name} damage to based on TargetingType: {TargetingType}.");
            return;
        }

        foreach (Character target in targetsToDistribute)
        {
            if (target != null)
            {
                Debug.LogError($"[DistributeDamage] Distributing {distributedDamage} damage to {target.stats.CharacterName} via {TargetingType}.");
                // Create a temporary SkillEffect to deal damage to each target
                // This reuses the existing DealDamage logic
                // Pass a specific 'Other' type for distributed damage, or the original sourceType if desired
                DealDamage(target, owner.lane, AttackerStats, 1.0f, 1.0f, Character.DamageSourceType.Other);
            }
            else
            {
                Debug.LogWarning($"[DistributeDamage] Target in targetsToDistribute is null.");
            }
        }
    }
}