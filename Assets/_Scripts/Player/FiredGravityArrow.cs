using UnityEngine;
using System.Collections;

public class FiredGravityArrow : MonoBehaviour
{
    private Rigidbody m_rigidBody;

    private bool m_flying = true;

    // Use this for initialization
    void Awake()
    {
        m_rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_flying)
        {
            if (m_rigidBody.velocity.magnitude > 0.0f)
            {
                transform.rotation = Quaternion.LookRotation(m_rigidBody.velocity);
            }
        }

    }

    private void OnEnable()
    {
        m_flying = true;

        m_rigidBody.useGravity = true;
        m_rigidBody.detectCollisions = true;
        m_rigidBody.isKinematic = false;

        transform.parent = null;
    }

    private void OnCollisionEnter(Collision other)
    {
        m_flying = false;

        m_rigidBody.velocity = Vector3.zero;
        m_rigidBody.useGravity = false;
        m_rigidBody.detectCollisions = false;
        m_rigidBody.isKinematic = true;

        if (other.rigidbody != null)
        {
            transform.parent = other.transform;
        }
    }
}
