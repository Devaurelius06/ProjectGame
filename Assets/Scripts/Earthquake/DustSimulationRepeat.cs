using UnityEngine;
using System.Collections.Generic;

public class DustSimulationRepeating : MonoBehaviour
{
    public ParticleSystem dustParticlePrefab;
    public EarthquakeManager earthquakeManager;
    public float dustLifeTime = 2f;
    public float emissionInterval = 0.5f;
    public float dustEmissionMultiplier = 1f;
    public int maxDustPoolSize = 20;

    private List<Rigidbody> ceilingObjects = new List<Rigidbody>();
    private List<ParticleSystem> dustPool = new List<ParticleSystem>();
    private float emissionTimer;
    private bool isShaking = false;
    private float shakeStartTime = 0f;

    private void Start()
    {
        if (earthquakeManager == null)
            earthquakeManager = GetComponent<EarthquakeManager>();

        if (earthquakeManager != null)
            ceilingObjects.AddRange(earthquakeManager.ceilingObjects);

        InitializeDustPool();
    }

    private void Update()
    {
        if (earthquakeManager == null || earthquakeManager.cameraShake == null) return;

        float shakeIntensity = earthquakeManager.cameraShake.CurrentShakeIntensity;

        if (shakeIntensity > 0f)
        {
            if (!isShaking)
            {
                isShaking = true;
                shakeStartTime = Time.time; // Store the time when shaking started
            }

            emissionTimer -= Time.deltaTime;

            if (emissionTimer <= 0f)
            {
                EmitDustRepeatedly(shakeIntensity);
                emissionTimer = Mathf.Clamp(emissionInterval / (shakeIntensity + 0.1f), 0.1f, emissionInterval);
            }
        }
        else
        {
            isShaking = false;
            emissionTimer = 0f;
        }
    }

    private void EmitDustRepeatedly(float shakeIntensity)
    {
        foreach (var obj in ceilingObjects)
        {
            if (obj != null && obj.CompareTag("Ceiling"))
            {
                EmitDust(obj.transform.position, shakeIntensity);
            }
        }
    }

    private void EmitDust(Vector3 position, float shakeIntensity)
    {
        ParticleSystem dust = GetDustFromPool();
        if (dust == null) return;

        dust.transform.position = position;

        // Adjust particle properties based on shake intensity
        var main = dust.main;
        main.startLifetime = Mathf.Lerp(0.5f, dustLifeTime, shakeIntensity);
        main.startSize = Mathf.Lerp(0.2f, 1f, shakeIntensity);
        main.startSpeed = Mathf.Lerp(0.1f, 2f, shakeIntensity);
        main.startColor = Color.Lerp(new Color(1f, 1f, 1f, 0.3f), new Color(0.5f, 0.5f, 0.5f, 0.8f), shakeIntensity);

        var emission = dust.emission;
        emission.rateOverTime = Mathf.Lerp(10f, 50f, shakeIntensity); // Adjusted for more variation
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, (short)Mathf.Lerp(5, 30, shakeIntensity))
        });

        // Adding delayed velocity and smoothness to prevent instant detachment
        var velocity = dust.velocityOverLifetime;
        velocity.enabled = true;

        // Smooth out the fall by making the y velocity gradually increase over time (so it doesn't fall too fast)
        float fallSpeed = Mathf.Lerp(0.1f, 0.5f, shakeIntensity);  // Less fall speed when intensity is low
        float randomness = Mathf.Lerp(0.1f, 1.5f, shakeIntensity);

        velocity.x = new ParticleSystem.MinMaxCurve(-randomness, randomness);
        velocity.y = new ParticleSystem.MinMaxCurve(0.0f, fallSpeed); // Ensure the dust doesn't immediately fall
        velocity.z = new ParticleSystem.MinMaxCurve(-randomness, randomness);

        var shape = dust.shape;
        shape.enabled = true;
        shape.angle = Mathf.Lerp(10f, 25f, shakeIntensity);

        dust.gameObject.SetActive(true);
        dust.Play();

        StartCoroutine(DeactivateAfter(dust, dustLifeTime + 1f));
    }

    private void InitializeDustPool()
    {
        for (int i = 0; i < maxDustPoolSize; i++)
        {
            ParticleSystem dust = Instantiate(dustParticlePrefab);
            dust.gameObject.SetActive(false);
            dustPool.Add(dust);
        }
    }

    private ParticleSystem GetDustFromPool()
    {
        foreach (var dust in dustPool)
        {
            if (!dust.gameObject.activeInHierarchy)
                return dust;
        }
        return null;
    }

    private System.Collections.IEnumerator DeactivateAfter(ParticleSystem dust, float delay)
    {
        yield return new WaitForSeconds(delay);
        dust.Stop();
        dust.gameObject.SetActive(false);
    }
}
