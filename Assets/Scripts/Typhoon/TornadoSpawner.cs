using UnityEngine;

public class GroundTornadoSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject groundSpawnerBox;       // Box that defines the area
    public GameObject tornadoPrefab;

    [Header("FX Particles")]
    public GameObject groundDisturbanceFX;
    public GameObject vortexFormingFX;
    public GameObject touchdownFX;

    [Header("Realistic Timing (in seconds)")]
    public float groundDisturbanceDelay = 3f;
    public float vortexFormDelay = 5f;
    public float touchdownDelay = 7f;

    [Header("Tornado Movement")]
    public float movementSpeed = 2f;

    private GameObject spawnedTornado;
    private bool hasSpawned = false;
    private Vector3 moveTarget;

    void Start()
    {
        if (!hasSpawned)
        {
            StartCoroutine(SpawnTornadoSequence());
            hasSpawned = true;
        }
    }

    System.Collections.IEnumerator SpawnTornadoSequence()
    {
        Vector3 spawnPosition = GetRandomGroundPosition();

        // 🌱 Ground disturbance
        Debug.Log("🌱 Ground disturbance...");
        if (groundDisturbanceFX != null)
        {
            GameObject fx1 = Instantiate(groundDisturbanceFX, spawnPosition, Quaternion.identity);
            Destroy(fx1, groundDisturbanceDelay);
        }
        yield return new WaitForSeconds(groundDisturbanceDelay);

        // 🌪️ Vortex forming
        Debug.Log("🌪️ Vortex forming...");
        if (vortexFormingFX != null)
        {
            GameObject fx2 = Instantiate(vortexFormingFX, spawnPosition, Quaternion.identity);
            Destroy(fx2, vortexFormDelay);
        }
        yield return new WaitForSeconds(vortexFormDelay);

        // 🌀 Touchdown
        Debug.Log("🌀 Tornado touchdown...");
        if (touchdownFX != null)
        {
            GameObject fx3 = Instantiate(touchdownFX, spawnPosition, Quaternion.identity);
            Destroy(fx3, 2f);
        }
        yield return new WaitForSeconds(touchdownDelay);

        // 🌪️ Spawn tornado
        spawnedTornado = Instantiate(tornadoPrefab, spawnPosition, Quaternion.Euler(180, 0, 0));
        moveTarget = GetRandomGroundPosition();
        Debug.Log("✅ Tornado spawned on top of platform and moving!");
    }

    void Update()
    {
        if (spawnedTornado != null)
        {
            spawnedTornado.transform.position = Vector3.MoveTowards(
                spawnedTornado.transform.position,
                moveTarget,
                movementSpeed * Time.deltaTime
            );

            if (Vector3.Distance(spawnedTornado.transform.position, moveTarget) < 0.5f)
            {
                moveTarget = GetRandomGroundPosition();
            }
        }
    }

    Vector3 GetRandomGroundPosition()
    {
        Bounds bounds = groundSpawnerBox.GetComponent<Renderer>().bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        float y = bounds.max.y; // top of the platform
        return new Vector3(x, y, z);
    }
}
