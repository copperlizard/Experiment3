using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FiredGravityArrow : MonoBehaviour
{
    public GameObject m_gravEffect, m_endGravEffect, m_player;

    public float m_gravEffectTime = 5.0f, m_endGravEffectTime = 1.0f, m_gravEffectRadius = 5.0f, m_gravEffectForce = 60000.0f, m_endGravEffectForce = 10000.0f, m_gravDamage = 0.01f;

    private Rigidbody m_rigidBody;

    private PlayerStateInfo m_trappedPlayerState;
    private List<GoblinStateInfo> m_trappedGoblins = new List<GoblinStateInfo>();

    private Vector3 m_firedFromPos;

    private bool m_flying = true;

    // Use this for initialization
    void Awake()
    {
        m_rigidBody = GetComponent<Rigidbody>();

        if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }
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

        m_firedFromPos = m_player.transform.position;
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

        //apply grav force
        while (startTime + m_gravEffectTime > Time.time)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_gravEffectRadius);
            
            //Give player escape chance
            if (m_trappedPlayerState != null)
            {
                if (m_trappedPlayerState.m_gravLocked)
                {
                    m_trappedPlayerState.m_gravLocked = false;
                }
            }
            
            //Give goblins escape chance
            if (m_trappedGoblins.Count > 0)
            {
                for (int i = 0; i < m_trappedGoblins.Count; i++)
                {
                    m_trappedGoblins[i].m_gravLocked = false;
                }
            }                        

            for (int i = 0; i < hits.Length; i++)
            {
                //Player trapped                
                if (hits[i].tag == "Player")
                {
                    if (m_trappedPlayerState == null)
                    {
                        m_trappedPlayerState = hits[i].GetComponent<PlayerStateInfo>();
                    }
                    
                    m_trappedPlayerState.m_gravLocked = true;
                    m_trappedPlayerState.m_health = Mathf.Clamp(m_trappedPlayerState.m_health - m_gravDamage * Time.deltaTime, 0.0f, 1.0f);                                        
                }
                //Goblin trapped
                else if (hits[i].tag == "Goblin")
                {
                    //Debug.Log("hit goblin " + hits[i].name);

                    GoblinStateInfo thisGoblin = hits[i].GetComponentInParent<GoblinStateInfo>();

                    if (!thisGoblin.m_gravLocked)
                    {
                        thisGoblin.m_gravLocked = true;
                        thisGoblin.m_health = Mathf.Clamp(thisGoblin.m_health - m_gravDamage * Time.deltaTime, 0.0f, 1.0f);
                        thisGoblin.m_playerLastSeenPos = m_firedFromPos;
                        m_trappedGoblins.Add(thisGoblin);
                    }
                }                

                if (hits[i].attachedRigidbody != null)
                {
                    hits[i].attachedRigidbody.AddExplosionForce(-m_gravEffectForce * Time.deltaTime, transform.position, m_gravEffectRadius * 2.0f, -0.5f);
                }
            }
            
            //Player escaped
            if (m_trappedPlayerState != null)
            {
                if (!m_trappedPlayerState.m_gravLocked)
                {
                    m_trappedPlayerState = null;
                }
            }
            
            //Check for escaped goblins
            if (m_trappedGoblins.Count > 0)
            {
                for (int i = 0; i < m_trappedGoblins.Count;)
                {
                    if (!m_trappedGoblins[i].m_gravLocked)
                    {
                        m_trappedGoblins.Remove(m_trappedGoblins[i]);
                    }
                    else
                    {
                        i++;
                    }
                }
            }            

            yield return null;
        }

        //Free player
        if (m_trappedPlayerState != null)
        {
            if (m_trappedPlayerState.m_gravLocked)
            {
                m_trappedPlayerState.m_gravLocked = false;
                m_trappedPlayerState = null;
            }
        }

        //Free goblins        
        if (m_trappedGoblins.Count > 0)
        {
            for (int i = 0; i < m_trappedGoblins.Count; i++)
            {
                if (m_trappedGoblins[i].m_gravLocked)
                {
                    m_trappedGoblins[i].m_gravLocked = false;
                    m_trappedGoblins.Remove(m_trappedGoblins[i]);
                }
            }
        }

        m_gravEffect.SetActive(false);
        m_endGravEffect.SetActive(true);

        startTime = Time.time;

        //apply explode force
        while (startTime + m_endGravEffectTime > Time.time)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_gravEffectRadius);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].attachedRigidbody != null)
                {
                    hits[i].attachedRigidbody.AddExplosionForce(m_endGravEffectForce * Time.deltaTime, transform.position, m_gravEffectRadius, 0.5f);
                }
            }

            yield return null;
        }

        m_endGravEffect.SetActive(false);

        gameObject.SetActive(false);

        yield return null;
    }
}
