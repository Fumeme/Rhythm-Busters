using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill Definition")]
[Serializable]
public class SkillDefinition : ScriptableObject
{


    public string SkillName = "slap";
    [TextArea] public string description = "slapper";
    [Min(0)] public int cooldown = 15, warmup = 0, cost = 300;
    public Skill.SkillType skillType; // Enum to define Act, Talent or Ult
    public SkillCondition Conditions; // Visible in Inspector


    public List<SkillEffectDefinition> effects = new List<SkillEffectDefinition>();
    public Sprite SkillIcon;

    public SkillDefinition GetDefinition()
    {
        return this;
    }
}