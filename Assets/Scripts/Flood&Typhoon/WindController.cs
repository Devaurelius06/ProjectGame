using UnityEngine;
using System.Collections;

public class WindGustController : MonoBehaviour
{
    public GameObject windArea;                         // The area where the wind gust starts (box-shaped field)
    public ParticleSystem fogParticleSystem;            // Fog particle system

    public Vector2 gustIntervalRange = new Vector2(5f, 12f); // Delay between gusts
    public float gustDuration = 6f;                     // Duration of each gust
    public Vector3 gustDirection = new Vector3(-12f, -1f, 0f); // Strong slant leftward and slightly down

    private Bounds windBounds;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;

    void Start()
    {
        if (windArea == null || fogParticleSystem == null)
        {
            Debug.LogError("Missing reference to windArea or fogParticleSystem!");
            return;
        }

        windBounds = windArea.GetComponent<Renderer>().bounds;

        velocityModule = fogParticleSystem.velocityOverLifetime;
        velocityModule.enabled = true;

        ScheduleNextGust();
    }

    void ScheduleNextGust()
    {
        float delay = Random.Range(gustIntervalRange.x, gustIntervalRange.y);
        Invoke(nameof(TriggerWindGust), delay);
    }

    void TriggerWindGust()
    {
        Vector3 spawnPosition = new Vector3(
            windBounds.max.x,                              // Start from far right
            Random.Range(windBounds.min.y, windBounds.max.y),
            Random.Range(windBounds.min.z, windBounds.max.z)
        );

        fogParticleSystem.transform.position = spawnPosition;

        StartCoroutine(GustRoutine());
    }

    IEnumerator GustRoutine()
    {
        // Start particles with strong gust
        fogParticleSystem.Play();
        velocityModule.x = new ParticleSystem.MinMaxCurve(gustDirection.x);
        velocityModule.y = new ParticleSystem.MinMaxCurve(gustDirection.y);
        velocityModule.z = new ParticleSystem.MinMaxCurve(gustDirection.z);

        yield return new WaitForSeconds(gustDuration);

        // Stop particles and calm wind
        fogParticleSystem.Stop();
        velocityModule.x = new ParticleSystem.MinMaxCurve(0f);
        velocityModule.y = new ParticleSystem.MinMaxCurve(0f);
        velocityModule.z = new ParticleSystem.MinMaxCurve(0f);

        ScheduleNextGust();
    }
}
