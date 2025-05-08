using UnityEngine;

public class EarthquakePhysics : MonoBehaviour
{
    private Rigidbody rb;
    private bool isShaking = false;
    private float shakeIntensity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on the object");
        }
    }

    void Update()
    {
        if (isShaking)
        {
            ApplyShake(shakeIntensity);
        }
    }

    // Start shaking and set the intensity
    public void StartShaking(float intensity)
    {
        shakeIntensity = intensity;
        isShaking = true;
    }

    // Stop shaking
    private void StopShaking()
    {
        isShaking = false;
    }

    // Apply a shake force on the object
    private void ApplyShake(float intensity)
    {
        if (rb != null)
        {
            // Apply a random force and torque to simulate shaking
            Vector3 shakeForce = Random.insideUnitSphere * intensity;
            rb.AddForce(shakeForce, ForceMode.Impulse);  // Impulse for quick shakes

            // Apply a torque to make the object rotate as well (for realistic shaking)
            Vector3 shakeTorque = Random.insideUnitSphere * intensity;
            rb.AddTorque(shakeTorque, ForceMode.Impulse);
        }
    }
}
