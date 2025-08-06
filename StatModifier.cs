using UnityEngine;

[CreateAssetMenu(fileName = "NewStatModifier", menuName = "Status Effects/Stat Modifier")]
public class StatModifier : ScriptableObject
{
    [System.Serializable]
    public class StatEffect
    {
        public string statName;
        public float baseMultiplier = 0.05f;
        public float tierBonusIncrement = 0.01f;
    }

    public List<StatEffect> statEffects;
}
