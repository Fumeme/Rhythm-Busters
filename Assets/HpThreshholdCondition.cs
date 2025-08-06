using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Skill System/Skill Conditions/HP Threshhold")]
public class HPThreshholdCondition : SkillCondition
{
    private List<Character> Targets = new List<Character>();
    public Character TriggeringCharacter { get; private set; }

    public enum Targetting
    {
        self,
        Ally,
        Enemy,
        Both
    }
    public bool aboveThreshold = false; // Set to true if you want to check for above threshold
    public Targetting targetting = Targetting.Ally;
    public float hpThreshold = 0.3f; // Example threshold of 30%



    private void OnTakeDamage(float damage, Character.DamageSourceType sourceType)
    {
        // Check if the character's HP is above or below the threshold
        bool isAboveThreshold = (character.stats.currentHealth / character.stats.MaxHealth) >= hpThreshold;
        Debug.Log($"Current HP: {character.stats.currentHealth}, Max HP: {character.stats.MaxHealth}, Above Threshold: {isAboveThreshold}");
        if (isAboveThreshold == aboveThreshold)
        {
            Debug.Log($"Condition met for {character.name}: {isAboveThreshold}");
            conditionMet = true;
            // Immediately check the skill
            owner.Skills.ForEach(skill =>
            {
                if (skill.Conditions.Contains(this))
                    skill.CheckAndTriggerSkill();
            });
            // Reset after checking
            conditionMet = false;
        }
    }
    private void Onheal(float heal)
    {
        // Check if the character's HP is above or below the threshold
        bool isAboveThreshold = (character.stats.currentHealth / character.stats.MaxHealth) >= hpThreshold;
        Debug.Log($"Current HP: {character.stats.currentHealth}, Max HP: {character.stats.MaxHealth}");
        if (isAboveThreshold == aboveThreshold)
        {
            Debug.Log($"Condition met for {character.name}: {isAboveThreshold}");
            conditionMet = true;
            // Immediately check the skill
            owner.Skills.ForEach(skill =>
            {
                if (skill.Conditions.Contains(this))
                    skill.CheckAndTriggerSkill();
            });
            // Reset after checking
            conditionMet = false;
        }
    }

    public override void Initialize(Character owner)
    {
        base.Initialize(owner);

        Debug.Log($"Initializing HPThreshholdCondition for {owner.stats.CharacterName}");

        switch (targetting)
        {
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
            case Targetting.self:
            default:
                owner.OnTakeDamage += OnTakeDamage;
                owner.OnTakeHeal += Onheal;
                Targets.Add(owner);
                break;
        }


    }

    private void FindAndSubscribeToAllies()
    {
        // Get the owner's team
        List<Character> team = owner.stats.CharAffil == Character.Affiliation.Player
            ? LanesManager.Instance.PlayerCharacters
            : LanesManager.Instance.EnemyCharacters;

        foreach (Character ally in team)
        {


            ally.OnTakeDamage += OnTakeDamage;
            ally.OnTakeHeal += Onheal;
            Targets.Add(ally);
        }
    }
    private void FindAndSubscribeToEnemies()
    {
        // Get the owner's team
        List<Character> team = owner.stats.CharAffil == Character.Affiliation.Enemy
            ? LanesManager.Instance.EnemyCharacters
            : LanesManager.Instance.PlayerCharacters;

        foreach (Character enemy in team)
        {

            enemy.OnTakeDamage += OnTakeDamage;
            enemy.OnTakeHeal += Onheal;
            Targets.Add(enemy);

        }
    }



    public override void Cleanup()
    {
        Debug.Log($"Cleaning up HPThreshholdCondition for {owner.stats.CharacterName}");
        foreach (Character character in Targets)
        {
            if (character != null)
            {
                character.OnTakeDamage -= OnTakeDamage;
                character.OnTakeHeal -= Onheal;
                Debug.Log($"Unsubscribed from OnTakeDamage and OnTakeHeal for {character.stats.CharacterName}");
            }
        }
        Targets.Clear();
    }

    protected override void SubscribeEvents()
    {
      //  throw new System.NotImplementedException();
    }

    protected override void UnsubscribeEvents()
    {
       // throw new System.NotImplementedException();
    }
}