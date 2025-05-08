using UnityEngine;

public class LightningSpawn : MonoBehaviour
{
    public GameObject cloudBoxObject;         // Your 3D cloud box object (assign in Inspector)
    public GameObject lightningPrefab;        // Lightning effect prefab
    public float minStrikeDelay = 4f;
    public float maxStrikeDelay = 12f;
    public float initialDelay = 5f;
    public int maxBoltsPerStrike = 3;

    private Bounds cloudBounds;
    private bool isReady = false;

    void Start()
    {
        if (cloudBoxObject == null || lightningPrefab == null)
        {
            Debug.LogWarning("Missing cloud box or lightning prefab.");
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
        Invoke(nameof(SpawnLightning), delay);
    }

    void SpawnLightning()
    {
        if (!isReady) return;

        cloudBounds = cloudBoxObject.GetComponent<Renderer>().bounds;

        int bolts = Random.Range(1, maxBoltsPerStrike + 1);
        for (int i = 0; i < bolts; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(cloudBounds.min.x, cloudBounds.max.x),
                Random.Range(cloudBounds.min.y, cloudBounds.max.y),
                Random.Range(cloudBounds.min.z, cloudBounds.max.z)
            );

            GameObject bolt = Instantiate(lightningPrefab, randomPos, Random.rotation);
            float intensity = Random.Range(0.8f, 1.3f);
            bolt.transform.localScale *= intensity;

            Destroy(bolt, 2f);
        }

        ScheduleNextStrike();
    }
}
