using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Added for .Where(a => a != null)

[CreateAssetMenu(menuName = "Skill System/Skill Conditions/On Strike (Dealt Damage)")]
public class OnStrikeCondition : SkillCondition
{
    public enum Targetting
    {
        Self,
        Ally,
        Enemy,
        Both
    }

    [Tooltip("The number of damage instances (strikes dealt) required to trigger the condition.")]
    public int strikeCountThreshold = 1;

    [Tooltip("Which characters to monitor for dealing damage (performing strikes).")]
    public Targetting targetting = Targetting.Self;

    [Tooltip("Filter by specific type of damage-dealing action. 'Any' means all.")]
    public Character.DamageSourceType requiredDamageSourceCategory = Character.DamageSourceType.Strike;

    private int currentStrikeCount;
    private List<Character> monitoredCharacters = new List<Character>();

    public Character.DamageSourceType DamageSourceType = Character.DamageSourceType.Strike;

    public override void Initialize(Character owner)
    {
        base.Initialize(owner);
        currentStrikeCount = 0;
        // The base Initialize sets TriggeringAmount to 0f
        Debug.Log($"Initializing OnActionDealDamageCondition for {owner.stats.CharacterName}. Monitoring {targetting} for {requiredDamageSourceCategory} actions.");
        SubscribeEvents();
    }

    protected override void SubscribeEvents()
    {
        UnsubscribeEvents(); // Clear previous subscriptions
        monitoredCharacters.Clear();

        switch (targetting)
        {
            case Targetting.Self:
                if (owner != null)
                {
                    owner.OnDealDamage += OnCharacterDealDamageWithSource;
                    monitoredCharacters.Add(owner);
                    Debug.Log($"OnActionDealDamageCondition: Subscribing to self ({owner.stats.CharacterName}) OnDealDamageWithSource.");
                }
                break;
            case Targetting.Ally:
                FindAndSubscribeToAllies();
                break;
            case Targetting.Enemy:
                FindAndSubscribeToEnemies();
                break;
            case Targetting.Both:
                FindAndSubscribeToAllies();
                FindAndSubscribeToEnemies();
                break;
        }
    }

    private void OnCharacterDealDamageWithSource(float damage, Character.DamageSourceType category)
    {
        // Filter by the required damage source category
        if (requiredDamageSourceCategory != Character.DamageSourceType.Other && requiredDamageSourceCategory != category)
        {
            // Debug.Log($"OnActionDealDamageCondition: Ignoring damage from {category}. Expected {requiredDamageSourceCategory}.");
            return;
        }

        currentStrikeCount++;
        Debug.Log($"OnActionDealDamageCondition: A monitored character dealt damage ({category}). Current strikes: {currentStrikeCount}/{strikeCountThreshold}");

        if (currentStrikeCount >= strikeCountThreshold)
        {
            conditionMet = true;
            TriggeringValue = damage; // Store the triggering damage amount!
            Debug.Log($"OnActionDealDamageCondition: Condition met! Current strikes ({currentStrikeCount}) >= Threshold ({strikeCountThreshold}). Triggering amount: {TriggeringValue}.");

            // Immediately check the skill, passing THIS condition instance
            owner.Skills.ForEach(skill =>
            {
                if (skill.Conditions.Contains(this))
                {
                    // Fix for CS1503: Argument 1: cannot convert from 'OnStrikeCondition' to 'Character'
                    // The method `CheckAndTriggerSkill` expects a `Character` as its argument, but the code is passing `this` (an instance of `OnStrikeCondition`).
                    // To fix this, we need to pass the correct `Character` instance, which is likely the `owner` of the condition.

                    owner.Skills.ForEach(skill =>
                    {
                        if (skill.Conditions.Contains(this))
                        {
                            skill.CheckAndTriggerSkill(owner); // Pass the owner (Character) instead of `this`
                        }
                    });
                   // skill.CheckAndTriggerSkill(this); // Pass the condition itself
                }
            });

            // Reset the strike count and triggering amount after checking/triggering
            currentStrikeCount = 0;
            TriggeringValue = 0f; // Reset for next trigger
            conditionMet = false; // Reset conditionMet after attempting to trigger skill
        }
    }
    protected override void UnsubscribeEvents()
    {
        foreach (Character charToUnsubscribe in monitoredCharacters)
        {
            if (charToUnsubscribe != null)
            {
                charToUnsubscribe.OnDealDamage -= OnCharacterDealDamageWithSource;
                Debug.Log($"OnActionDealDamageCondition: Unsubscribed from {charToUnsubscribe.stats.CharacterName} OnDealDamageWithSource.");
            }
        }
        monitoredCharacters.Clear();
    }

    private void FindAndSubscribeToAllies()
    {
        // Get the owner's team
        List<Character> team = owner.stats.CharAffil == Character.Affiliation.Player
            ? LanesManager.Instance.PlayerCharacters
            : LanesManager.Instance.EnemyCharacters;

        foreach (Character ally in team.Where(a => a != null))
        {
            if (!monitoredCharacters.Contains(ally))
            {
                ally.AfterStrike += OnCharacterDealDamage;
                monitoredCharacters.Add(ally);
                Debug.Log($"OnStrikeCondition: Subscribing to ally ({ally.stats.CharacterName}) OnDealDamage.");
            }
        }
    }

    private void FindAndSubscribeToEnemies()
    {
        // Get the opposing team
        List<Character> team = owner.stats.CharAffil == Character.Affiliation.Enemy
            ? LanesManager.Instance.PlayerCharacters // If owner is enemy, players are enemies
            : LanesManager.Instance.EnemyCharacters; // If owner is player, enemies are enemies

        foreach (Character enemy in team.Where(e => e != null))
        {
            if (!monitoredCharacters.Contains(enemy))
            {
                enemy.AfterStrike += OnCharacterDealDamage;
                monitoredCharacters.Add(enemy);
                Debug.Log($"OnStrikeCondition: Subscribing to enemy ({enemy.stats.CharacterName}) OnDealDamage.");
            }
        }
    }

    private void OnCharacterDealDamage(Character character)
    {
        currentStrikeCount++;
        Debug.Log($"OnStrikeCondition: A monitored character dealt damage. Current strikes: {currentStrikeCount}/{strikeCountThreshold}");

        if (currentStrikeCount >= strikeCountThreshold)
        {
            conditionMet = true;
            Debug.Log($"OnStrikeCondition: Condition met! Current strikes ({currentStrikeCount}) >= Threshold ({strikeCountThreshold}).");

            // Immediately check the skill
            owner.Skills.ForEach(skill =>
            {
                if (skill.Conditions.Contains(this))
                {
                    skill.CheckAndTriggerSkill();
                }
            });

            // Reset the strike count after checking/triggering
            currentStrikeCount = 0;
            conditionMet = false; // Reset conditionMet after attempting to trigger skill
        }
    }

    public override void Cleanup()
    {
        Debug.Log($"Cleaning up OnStrikeCondition for {owner.stats.CharacterName}.");
        UnsubscribeEvents();
        currentStrikeCount = 0;
        base.Cleanup();
    }
}