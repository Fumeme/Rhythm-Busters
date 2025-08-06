using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Skill System/Skill Conditions/Character Before Act")]
public class BeforeActCondition : SkillCondition
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
    public Targetting targetting = Targetting.Ally;

    private void OnBeforeAct(Character triggeringAlly)
    {
        this.TriggeringCharacter = triggeringAlly;  // Store the triggering ally
        conditionMet = true;

        // Immediately check the skill
        owner.Skills.ForEach(skill => {
            if (skill.Conditions.Contains(this))
                skill.CheckAndTriggerSkill();
        });

        // Reset after checking
        conditionMet = false;
        this.TriggeringCharacter = null;  // Clear reference
    }
    public override void Initialize(Character owner)
    {
        base.Initialize(owner);

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
                owner.BeforeAct += OnBeforeAct;
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

        // Subscribe to all allies' BeforeUlt events
        foreach (Character ally in team)
        {

                ally.BeforeAct += OnBeforeAct;
                Targets.Add(ally);
        }
    }
    private void FindAndSubscribeToEnemies()
    {
        // Get the owner's team
        List<Character> team = owner.stats.CharAffil == Character.Affiliation.Enemy
            ? LanesManager.Instance.EnemyCharacters
            : LanesManager.Instance.PlayerCharacters;

        // Subscribe to all allies' BeforeUlt events
        foreach (Character enemy in team)
        {
            if (enemy != owner) // Exclude self if needed
            {
                enemy.BeforeAct += OnBeforeAct;
                Targets.Add(enemy);
            }
        }
    }



    public override void Cleanup()
    {
        foreach (Character characters in Targets)
        {
            if (characters != null)
                characters.BeforeAct -= OnBeforeAct;
        }
        Targets.Clear();
    }

    protected override void SubscribeEvents()
    {
        throw new System.NotImplementedException();
    }

    protected override void UnsubscribeEvents()
    {
        throw new System.NotImplementedException();
    }
}