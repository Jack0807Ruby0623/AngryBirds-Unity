using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class YellowBird : Ball 
{
    [Header("💥 爆炸参数")]
    public float explosionRadius = 3f;      
    public float explosionForce = 500f;     
    public float forceFalloff = 0.5f;       

    [Header("🎨 特效")]
    public GameObject explosionEffectPrefab; 
    public AudioClip explosionSound;       

    [Header("🎥 相机抖动（剧烈旋转版）")]
    public bool enableCameraShake = true;
    [Range(5f, 30f)]
    public float cameraShakeRotationAmount = 15f;  
    [Range(0.3f, 1.2f)]
    public float cameraShakeDuration = 0.6f;      

    private bool hasExploded = false;       
    private Camera mainCamera;             

    void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.CompareTag("Interactable") ||
            collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Enemy"))
        {
            TriggerExplosion();
        }
    }

    void TriggerExplosion()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null && !ps.isPlaying)
            {
                ps.Play();

                var renderer = ps.GetComponent<ParticleSystemRenderer>();

                if (renderer != null)
                {
                    renderer.sortingLayerName = "Effects";   
                    renderer.sortingOrder = 100;           
                }
            }
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        if (enableCameraShake && mainCamera != null)
        {
            StartCoroutine(DoCameraRotationShake());
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Interactable") && col.attachedRigidbody != null)
            {
                Vector2 direction = (col.transform.position - transform.position).normalized;
                float distance = Vector2.Distance(col.transform.position, transform.position);

                float force = explosionForce * Mathf.Max(0, 1 - (distance / explosionRadius) * forceFalloff);
                col.attachedRigidbody.AddForce(direction * force, ForceMode2D.Impulse);

                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    float damage = 50f * Mathf.Max(0, 1 - (distance / explosionRadius)); 
                    enemy.TakeDamage(damage);
                }
            }
        }

        StartCoroutine(ReleaseAfterExplosion());
    }

    IEnumerator DoCameraRotationShake()
    {
        if (mainCamera == null) yield break;

        Transform camTransform = mainCamera.transform;
        Quaternion originalRotation = camTransform.localRotation;
        Vector3 originalPosition = camTransform.localPosition;
        float elapsed = 0f;

        float maxAngle = cameraShakeRotationAmount;
        float duration = cameraShakeDuration;
        float frequency = 8f;
        float randomness = 0.4f;
        float maxPosShake = 0.2f;

        while (elapsed < duration)
        {
            float decay = Mathf.Pow(1 - elapsed / duration, 2);

            float baseShake = Mathf.Sin(elapsed * frequency) * maxAngle * decay;
            float randomShake = Random.Range(-1f, 1f) * randomness * maxAngle * decay;
            float totalShake = baseShake + randomShake;
            camTransform.localRotation = originalRotation * Quaternion.Euler(0, 0, totalShake);

            Vector3 posOffset = Random.insideUnitCircle * maxPosShake * decay;
            posOffset.z = 0;
            camTransform.localPosition = originalPosition + posOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        while (Quaternion.Angle(camTransform.localRotation, originalRotation) > 0.1f ||
               Vector3.Distance(camTransform.localPosition, originalPosition) > 0.01f)
        {
            camTransform.localRotation = Quaternion.Lerp(camTransform.localRotation, originalRotation, Time.deltaTime * 10f);
            camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, originalPosition, Time.deltaTime * 10f);
            yield return null;
        }

        camTransform.localRotation = originalRotation;
        camTransform.localPosition = originalPosition;
    }

    IEnumerator ReleaseAfterExplosion()
    {
        yield return new WaitForSeconds(0.1f);

        SpringJoint2D spring = GetComponent<SpringJoint2D>();
        if (spring != null) spring.enabled = false;

        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        gameObject.SetActive(false);

        if (nextball != null)
        {
            nextball.SetActive(true);
        }
        else
        {
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}