using UnityEngine;

public class WindGust : MonoBehaviour
{
    [Header("Wind Gust Particle FX")]
    public ParticleSystem windGustFXPrefab;

    [Header("Spawn Box")]
    public GameObject windSpawnBox;

    [Header("Weather Settings")]
    public bool startWeatherOnPlay = false;

    void Start()
    {
        if (startWeatherOnPlay)
        {
            TriggerWeather();
        }
    }

    // Call this method to spawn a gust immediately
    public void TriggerWeather()
    {
        if (windGustFXPrefab != null && windSpawnBox != null)
        {
            Vector3 spawnPos = GetRandomPositionInBox(windSpawnBox);
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // Spawn and play the gust
            ParticleSystem gust = Instantiate(windGustFXPrefab, spawnPos, randomRotation);
            gust.Play();

            Debug.Log("🌬️ Wind gust spawned at: " + spawnPos);
        }
    }

    Vector3 GetRandomPositionInBox(GameObject box)
    {
        Bounds bounds = box.GetComponent<Renderer>().bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = bounds.max.y; // top of box
        float z = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(x, y, z);
    }
}
