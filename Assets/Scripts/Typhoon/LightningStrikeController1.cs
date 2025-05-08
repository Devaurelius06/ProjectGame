using UnityEngine;
using System.Collections;  // <-- Add this line to enable IEnumerator and coroutines

public class LightningtrikeSpawn : MonoBehaviour
{
    public GameObject cloudBoxObject;           // 3D object representing cloud area
    public GameObject lightningStrikePrefab;    // Lightning effect prefab
    public float minStrikeDelay = 4f;
    public float maxStrikeDelay = 12f;
    public float initialDelay = 5f;
    public float lightningHeightOffset = 10f;   // Height from the cloud to start the lightning
    public GameObject groundObject;             // The "Ground" 3D object for raycast detection

    private Bounds cloudBounds;
    private bool isReady = false;

    void Start()
    {
        if (cloudBoxObject == null || lightningStrikePrefab == null || groundObject == null)
        {
            Debug.LogWarning("Missing cloud, lightning prefab, or ground object.");
            return;
        }

        cloudBounds = cloudBoxObject.GetComponent<Renderer>().bounds;
        Invoke(nameof(StartStorm), initialDelay);
    }

    void StartStorm()
    {
        isReady = true;
        ScheduleNextStrike();
    }

    void ScheduleNextStrike()
    {
        float delay = Random.Range(minStrikeDelay, maxStrikeDelay);
        Invoke(nameof(SpawnLightningStrike), delay);
    }

    void SpawnLightningStrike()
    {
        if (!isReady) return;

        cloudBounds = cloudBoxObject.GetComponent<Renderer>().bounds;

        // Random position inside the cloud bounds (x, z)
        float x = Random.Range(cloudBounds.min.x, cloudBounds.max.x);
        float z = Random.Range(cloudBounds.min.z, cloudBounds.max.z);

        // Start lightning from a point above the cloud
        float startY = cloudBounds.max.y + lightningHeightOffset; // Above the cloud
        Vector3 startPos = new Vector3(x, startY, z);

        // Raycast downwards to detect the "Ground" object
        RaycastHit hit;
        Vector3 direction = Vector3.down; // Cast downwards from the lightning

        // Cast ray towards the "Ground" object
        if (Physics.Raycast(startPos, direction, out hit))
        {
            // If ray hits the ground, use the hit point as the end position for the lightning
            Vector3 endPos = hit.point;

            // Instantiate lightning bolt at start position
            GameObject bolt = Instantiate(lightningStrikePrefab, startPos, Quaternion.identity);

            // Set up LineRenderer to connect the start and end positions (cloud to ground)
            LineRenderer lineRenderer = bolt.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, startPos);   // Start point (cloud)
                lineRenderer.SetPosition(1, endPos);     // End point (ground hit)
            }

            // Optionally adjust the scale/intensity of the lightning
            float scale = Random.Range(0.8f, 1.4f);
            bolt.transform.localScale *= scale;

            // Check if there's a ParticleSystem on the bolt
            ParticleSystem ps = bolt.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Destroy the bolt after the particle system finishes
                StartCoroutine(DestroyAfterParticleSystem(bolt, ps));
            }
            else
            {
                // Destroy the bolt after a fixed time (e.g., 2 seconds) if no ParticleSystem
                Destroy(bolt, 2f);
            }
        }
        else
        {
            // If no ground is detected, log a warning
            Debug.LogWarning("No ground detected for lightning strike.");
        }

        // Schedule next strike after a random delay
        ScheduleNextStrike();
    }

    // Coroutine to wait for the particle system to finish before destroying the object
    private IEnumerator DestroyAfterParticleSystem(GameObject bolt, ParticleSystem ps)
    {
        // Wait for the particle system's lifetime to finish
        yield return new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);

        // Destroy the lightning bolt object after the particles have finished
        Destroy(bolt);
    }
}
