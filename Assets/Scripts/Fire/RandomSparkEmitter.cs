using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSparkEmitter : MonoBehaviour
{
    public ParticleSystem sparkParticlesPrefab;
    public List<Rigidbody> lightObjects; // List of light objects
    public List<Rigidbody> heavyObjects; // List of heavy objects
    public float minDelay = 0.5f;
    public float maxDelay = 2f;
    [Range(0f, 1f)]
    public float randomIgnitionChance = 0.1f; // 30% chance to ignite instantly
    public float sparkCooldown = 2.5f; // Cooldown between sparks

    private FireIgnition fireIgnition;
    private bool canIgnite = true; // Flag to check if we can ignite a new object

    private void Start()
    {
        fireIgnition = FindObjectOfType<FireIgnition>();
        StartCoroutine(EmitSparksRandomly());
    }

    private IEnumerator EmitSparksRandomly()
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            // Check if we can ignite a new object
            if (!canIgnite)
                continue;

            // Combine light and heavy objects into one list to handle both
            List<Rigidbody> flammableObjects = new List<Rigidbody>(lightObjects);
            flammableObjects.AddRange(heavyObjects);

            if (flammableObjects.Count > 0)
            {
                Rigidbody randomObj = flammableObjects[Random.Range(0, flammableObjects.Count)];

                if (randomObj != null && sparkParticlesPrefab != null)
                {
                    Transform target = randomObj.transform;

                    // Skip if already burning
                    if (target.GetComponentInChildren<FireMarker>() != null) continue;

                    Vector3 spawnPos = GetTopCenterPosition(target);
                    ParticleSystem sparks = Instantiate(sparkParticlesPrefab, spawnPos, Quaternion.identity);
                    sparks.Play();
                    Destroy(sparks.gameObject, sparks.main.duration + sparks.main.startLifetime.constantMax);

                    if (fireIgnition != null)
                    {
                        // Randomly choose to force ignite or normally spark
                        if (Random.value < randomIgnitionChance)
                            fireIgnition.ForceIgnite(target);
                        else
                            fireIgnition.TrySpark(target);
                    }

                    // Set cooldown to prevent igniting another object immediately
                    canIgnite = false;
                    StartCoroutine(ResetIgniteCooldown());
                }
            }
        }
    }

    private Vector3 GetTopCenterPosition(Transform target)
    {
        Renderer rend = target.GetComponent<Renderer>();
        if (rend != null)
        {
            Vector3 center = rend.bounds.center;
            float topY = rend.bounds.max.y;
            return new Vector3(center.x, topY, center.z);
        }
        else
        {
            return target.position + new Vector3(0, 0.5f, 0); // fallback
        }
    }

    private IEnumerator ResetIgniteCooldown()
    {
        // Wait for the cooldown period before allowing a new ignition
        yield return new WaitForSeconds(sparkCooldown);
        canIgnite = true;
    }
}
