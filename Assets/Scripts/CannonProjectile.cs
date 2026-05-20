using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CannonProjectile : BaseProjectile
{
    [SerializeField] private Rigidbody m_rigidbody;
    private Vector3 m_velocity;

    public void Init(Vector3 direction)
    {
        m_velocity = direction * m_speed;
        m_rigidbody.linearVelocity = m_velocity;
    }

    protected override void Move()
    {
        //No move besides Rigidbody Physics. Only rotation for non-symitric projectiles.
        if (m_velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(m_velocity);
    }
}