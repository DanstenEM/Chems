using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    public float lifeTime = 0.1f;

    LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Init(Vector3 start, Vector3 end)
    {
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        Destroy(gameObject, lifeTime);
    }
}