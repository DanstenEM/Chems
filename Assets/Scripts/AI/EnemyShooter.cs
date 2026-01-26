using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.6f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float fireDistance = 50f;
    [Header("Tracer")]
    [SerializeField] private BulletTracer tracerPrefab;
    [SerializeField] private Transform muzzle;

    float nextFireTime;

    public void ShootAt(Transform target)
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        if (!muzzle) muzzle = transform;

        Vector3 dir = (target.position + Vector3.up) - muzzle.position;
        Ray ray = new Ray(muzzle.position, dir.normalized);

        Vector3 startPoint = muzzle.position;
        Vector3 endPoint = muzzle.position + dir.normalized * fireDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, fireDistance))
        {
            endPoint = hit.point;

            Health hp = hit.collider.GetComponentInParent<Health>();
            if (hp != null)
                hp.TakeDamage(damage);
        }

        SpawnTracer(startPoint, endPoint);

        Debug.DrawRay(startPoint, dir, Color.red, 0.2f);
    }
    void SpawnTracer(Vector3 start, Vector3 end)
    {
        if (!tracerPrefab) return;

        BulletTracer tracer = Instantiate(tracerPrefab);
        tracer.Init(start, end);
    }
}