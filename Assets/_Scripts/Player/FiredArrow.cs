using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class FiredArrow : MonoBehaviour
{
    public GameObject m_player;

    public HUD m_HUD;

    public float m_headShotDamage = 0.4f, m_bodyShotDamage = 0.2f, m_damageOverTime = 0.01f;

    private Rigidbody m_rigidBody;

    private GoblinStateInfo m_hitGoblinState;

    private bool m_flying = true;

	// Use this for initialization
	void Awake ()
    {
        m_rigidBody = GetComponent<Rigidbody>();	

        if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        if (m_HUD == null)
        {
            m_HUD = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUD>();
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (m_flying)
        {
            if (m_rigidBody.velocity.magnitude > 0.0f)
            {
                transform.rotation = Quaternion.LookRotation(m_rigidBody.velocity);
            }
        }
        else
        {
            if (m_hitGoblinState != null)
            {
                m_hitGoblinState.m_health = Mathf.Clamp(m_hitGoblinState.m_health - m_damageOverTime * Time.deltaTime, 0.0f, 1.0f);
            }
        }
        
	}

    private void OnEnable ()
    {
        m_flying = true;

        m_rigidBody.useGravity = true;
        m_rigidBody.detectCollisions = true;
        m_rigidBody.isKinematic = false;

        transform.parent = null;

        m_hitGoblinState = null;
    }

    
    private void OnCollisionEnter (Collision other)
    {        
        m_flying = false;

        m_rigidBody.velocity = Vector3.zero;
        m_rigidBody.useGravity = false;
        m_rigidBody.detectCollisions = false;
        m_rigidBody.isKinematic = true;
        
        if (other.rigidbody != null)
        {
            transform.parent = other.transform;

            if (other.transform.tag == "Goblin")
            {
                m_HUD.IndicateHit();

                m_hitGoblinState = other.transform.GetComponentInParent<GoblinStateInfo>();

                if (other.transform.name == "Head")
                {
                    Debug.Log("headshot!");

                    m_hitGoblinState.m_health = Mathf.Clamp(m_hitGoblinState.m_health - m_headShotDamage, 0.0f, 1.0f);
                }
                else
                {
                    m_hitGoblinState.m_health = Mathf.Clamp(m_hitGoblinState.m_health - m_bodyShotDamage, 0.0f, 1.0f);
                }

                m_hitGoblinState.m_playerLastSeenPos = m_player.transform.position;
            }
        }        
    }
    

    /*
    private void OnTriggerEnter(Collider other)
    {
        m_flying = false;

        m_rigidBody.velocity = Vector3.zero;
        m_rigidBody.useGravity = false;
        m_rigidBody.detectCollisions = false;
        m_rigidBody.isKinematic = true;

        if (other.attachedRigidbody != null)
        {
            transform.parent = other.transform;

            if (other.transform.tag == "Goblin")
            {
                m_hitGoblinState = other.transform.GetComponentInParent<GoblinStateInfo>();

                if (other.transform.name == "Head")
                {
                    Debug.Log("headshot!");

                    m_hitGoblinState.m_health = Mathf.Clamp(m_hitGoblinState.m_health - m_headShotDamage, 0.0f, 1.0f);
                }
                else
                {
                    m_hitGoblinState.m_health = Mathf.Clamp(m_hitGoblinState.m_health - m_bodyShotDamage, 0.0f, 1.0f);
                }

                m_hitGoblinState.m_playerLastSeenPos = m_player.transform.position;
            }
        }
    }
    */
}
