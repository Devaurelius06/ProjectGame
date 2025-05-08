using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EarthquakeManager : MonoBehaviour
{
    public EarthquakeShake cameraShake;
    public List<Rigidbody> floorObjects;
    public List<Rigidbody> wallObjects;
    public List<Rigidbody> ceilingObjects;

    public float earthquakeIntervalMin = 10f;
    public float earthquakeIntervalMax = 15f;
    public float fallIntensityThreshold = 1f;
    public float minShakeDuration = 1f;  // Minimum time of shaking required before objects can fall

    public float jointBreakForceMultiplier = 10f; // Allows tuning joint break sensitivity
    public float massThresholdLight = 1f; // Light objects <= 1
    public float massThresholdHeavy = 3f; // Heavy objects >= 3

    private List<Rigidbody> lightFloorObjects = new List<Rigidbody>();
    private List<Rigidbody> heavyFloorObjects = new List<Rigidbody>();

    private List<Rigidbody> lightWallObjects = new List<Rigidbody>();
    private List<Rigidbody> heavyWallObjects = new List<Rigidbody>();

    private List<Rigidbody> lightCeilingObjects = new List<Rigidbody>();
    private List<Rigidbody> heavyCeilingObjects = new List<Rigidbody>();

    private float shakeStartTime = 0f;  // Start time for shake duration check

    private void Start()
    {
        CategorizeObjectsByMass();
        StartCoroutine(TriggerEarthquakeAtRandomIntervals());
    }

    private IEnumerator TriggerEarthquakeAtRandomIntervals()
    {
        while (true)
        {
            float waitTime = Random.Range(earthquakeIntervalMin, earthquakeIntervalMax);
            yield return new WaitForSeconds(waitTime);
            TriggerEarthquake();
        }
    }

    public void TriggerEarthquake()
    {
        shakeStartTime = Time.time; // Set the shake start time for duration checks
        cameraShake.StartEarthquake();

        // Apply shaking with varying intensities based on object weight
        ApplyShake(lightFloorObjects, cameraShake.CurrentShakeIntensity * 0.5f, true);  // Light objects start small, grow
        ApplyShake(heavyFloorObjects, cameraShake.CurrentShakeIntensity * 1.5f, false); // Heavy objects need more intensity to shake

        ApplyShake(lightWallObjects, cameraShake.CurrentShakeIntensity * 0.5f, true);  // Light objects start small, grow
        ApplyShake(heavyWallObjects, cameraShake.CurrentShakeIntensity * 1.5f, false); // Heavy objects need more intensity to shake

        ApplyShake(lightCeilingObjects, cameraShake.CurrentShakeIntensity * 0.5f, true);  // Light objects start small, grow
        ApplyShake(heavyCeilingObjects, cameraShake.CurrentShakeIntensity * 1.5f, false); // Heavy objects need more intensity to shake
    }

    private void ApplyShake(List<Rigidbody> objects, float intensity, bool isLightObject)
    {
        foreach (var obj in objects)
        {
            if (obj == null) continue;

            EarthquakePhysics eqPhysics = obj.GetComponent<EarthquakePhysics>() ?? obj.gameObject.AddComponent<EarthquakePhysics>();
            eqPhysics.StartShaking(cameraShake.CurrentShakeIntensity);

            // For light objects, start shaking as soon as camera shake starts
            if (isLightObject)
            {
                // Apply initial small force for light objects
                float lightShakeIntensity = Mathf.Lerp(0.1f, intensity, cameraShake.CurrentShakeIntensity / 3);  // Scale shake intensity between 0.1 and the current intensity
                obj.AddForce(Random.insideUnitSphere * lightShakeIntensity, ForceMode.Impulse);

                // Optionally, you could apply a growing shake as well
                float growingShakeIntensity = Mathf.Lerp(0.5f, intensity, cameraShake.CurrentShakeIntensity / 3);
                obj.AddForce(Random.insideUnitSphere * growingShakeIntensity, ForceMode.Impulse);
            }
            else
            {
                // For heavy objects, apply stronger initial force for more mass
                float heavyShakeIntensity = Mathf.Lerp(1.0f, intensity * 1.5f, cameraShake.CurrentShakeIntensity / 3); // Heavier objects need more intensity
                obj.AddForce(Random.insideUnitSphere * heavyShakeIntensity, ForceMode.Impulse);
            }

            // Check if the object is tagged "Ceiling" and whether the intensity is enough for it to fall
            if (obj.CompareTag("Ceiling") && cameraShake.CurrentShakeIntensity >= fallIntensityThreshold)
            {
                if (Time.time - shakeStartTime > minShakeDuration)  // Check if the shake has been sustained for at least the minimum duration
                {
                    SetJointBreakForce(obj);
                    StartCoroutine(DelayRemoveConstraints(obj));
                }
            }
        }
    }

    private IEnumerator DelayRemoveConstraints(Rigidbody obj)
    {
        yield return new WaitForSeconds(1f); // Adding a delay before releasing constraints
        obj.constraints = RigidbodyConstraints.None;
    }

    private void SetJointBreakForce(Rigidbody obj)
    {
        // Modify joint forces here to make it break under the shaking intensity
        Joint[] joints = obj.GetComponentsInChildren<Joint>();
        foreach (var joint in joints)
        {
            joint.breakForce = joint.breakForce * jointBreakForceMultiplier;
            joint.breakTorque = joint.breakTorque * jointBreakForceMultiplier;
        }
    }

    private void CategorizeObjectsByMass()
    {
        // Categorize floor objects
        foreach (var obj in floorObjects)
        {
            if (obj == null) continue;

            if (obj.mass <= massThresholdLight)  // Light objects (mass <= 1)
                lightFloorObjects.Add(obj);
            else if (obj.mass >= massThresholdHeavy) // Heavy objects (mass >= 3)
                heavyFloorObjects.Add(obj);
        }

        // Categorize wall objects
        foreach (var obj in wallObjects)
        {
            if (obj == null) continue;

            if (obj.mass <= massThresholdLight)  // Light objects (mass <= 1)
                lightWallObjects.Add(obj);
            else if (obj.mass >= massThresholdHeavy) // Heavy objects (mass >= 3)
                heavyWallObjects.Add(obj);
        }

        // Categorize ceiling objects
        foreach (var obj in ceilingObjects)
        {
            if (obj == null) continue;

            if (obj.mass <= massThresholdLight)  // Light objects (mass <= 1)
                lightCeilingObjects.Add(obj);
            else if (obj.mass >= massThresholdHeavy) // Heavy objects (mass >= 3)
                heavyCeilingObjects.Add(obj);
        }
    }
}
