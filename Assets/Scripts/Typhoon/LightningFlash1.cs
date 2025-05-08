using UnityEngine;
using System.Collections;  // <-- Make sure this is included

public class LightningFlashLooper : MonoBehaviour
{
    public Light lightningLight;               // Light source for the lightning flash
    public GameObject cloudBoxObject;          // The 3D object representing cloud area
    public GameObject lightningStrikePrefab;   // Lightning prefab
    public float flashIntensity = 4f;          // Intensity of the lightning flash
    public float flashDuration = 0.1f;         // Duration of the flash
    public float minStrikeDelay = 5f;          // Min delay between lightning strikes
    public float maxStrikeDelay = 15f;         // Max delay between lightning strikes

    private float defaultIntensity;
    private Bounds cloudBounds;

    void Start()
    {
        if (lightningLight == null || cloudBoxObject == null || lightningStrikePrefab == null)
        {
            Debug.LogError("Missing references in the inspector!");
            return;
        }

        defaultIntensity = lightningLight.intensity;
        cloudBounds = cloudBoxObject.GetComponent<Renderer>().bounds;
        ScheduleNextLightningStrike();
    }

    void ScheduleNextLightningStrike()
    {
        float delay = Random.Range(minStrikeDelay, maxStrikeDelay);
        Invoke(nameof(TriggerLightning), delay);
    }

    void TriggerLightning()
    {
        // Random position inside the cloud bounds (x, z)
        float x = Random.Range(cloudBounds.min.x, cloudBounds.max.x);
        float z = Random.Range(cloudBounds.min.z, cloudBounds.max.z);
        float y = cloudBounds.max.y + 10f;  // Start position above the cloud

        Vector3 strikePos = new Vector3(x, y, z);

        // Trigger lightning flash and instantiate lightning
        StartCoroutine(FlashLightning(strikePos));

        // Spawn the lightning at the calculated position
        Instantiate(lightningStrikePrefab, strikePos, Quaternion.identity);

        // Schedule the next lightning strike
        ScheduleNextLightningStrike();
    }

    // Fix: IEnumerator method properly defined
    IEnumerator FlashLightning(Vector3 strikePos)
    {
        // Simulate the lightning flash
        lightningLight.intensity = flashIntensity;
        lightningLight.color = Color.white;  // White light for realistic flash
        lightningLight.transform.position = strikePos;  // Position the light near the lightning strike

        yield return new WaitForSeconds(flashDuration);

        // Reset light intensity and color after the flash
        lightningLight.intensity = defaultIntensity;
        lightningLight.color = Color.gray; // You can use a dim color to simulate the aftermath of a strike
    }
}
