using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static LanesManager;

public class AutoAttack : MonoBehaviour
{
    private CharacterStats stats;
    /// <summary>
    /// total time betweenattacks
    /// </summary>
    public float attackInterval;
    /// <summary>
    /// current cooldown timer
    /// </summary>
    public float attackCooldown;
    CharacterUIManager characterUIManager;

    // Reference to the Character component on this GameObject
    private Character characterOwner; // NEW: Added a field for the Character component

    void Start()
    {
        characterOwner = GetComponent<Character>(); // NEW: Get the Character component
        if (characterOwner == null)
        {
            Debug.LogError($"{name}: Character component not found!");
            enabled = false; // Disable this script if no Character component
            return;
        }

        stats = characterOwner.stats; // Use the characterOwner to get stats
        attackInterval = GetAttackInterval();
        attackCooldown = 0;

        if (stats == null)
        {
            Debug.LogError($"{name}: CharacterStats not initialized!");
        }

    }



    CharacterStats GetStats()
    {
        characterUIManager = gameObject.GetComponent<CharacterUIManager>();
        // Ensure characterOwner is not null before accessing its stats
        if (characterOwner == null)
        {
            characterOwner = gameObject.GetComponent<Character>();
            if (characterOwner == null) return null; // Still null, something is wrong
        }
        return stats = characterOwner.stats;

    }

    public void LateUpdate()
    {

        if (stats == null) stats = GetStats();  // Ensure stats are initialized
        if (stats == null) return;



        // Update interval if speed has changed
        float currentInterval = GetAttackInterval();
        if (Mathf.Abs(attackInterval - currentInterval) > Mathf.Epsilon)
        {
            attackInterval = currentInterval;
        }

        attackCooldown += Time.deltaTime;

        // Check if cooldown has reached the required interval
        if (attackCooldown >= attackInterval)
        {
            StartCoroutine(PerformAttack());
            attackCooldown = 0f;
        }
        if (characterUIManager != null)
        {
            characterUIManager.cooldownBar.value = attackCooldown;
        }
    }

    public float MinimumInterval = 0.0250f;
    public float GetAttackInterval()
    {
        if (stats.Speed <= Mathf.Epsilon) return float.MaxValue;

        float baseInterval = 2.35f;
        float scalingFactor = 375;

        float interval = Mathf.Max(MinimumInterval, baseInterval - (stats.Speed / scalingFactor));

        if (characterUIManager != null)
        {
            characterUIManager.cooldownBar.maxValue = interval;
        }
        return interval;
    }

    // Attack routine
    public IEnumerator PerformAttack(float mult = 1.0f)
    {
        float strikeModifier = stats.StrikeModifier;
        // Find a target to attack based on lane and affiliation
        Character target;
        if (stats.CharAffil == Character.Affiliation.Player)
        {

            target = characterOwner.FindTargetInOrder(Character.Affiliation.Enemy); // Use characterOwner

        }
        else
        {
            target = characterOwner.FindTargetInOrder(Character.Affiliation.Player); // Use characterOwner

        }

        if (target != null)
        {
            float dmg = (GetStats().getSkillpower(characterOwner.lane.mainStat) * mult) * strikeModifier;

            // NEW: Trigger the OnDealDamage event on the attacking character
            // This is crucial for the DistributeDmgDealt effect to work.
            characterOwner.TriggerDealDamage(dmg, Character.DamageSourceType.Strike); // Use characterOwner

            target.TakeDamage(dmg, Character.DamageSourceType.Strike); // This calls TakeDamage on the *target*

            Debug.LogWarning($"{this.stats.CharacterName} striked {target.stats.CharacterName} for {dmg} damage.");

            // Attack animation (moving character forward/backward)
            float waittime = attackInterval / 6.5f;
            transform.position = new Vector3(transform.position.x - GetStats().Forward(), transform.position.y, transform.position.z);

            yield return new WaitForSeconds(waittime);

            transform.position = new Vector3(transform.position.x + GetStats().Forward(), transform.position.y, transform.position.z);

            // Debug.LogError("Basic attack executed");
        }
        else
        {
            Debug.Log($"target exists: {target != null}");
            Debug.Log($"{stats.CharacterName} couldn't find a target to attack.");
        }

        yield return null;
    }
}