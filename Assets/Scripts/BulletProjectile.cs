using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    public float speed = 120f;
    public float lifeTime = 3f;
    public float damage = 25f;
    public GameObject impactPrefab;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Init(Vector3 direction)
    {
        rb.linearVelocity = direction * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit: " + collision.collider.name);
        // DAMAGE
        Health health = collision.collider.GetComponentInParent<Health>();
        if (health)
            health.TakeDamage(damage);

        // IMPACT FX
        if (impactPrefab && collision.contacts.Length > 0)
        {
            ContactPoint p = collision.contacts[0];
            Instantiate(impactPrefab, p.point, Quaternion.LookRotation(p.normal));
        }

        // STOP physics influence immediately
        rb.isKinematic = true;

        Destroy(gameObject);
    }
}