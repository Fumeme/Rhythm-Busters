using System;
using System.Collections.Generic;
using UnityEngine;


public abstract class SkillCondition : ScriptableObject
{
    [SerializeField] protected string description;
    protected Character character;

    public string Description => description;

    protected bool conditionMet;
    protected Character owner;

    public bool IsConditionMet() => conditionMet;

    public float TriggeringValue { get; protected set; } = 0f;

    // Initialize and subscribe to relevant events
    public virtual void Initialize(Character owner)
    {
        TriggeringValue = 0f;
        character = owner;
        conditionMet = false;
        SubscribeEvents();
    }

    // Cleanup when skill is destroyed/reset
    public virtual void Cleanup()
    {
        TriggeringValue = 0f;
        UnsubscribeEvents();
    }

    protected abstract void SubscribeEvents();
    protected abstract void UnsubscribeEvents();

    private List<Character> allies = new List<Character>();

    private void OnAllyBeforeUlt(Character character)
    {
        // Skill activates when ANY ally uses their Ult
        conditionMet = true;

        // Immediately check if the skill can trigger
        
        owner.Skills.ForEach(skill => {
            if (skill.Conditions.Contains(this))
                skill.CheckAndTriggerSkill();
        });

        // Reset after checking
        conditionMet = false;
    }

    // Call this method when new allies join
    public void AddAlly(Character newAlly)
    {
        if (!allies.Contains(newAlly))
        {
            newAlly.BeforeUlt += OnAllyBeforeUlt;
            allies.Add(newAlly);
        }
    }

    // Call this method when allies leave
    public void RemoveAlly(Character oldAlly)
    {
        if (allies.Contains(oldAlly))
        {
            oldAlly.BeforeUlt -= OnAllyBeforeUlt;
            allies.Remove(oldAlly);
        }
    }
}
