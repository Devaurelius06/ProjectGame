using UnityEngine;
using System.Collections;

public class RainControl : MonoBehaviour
{
    public ParticleSystem softRainParticleSystem;
    public ParticleSystem heavyRainParticleSystem;
    public float buildUpDuration = 10f;          // Time it takes to fully intensify

    public float softMaxRate = 40f;
    public float heavyMaxRate = 70f;

    private ParticleSystem.EmissionModule softEmission;
    private ParticleSystem.EmissionModule heavyEmission;

    void Start()
    {
        if (softRainParticleSystem == null || heavyRainParticleSystem == null)
        {
            Debug.LogError("Assign both rain particle systems in the Inspector.");
            return;
        }

        softEmission = softRainParticleSystem.emission;
        heavyEmission = heavyRainParticleSystem.emission;

        softRainParticleSystem.Play();
        heavyRainParticleSystem.Play();

        // Start with only soft rain active
        softEmission.rateOverTime = 5f;
        heavyEmission.rateOverTime = 0f;

        StartCoroutine(GraduallyIntensifyRain());
    }

    IEnumerator GraduallyIntensifyRain()
    {
        float elapsed = 0f;

        while (elapsed < buildUpDuration)
        {
            float t = elapsed / buildUpDuration;

            // Increase both rains over time
            softEmission.rateOverTime = Mathf.Lerp(5f, softMaxRate, t);
            heavyEmission.rateOverTime = Mathf.Lerp(0f, heavyMaxRate, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Final intensity
        softEmission.rateOverTime = softMaxRate;
        heavyEmission.rateOverTime = heavyMaxRate;
    }
}
