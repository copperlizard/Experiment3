using UnityEngine;
using System.Collections;

public class FiredAirBurstArrow : MonoBehaviour
{
    public GameObject m_airBurstEffect;

    public float m_airBurstRange = 5.0f, m_airBurstTime = 3.0f, m_coneFactor = 0.5f;

    private Rigidbody m_rigidBody;

    private RaycastHit m_hit;

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

            if (Physics.Raycast(transform.position, transform.forward, out m_hit, m_airBurstRange, ~LayerMask.GetMask("Arrows")))
            {
                StartCoroutine(AirBurst());
            }
        }

        Debug.DrawLine(transform.position, transform.position + transform.forward, Color.magenta);

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

        StartCoroutine(AirBurst());
    }

    IEnumerator AirBurst ()
    {
        m_flying = false;

        m_rigidBody.velocity = Vector3.zero;
        m_rigidBody.useGravity = false;
        m_rigidBody.detectCollisions = false;
        m_rigidBody.isKinematic = true;

        m_airBurstEffect.SetActive(true);

        float startTime = Time.time;

        while (startTime + m_airBurstTime > Time.time)
        {
            RaycastHit[] hits;

            hits = Physics.CapsuleCastAll(transform.position, (transform.position + (transform.forward * m_airBurstRange)), 2.0f, transform.forward, 0.0f, ~LayerMask.GetMask("Arrows"));            

            Debug.DrawLine(transform.position, transform.position + transform.forward * m_airBurstRange, Color.green, 0.5f, true);

            for (int i = 0; i < hits.Length; i++)
            {
                Vector3 toHit = hits[i].transform.position - transform.position;

                float coneCheck = Vector3.Dot(transform.forward, toHit.normalized);                

                if (hits[i].rigidbody != null && coneCheck > m_coneFactor)
                {
                    hits[i].rigidbody.AddExplosionForce(400.0f * Time.timeScale, transform.position, m_airBurstRange + 2.0f);   
                }
            }

            yield return null;
        }


        m_airBurstEffect.SetActive(false);
        gameObject.SetActive(false);

        yield return null;
    }
}
