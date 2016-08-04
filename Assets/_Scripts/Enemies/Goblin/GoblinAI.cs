using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GoblinStateInfo))]
[RequireComponent(typeof(GoblinMovementController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class GoblinAI : MonoBehaviour
{
    public GameManager m_gameManager;

    public GameObject m_player;

    [System.Serializable]
    public struct SFX
    {
        [HideInInspector]
        public AudioSource m_goblinSFXSource;
        public AudioClip m_attackNoise, m_painNoise, m_idleNoise;
    }
    public SFX m_goblinSFX;

    public float m_goblinSightDetectionRange = 4.0f;

    private List<Transform> m_patrolPoints = new List<Transform>();

    private PlayerStateInfo m_playerStateInfo;

    private Transform m_leftHand, m_rightHand, m_head;

    private GoblinStateInfo m_goblinState;
    private GoblinMovementController m_goblinMover;
    private Animator m_goblinAnimator;
    private AnimatorStateInfo m_goblinAnimatorState;
    
    private NavMeshAgent m_goblinNavAgent;
    private NavMeshPath m_goblinPath;

    private float m_v, m_h, m_lastHealth = 1.0f;

    private int m_curPatrolPoint = 0, m_curPathPoint = 0;

    private bool m_goodPath = false, m_playerVisible, m_searching, m_pathPausing = false, m_painSoundLock = false;

    // Use this for initialization
    void Start ()
    {
        if (m_gameManager == null)
        {
            m_gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        }

        if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        m_playerStateInfo = m_player.GetComponent<PlayerStateInfo>();

        m_goblinState = GetComponent<GoblinStateInfo>();
        m_goblinMover = GetComponent<GoblinMovementController>();
        m_goblinAnimator = GetComponent<Animator>();

        m_leftHand = m_goblinAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
        m_rightHand = m_goblinAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        m_head = m_goblinAnimator.GetBoneTransform(HumanBodyBones.Head);

        m_goblinNavAgent = GetComponentInChildren<NavMeshAgent>();
        
        m_goblinPath = new NavMeshPath();

        GameObject[] patrolPointsObjs = GameObject.FindGameObjectsWithTag("GoblinPatrolPoint");

        for (int i = 0; i < patrolPointsObjs.Length; i++)
        {
            m_patrolPoints.Add(patrolPointsObjs[i].transform);
        }

        Random.seed = GetHashCode();
                
        m_curPatrolPoint = Random.Range(0, m_patrolPoints.Count);       

        m_goblinSFX.m_goblinSFXSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_gameManager.m_paused)
        {
            return;
        }

        m_goblinAnimatorState = m_goblinAnimator.GetCurrentAnimatorStateInfo(0);

        Think();

        m_goblinMover.Move(m_v, m_h);

        /*
#if UNITY_EDITOR

        
        for (int i = 0; i < m_patrolPoints.Count - 1; i++)
        {
            Debug.DrawLine(m_patrolPoints[i].position, m_patrolPoints[i + 1].position);
            if (i == m_patrolPoints.Count - 2)
            {
                Debug.DrawLine(m_patrolPoints[0].position, m_patrolPoints[i + 1].position);
            }
        }
        

        if (m_goodPath)
        {
            for (int i = 0; i < m_goblinPath.corners.Length - 1; i++)
            {
                Debug.DrawLine(m_goblinPath.corners[i], m_goblinPath.corners[i + 1], Color.green);
            }
        }
#endif
        */
    }

    void FixedUpdate ()
    {
        m_goblinNavAgent.transform.position = transform.position;
        m_goblinNavAgent.transform.rotation = transform.rotation;
    }

    void Think ()
    {
        if (m_goblinState.m_health < m_lastHealth)
        {
            m_goblinState.m_alert = true;
            m_goblinState.m_sprinting = true;
            m_lastHealth = m_goblinState.m_health;

            if (!m_painSoundLock)
            {
                StartCoroutine(Grunt());
            }
        }

        //Debug.Log(gameObject.name + " thinking!");
        Vector3 toPlayer = m_player.transform.position - transform.position;

        float distToPlayer = toPlayer.magnitude;
        
        //Check visibility
        m_playerVisible = !Physics.Raycast(m_head.transform.position, toPlayer, distToPlayer, ~LayerMask.GetMask("Goblin", "Player", "PlayerBubble")) && (distToPlayer < m_goblinSightDetectionRange);
        if (m_playerVisible)
        {
            m_goblinState.m_alert = true;
            m_goblinState.m_sprinting = true;

            m_goblinState.m_playerLastSeenPos = m_player.transform.position;
        }
        else
        {
            //modify path to player
            toPlayer = m_goblinState.m_playerLastSeenPos - transform.position;
            
            if (toPlayer.magnitude <= 5.0f && !m_playerVisible)
            {
                m_goblinState.m_alert = false;
                m_goblinState.m_sprinting = false;
            }
        }

        //Attacking
        if (m_goblinState.m_alert)
        {
            //Debug.Log("player visible == " + m_playerVisible.ToString());
                        
            if (!m_playerVisible && !m_searching)
            {
                //Debug.Log("seaching for player!");

                m_searching = true;
                StartCoroutine(AlertTimer());
            }
                
            if (distToPlayer < 2.0f)
            {
                Melee(distToPlayer);
            }

            toPlayer = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
            toPlayer = toPlayer.normalized;
            
            m_v = toPlayer.z;
            m_h = toPlayer.x;
        }
        //Patrolling
        else
        {
            if (!m_goblinState.m_grounded || m_pathPausing)
            {
                //Debug.Log(gameObject.name + " patrol paused!");

                //No patrolling in the air
                return;
            }

            //Debug.Log(gameObject.name + " is patrolling!");

            Vector3 toCurPoint = m_patrolPoints[m_curPatrolPoint].position - transform.position;

            float distToPoint = toCurPoint.magnitude;

            //Check dist to patrol point
            if (distToPoint < 0.5f)
            {
                //Randomly choose new patrol point
                int lastPoint = m_curPatrolPoint;
                while (m_curPatrolPoint == lastPoint)
                {
                    m_curPatrolPoint = Random.Range(0, m_patrolPoints.Count);
                }

                StartCoroutine(PatrolPause());

                //Debug.Log(gameObject.name + " patrolling to " + m_patrolPoints[m_curPatrolPoint].name);

                return;
            }

            NavMeshHit interrupt;

            bool crowPath = !m_goblinNavAgent.Raycast(m_patrolPoints[m_curPatrolPoint].position, out interrupt);

            //As the crow flies
            if (crowPath)
            {
                toCurPoint = Vector3.ProjectOnPlane(toCurPoint, Vector3.up);
                toCurPoint = toCurPoint.normalized;

                m_v = toCurPoint.z;
                m_h = toCurPoint.x;

                m_goodPath = false;
            }
            //Following Path
            else
            {
                //Check if path
                if (!m_goodPath)
                {
                    //Get path
                    m_goodPath = m_goblinNavAgent.CalculatePath(m_patrolPoints[m_curPatrolPoint].position, m_goblinPath);
                    m_curPathPoint = 0;
                }
                //Traverse path
                else
                {
                    //Verify Path
                    bool pathVerified = !m_goblinNavAgent.Raycast(m_goblinPath.corners[m_curPathPoint], out interrupt);

                    if (!pathVerified)
                    {
                        m_goodPath = false;
                        return;
                    }

                    Vector3 toPathPoint = m_goblinPath.corners[m_curPathPoint] - transform.position;

                    float distToPathPoint = toPathPoint.magnitude;

                    //Check dist to corner
                    if (distToPathPoint < 0.5f)
                    {
                        //Proceed to next corner
                        m_curPathPoint += 1;

                        if (m_curPathPoint >= m_goblinPath.corners.Length)
                        {
                            //Completed path
                            m_goodPath = false;
                            return;
                        }

                        return;
                    }

                    //Move towards corner
                    toPathPoint = Vector3.ProjectOnPlane(toPathPoint, Vector3.up);
                    toPathPoint = toPathPoint.normalized;

                    m_v = toPathPoint.z;
                    m_h = toPathPoint.x;
                }
            }
        }        
    }

    IEnumerator Grunt ()
    {
        m_painSoundLock = true;
        m_goblinSFX.m_goblinSFXSource.pitch = Random.Range(0.9f, 1.1f);        
        m_goblinSFX.m_goblinSFXSource.PlayOneShot(m_goblinSFX.m_painNoise, (m_goblinState.m_alert) ? 1.0f : 0.5f);        
        
        Collider[] hits = Physics.OverlapSphere(transform.position, 10.0f);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].tag == "Goblin")
            {
                GoblinStateInfo thisGoblin = hits[i].GetComponent<GoblinStateInfo>();

                if (thisGoblin != null)
                {
                    thisGoblin.m_alert = true;
                    thisGoblin.m_sprinting = true;
                    thisGoblin.m_playerLastSeenPos = m_goblinState.m_playerLastSeenPos;
                }
            }
        }
        
        yield return new WaitForSeconds(Random.Range(2.0f, 4.0f));
        m_painSoundLock = false;
        yield return null;
    }

    IEnumerator AlertTimer ()
    {
        yield return new WaitForSeconds(Random.Range(30.0f, 45.0f));

        if (!m_playerVisible)
        {
            m_goblinState.m_alert = false;
            m_goblinState.m_sprinting = false;

            //Debug.Log("player lost");
        }
        else
        {
            //Debug.Log("player was found");
        }

        m_searching = false;

        yield return null;
    }

    IEnumerator PatrolPause ()
    {
        m_pathPausing = true;

        m_goblinSFX.m_goblinSFXSource.PlayOneShot(m_goblinSFX.m_idleNoise);

        m_v = 0.0f;
        m_h = 0.0f;

        yield return new WaitForSeconds(Random.Range(0.0f, 5.0f));

        m_pathPausing = false;

        yield return null;
    }

    void Melee (float dist)
    {
        if (m_goblinAnimatorState.IsName("GoblinSwipe") || m_goblinAnimatorState.IsName("GoblinPunch"))
        {
            return;
        }

        m_goblinSFX.m_goblinSFXSource.pitch = Random.Range(0.8f, 1.1f);
        m_goblinSFX.m_goblinSFXSource.PlayOneShot(m_goblinSFX.m_attackNoise);

        if (dist > 1.5f)
        {
            m_goblinAnimator.Play("GoblinSwipe", 0);
            return;
        }

        if (Random.Range(0.0f, 1.0f) > 0.5f)
        {
            m_goblinAnimator.Play("GoblinSwipe", 0);
        }
        else
        {
            m_goblinAnimator.Play("GoblinPunch", 0);
        }
        
    }

    void Swipe ()
        //animation event
    {
        //left hand
        Collider[] hits = Physics.OverlapSphere(m_leftHand.position, 0.5f, LayerMask.GetMask("PlayerBubble"));

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].tag == "Player")
            {
                m_playerStateInfo.m_health = Mathf.Clamp(m_playerStateInfo.m_health - 0.2f, 0.0f, 1.0f);
            }
        }
    }

    void Punch ()
    //animation event
    {
        Collider[] hits = Physics.OverlapSphere(m_rightHand.position, 0.5f, LayerMask.GetMask("PlayerBubble"));

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].tag == "Player")
            {
                m_playerStateInfo.m_health = Mathf.Clamp(m_playerStateInfo.m_health - 0.1f, 0.0f, 1.0f);
            }
        }
    }
}
