using UnityEngine;
using System.Collections;

public class FiredGravityArrow : MonoBehaviour
{
    public GameObject m_gravEffect, m_endGravEffect;

    public float m_gravEffectTime = 5.0f, m_endGravEffectTime = 1.0f, m_gravEffectRadius = 5.0f, m_gravEffectForce = 10.0f, m_endGravEffectForce = 5.0f;

    private Rigidbody m_rigidBody;

    private PlayerStateInfo m_trappedPlayer;

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

        StartCoroutine(GravityEffect());
    }

    IEnumerator GravityEffect ()
    {
        m_gravEffect.SetActive(true);

        float startTime = Time.time;

        while (startTime + m_gravEffectTime > Time.time)
        {
            //apply grav force

            Collider[] hits = Physics.OverlapSphere(transform.position, m_gravEffectRadius);

            if (m_trappedPlayer != null)
            {
                if (m_trappedPlayer.m_gravLocked)
                {
                    m_trappedPlayer.m_gravLocked = false;
                }
            }

            for (int i = 0; i < hits.Length; i++)
            {                
                if (hits[i].tag == "Player")
                {
                    m_trappedPlayer = hits[i].GetComponent<PlayerStateInfo>();
                    m_trappedPlayer.m_gravLocked = true;                                        
                }                

                if (hits[i].attachedRigidbody != null)
                {
                    hits[i].attachedRigidbody.AddExplosionForce(-m_gravEffectForce, transform.position, m_gravEffectRadius * 2.0f, -0.5f);
                }
            }

            if (m_trappedPlayer != null)
            {
                if (!m_trappedPlayer.m_gravLocked)
                {
                    m_trappedPlayer = null;
                }
            }

            yield return null;
        }

        if (m_trappedPlayer != null)
        {
            if (m_trappedPlayer.m_gravLocked)
            {
                m_trappedPlayer.m_gravLocked = false;
                m_trappedPlayer = null;
            }
        }

        m_gravEffect.SetActive(false);
        m_endGravEffect.SetActive(true);

        startTime = Time.time;

        while (startTime + m_endGravEffectTime > Time.time)
        {
            //apply explode force

            Collider[] hits = Physics.OverlapSphere(transform.position, m_gravEffectRadius);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].attachedRigidbody != null)
                {
                    hits[i].attachedRigidbody.AddExplosionForce(m_endGravEffectForce, transform.position, m_gravEffectRadius, 0.5f);
                }
            }

            yield return null;
        }

        m_endGravEffect.SetActive(false);

        gameObject.SetActive(false);

        yield return null;
    }
}
