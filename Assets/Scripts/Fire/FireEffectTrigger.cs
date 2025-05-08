using UnityEngine;

public class FireEffectTrigger : MonoBehaviour
{
    public FireIgnition fireIgnition;
    public float spreadRadius = 3f;
    public float spreadInterval = 1f;
    public float heatPerSecond = 25f;

    private float timer;

    void Start()
    {
        if (fireIgnition == null)
        {
            Debug.LogError("Missing reference to FireIgnition in FireEffectTrigger!", this);
        }
    }

    void Update()
    {
        if (fireIgnition == null) return;

        timer += Time.deltaTime;

        if (timer >= spreadInterval)
        {
            SpreadFire();
            timer = 0f;
        }
    }

    void SpreadFire()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, spreadRadius);
        foreach (Collider col in colliders)
        {
            Transform target = col.transform;

            if (target.CompareTag(fireIgnition.flammableTag) && target.GetComponentInChildren<FireMarker>() == null)
            {
                fireIgnition.AddHeat(target, heatPerSecond);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spreadRadius);
    }
}
