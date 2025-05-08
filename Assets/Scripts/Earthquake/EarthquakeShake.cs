using UnityEngine;

public class EarthquakeShake : MonoBehaviour
{
    public float maxMagnitude = 3f;   // Maximum shake intensity (will increase over time)
    public float duration = 10f;       // Total duration of the earthquake
    private Vector3 originalPos;      // The initial position of the object (camera)
    private float elapsed = 0f;       // Time passed since the start of the earthquake
    private bool isShaking = false;   // Flag to check if the earthquake is ongoing

    public float CurrentShakeIntensity { get; private set; }  // Current intensity of the shake

    void Start()
    {
        // Store the initial position of the camera (or any object this script is attached to)
        originalPos = transform.localPosition;
    }

    void Update()
    {
        // If the earthquake is ongoing, update the shake intensity and position
        if (isShaking)
        {
            if (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Gradually increase shake intensity over time using linear interpolation
                CurrentShakeIntensity = Mathf.Lerp(0.1f, maxMagnitude, elapsed / duration);

                // Generate a random shake movement based on the current intensity
                Vector3 randomPoint = originalPos + Random.insideUnitSphere * CurrentShakeIntensity;

                // Apply the shake, maintaining the original Z position (or use another axis if needed)
                transform.localPosition = new Vector3(randomPoint.x, randomPoint.y, originalPos.z);
            }
            else
            {
                // If the duration has passed, stop the earthquake
                StopEarthquake();
            }
        }
    }

    // Start the earthquake with a reset of elapsed time
    public void StartEarthquake()
    {
        isShaking = true;
        elapsed = 0f;  // Reset the elapsed time when the earthquake starts
    }

    // Stop the earthquake and reset the position to its original state
    public void StopEarthquake()
    {
        isShaking = false;
        transform.localPosition = originalPos; // Reset position after shake
    }
}
