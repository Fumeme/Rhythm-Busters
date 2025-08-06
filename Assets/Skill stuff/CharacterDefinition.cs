using System;
using System.Collections.Generic;
using UnityEngine;
using static CharacterStats;
using static Character;

[CreateAssetMenu(fileName = "New Character", menuName = "Character Template")]
[Serializable]
public class CharacterDefinition : ScriptableObject
{
    public String CharacterName = "Name";
    public ClassType Class = ClassType.Attacker;

    [Header("Base Stat Ratings (100-2000 range)")]
    [Min(100)] public int HealthRating = 100;
    [Min(100)] public int StaminaRating = 100;
    [Min(0)] public int DefenseRating = 100;
    [Min(25)] public int SpeedRating = 25;
    [Min(50)] public int MightRating = 750;
    [Min(50)] public int ArcaneRating = 750;
    [Min(50)] public int Critical = 250;
    [Min(50)] public int AccuracyRating = 750;

    [TextArea] public String Description = "Description";
    public Affiliation Affiliation = Character.Affiliation.Player;
    [Header("Skills")]
    public List<SkillDefinition> SkillDefinitions = new List<SkillDefinition>();
    [Header("Visuals")]
    public Sprite CharacterSprite;
    public Sprite WeaponSprite;

}