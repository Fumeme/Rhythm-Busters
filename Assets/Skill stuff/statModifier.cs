using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStatModifier", menuName = "Status Effects/Stat Modifier")]
[System.Serializable]

public class StatModifier : ScriptableObject
{
    /// <summary>
    ///     The icon for the status effect
    /// </summary>
    public Sprite icon;
    /// <summary>
    /// The name of the status effect
    /// </summary>
    public string StatusName = "Arson";
    [TextArea] public string Description = "also Arson";
    public StatusEffect.EffectSign sign = StatusEffect.EffectSign.Positive;
    public bool isOverTimeEffect = false; // if effect deals damage or heals over time
    /// <summary>
    /// The number of tiers for the effect
    /// </summary>
    public int TierCount = 3;
    /// <summary>
    /// The duration of the effect in Node ticks
    /// </summary>
    public int Duration = 15;
    [System.Serializable]
    public class StatEffect
    {
        /// <summary>
        /// The name of the stat to modify
        /// </summary>
        public string statName = "MaxHP";
        public StatusEffect.EffectSign sign = StatusEffect.EffectSign.Positive;
        /// <summary>
        /// The initial multiplier for the stat effect. 0.00 means no change
        /// </summary>
        [Range(0, 0.75f), Tooltip("0.00 is initial value")] public float InitialMultiplier = 0.00f;
        /// <summary>
        /// The base multiplier for the stat effect. 0.05 means 5% increase/decrease
        /// </summary>
        [Range(0, 0.5f), Tooltip("0.05 is initial value")] public float baseMultiplier = 0.05f;
        /// <summary>
        /// The increment for each tier. 0.01 means 1% increase/decrease
        /// </summary>
        [Range(0, 0.5f), Tooltip("0.01 is initial value")] public float tierBonusIncrement = 0.01f;

        public enum DotType
        {
            Damage, Heal, Drain, Recovery, None
        }
        public DotType dotType = DotType.None;
    }
    /// <summary>
    /// The list of stat effects to apply
    /// </summary>
    public List<StatEffect> statEffects = new();
}