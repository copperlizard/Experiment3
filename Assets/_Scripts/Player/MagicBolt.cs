using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MagicBolt : MonoBehaviour
{
    public GameObject m_bolt, m_burst;

    public float m_maxLife = 3.0f, m_burstTime = 1.0f, m_burstRadius = 0.5f, m_burstForce = 5.0f, m_boltDamage = 0.2f;

    private Rigidbody m_rigidBody;

    private bool m_detonated = false;

	// Use this for initialization
	void Awake ()
    {
        m_rigidBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void OnEnable ()
    {
        m_rigidBody.detectCollisions = true;
        m_rigidBody.isKinematic = false;
        m_detonated = false;
        m_bolt.SetActive(true);
        StartCoroutine(detonateTimer(m_maxLife));
    }

    void OnCollisionEnter (Collision other)
    {
        m_rigidBody.velocity = Vector3.zero;
        m_rigidBody.detectCollisions = false;
        m_rigidBody.isKinematic = true;

        if (!m_detonated)
        {
            Detonate();
        }
    }

    void Detonate ()
    {
        m_detonated = true;

        m_bolt.SetActive(false);
        m_burst.SetActive(true);

        Collider[] hits;
        hits = Physics.OverlapSphere(transform.position, m_burstRadius, ~LayerMask.GetMask("Arrows"));

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].attachedRigidbody != null)
            {
                hits[i].attachedRigidbody.AddExplosionForce(m_burstForce, transform.position, m_burstRadius, 0.5f, ForceMode.Impulse);
            }

            if (hits[i].tag == "Goblin")
            {
                GoblinStateInfo thisGoblin = hits[i].GetComponentInParent<GoblinStateInfo>();
                thisGoblin.m_health = Mathf.Clamp(thisGoblin.m_health - m_boltDamage * Time.deltaTime, 0.0f, 1.0f);
            }
        }


        StartCoroutine(deactivateTimer(m_burstTime));
    }

    IEnumerator detonateTimer (float time)
    {
        yield return new WaitForSeconds(time);
        Detonate();
    }

    IEnumerator deactivateTimer (float time)
    {
        yield return new WaitForSeconds(time);
        m_burst.SetActive(false);
        gameObject.SetActive(false);
    }
}
