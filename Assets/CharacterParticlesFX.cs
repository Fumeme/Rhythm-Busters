using UnityEngine;

public class CharacterParticlesFX : MonoBehaviour
{
    public ParticleSystem SkillParticlesPrefab; // Reference to the particle system prefab
    public ParticleSystem NodeParticlesPrefab; // Reference to the particle system prefab
    public enum ParticleColor { Red, Blue, Green, Purple }
    [SerializeField] ParticleColor color = ParticleColor.Red;

    public void NodeFX( Vector2 pos)
    {
        if (NodeParticlesPrefab != null)
        {
            Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
            ParticleSystem newParticles = Instantiate(NodeParticlesPrefab, pos, rotation);

            // Play the instantiated particle system
            newParticles.Play();

            // Optionally destroy the particle system after a delay to prevent clutter
            Destroy(newParticles.gameObject, newParticles.main.duration + newParticles.main.startLifetime.constantMax);


        }
    }

    public void Emit( ParticleColor particleColor = ParticleColor.Red)
    {
        color = particleColor;
        if (SkillParticlesPrefab != null)
        {
            // Instantiate a new particle system
            Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
            ParticleSystem newParticles = Instantiate(SkillParticlesPrefab, transform.position, rotation);

            // Modify the color based on the enum
            var main = newParticles.main;
            switch (color)
            {
                case ParticleColor.Red:
                    main.startColor = Color.red;
                    break;
                case ParticleColor.Blue:
                    main.startColor = Color.blue;
                    break;
                case ParticleColor.Green:
                    main.startColor = Color.green;
                    break;
                case ParticleColor.Purple:
                    main.startColor = new Color(0.5f, 0f, 0.5f); // Custom purple color
                    break;
            }

            // Play the instantiated particle system
            newParticles.Play();

            // Optionally destroy the particle system after a delay to prevent clutter
            Destroy(newParticles.gameObject, newParticles.main.duration + newParticles.main.startLifetime.constantMax);
        }
        else
        {
            Debug.LogWarning("Particle prefab is not assigned!");
        }
    }
}
