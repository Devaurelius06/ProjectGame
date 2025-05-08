using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;

[System.Serializable]
public class FlammableTarget
{
    public Transform target;
    public float heatThreshold = 100f;
}


public class FireIgnition : MonoBehaviour
{ 
    [Header("Player and Blur Distance")]
    public Transform player; // NEW: Player reference
    public Transform blurCenterTarget; // NEW: Center of area for blur logic
    public float minFocusDistance = 1f; // Near = blur
    public float maxFocusDistance = 20f; // Far = clear
    public float blurRange = 50f; // max effective range for blur calculation

    [Header("Prefabs")]
    public GameObject sparkEffectPrefab;
    public GameObject fireEffectPrefab;

    [Header("Settings")]
    public string flammableTag = "Flammable";
    public int sparksBeforeIgnition = 5;
    public float ignitionDelay = 1f;
    public float fireDuration = 20f;
    public float smokeIntensityIncrease = 0.1f;
    public float maxSmokeLevel = 1f;
    private int fireCount = 0;
    public int smokeActivationFireThreshold = 5;


    [Header("Flammable Objects")]
    public List<FlammableTarget> flammableObjects = new List<FlammableTarget>();

    [Header("Heavy Objects (not destroyed by fire)")]
    public List<Transform> heavyObjects = new List<Transform>();

    private Dictionary<Transform, int> sparkCounters = new Dictionary<Transform, int>();
    private Dictionary<Transform, float> heatLevels = new Dictionary<Transform, float>();

    private float currentSmokeLevel = 0.0f;

    [Header("Post Processing")]
    public PostProcessVolume postProcessVolume;
    private DepthOfField depthOfField;

    private void Start()
    {
        RenderSettings.fog = false; // Fog disabled initially
        RenderSettings.fogColor = Color.gray;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 0f;
        RenderSettings.fogEndDistance = 60f;

        postProcessVolume.profile.TryGetSettings(out depthOfField);
    }

    public void TrySpark(Transform target)
    {
        if (target == null || !target.CompareTag(flammableTag)) return;
        if (target.GetComponentInChildren<FireMarker>() != null) return;

        if (!sparkCounters.ContainsKey(target))
            sparkCounters[target] = 0;

        sparkCounters[target]++;

        Vector3 topPosition = GetTopPosition(target);
        GameObject sparks = Instantiate(sparkEffectPrefab, topPosition, Quaternion.identity);
        sparks.transform.SetParent(target);
        Destroy(sparks, 2.5f);

        if (sparkCounters[target] >= sparksBeforeIgnition)
        {
            StartCoroutine(IgniteSequence(target));
            sparkCounters.Remove(target);
        }
    }

    public void ForceIgnite(Transform target)
    {
        if (target == null || !target.CompareTag(flammableTag)) return;
        if (target.GetComponentInChildren<FireMarker>() != null) return;

        StartCoroutine(IgniteSequence(target));
    }

    public void AddHeat(Transform target, float amount)
    {
        if (target == null || !target.CompareTag(flammableTag)) return;
        if (target.GetComponentInChildren<FireMarker>() != null) return;

        FlammableTarget data = flammableObjects.Find(f => f.target == target);
        if (data == null) return;

        if (!heatLevels.ContainsKey(target))
            heatLevels[target] = 0;

        heatLevels[target] += amount * Time.deltaTime;

        if (heatLevels[target] >= data.heatThreshold)
        {
            heatLevels.Remove(target);
            ForceIgnite(target);
        }
    }

    private IEnumerator IgniteSequence(Transform target)
    {
        if (target == null) yield break;

        target.gameObject.AddComponent<FireMarker>();

        yield return new WaitForSeconds(ignitionDelay);

        Vector3 topPosition = GetTopPosition(target);
        GameObject fire = Instantiate(fireEffectPrefab, topPosition, Quaternion.LookRotation(Vector3.up));
        fire.transform.SetParent(target);

        FireEffectTrigger trigger = fire.GetComponent<FireEffectTrigger>();
        if (trigger == null)
        {
            trigger = fire.AddComponent<FireEffectTrigger>();
        }
        trigger.fireIgnition = this;

        IncreaseSmokeLevel();

        if (!heavyObjects.Contains(target))
        {
            yield return new WaitForSeconds(fireDuration);
            DisableParticleEmitters(target);
            Destroy(target.gameObject);
        }
    }

    private Vector3 GetTopPosition(Transform target)
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
            return target.position + new Vector3(0, 0.5f, 0);
        }
    }

   private void IncreaseSmokeLevel()
    {
        fireCount++;

        // Don't apply smoke/fog effects until the fire count reaches the threshold
        if (fireCount < smokeActivationFireThreshold) return;

        currentSmokeLevel += smokeIntensityIncrease;
        currentSmokeLevel = Mathf.Min(currentSmokeLevel, maxSmokeLevel);

        if (!RenderSettings.fog)
        {
            RenderSettings.fog = true;
        }

        RenderSettings.fogDensity = Mathf.Lerp(0f, 0.05f, currentSmokeLevel);
        RenderSettings.fogColor = Color.Lerp(Color.white, Color.gray, currentSmokeLevel);
        RenderSettings.fogStartDistance = Mathf.Lerp(0f, 10f, currentSmokeLevel);
        RenderSettings.fogEndDistance = Mathf.Lerp(60f, 40f, currentSmokeLevel);

        if (depthOfField != null)
        {
            depthOfField.focusDistance.value = Mathf.Lerp(2f, 20f, currentSmokeLevel);
            depthOfField.aperture.value = Mathf.Lerp(5f, 32f, currentSmokeLevel);
        }
    }

    private void DisableParticleEmitters(Transform target)
    {
        var sparkEmitter = target.GetComponent<RandomSparkEmitter>();
        if (sparkEmitter != null)
        {
            sparkEmitter.enabled = false;
        }

        ParticleSystem[] particles = target.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop();
        }
    }

        private void Update()
    {
        if (depthOfField == null || player == null || blurCenterTarget == null) return;

        float distance = Vector3.Distance(player.position, blurCenterTarget.position);

        // Invert the distance: closer = lower focus distance (blurrier), farther = higher (clearer)
        float t = Mathf.Clamp01(distance / blurRange);
        float invertedFocus = Mathf.Lerp(minFocusDistance, maxFocusDistance, t);
        depthOfField.focusDistance.value = invertedFocus;
    }

}

public class FireMarker : MonoBehaviour { }
