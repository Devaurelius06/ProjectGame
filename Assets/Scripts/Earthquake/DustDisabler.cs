using UnityEngine;

public class DustDisabler : MonoBehaviour
{
    private bool hasHitGround = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHitGround) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            hasHitGround = true;
            ClearAllDust();
        }
    }

    private void ClearAllDust()
    {
        ParticleSystem[] dustParticles = GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem dust in dustParticles)
        {
            if (dust.isPlaying) dust.Stop();

            // Option 1: Disable the object
            dust.gameObject.SetActive(false);

            // Option 2: Destroy it completely
            // Destroy(dust.gameObject);
        }
    }
}
