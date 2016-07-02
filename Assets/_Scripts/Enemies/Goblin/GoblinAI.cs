using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GoblinStateInfo))]
[RequireComponent(typeof(GoblinMovementController))]
[RequireComponent(typeof(Animator))]
public class GoblinAI : MonoBehaviour
{
    public GameManager m_gameManager;

    public GameObject m_player;

    public List<Transform> m_patrolPoints = new List<Transform>();

    private PlayerStateInfo m_playerStateInfo;

    private Transform m_leftHand, m_rightHand;

    private GoblinStateInfo m_goblinState;
    private GoblinMovementController m_goblinMover;
    private Animator m_goblinAnimator;
    private AnimatorStateInfo m_goblinAnimatorState;

    private NavMeshAgent m_goblinNavAgent;
    private NavMeshPath m_goblinPath;

    private float m_v, m_h;

    private int m_curPatrolPoint = 0, m_curPathPoint = 0;

    private bool m_goodPath = false;

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

        m_goblinNavAgent = GetComponentInChildren<NavMeshAgent>();
        
        m_goblinPath = new NavMeshPath();
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
    }

    void FixedUpdate ()
    {
        m_goblinNavAgent.transform.position = transform.position;
        m_goblinNavAgent.transform.rotation = transform.rotation;
    }

    void Think ()
    {
        //Attacking
        if (m_goblinState.m_alert)
        {
            Vector3 toPlayer = m_player.transform.position - transform.position;

            float distToPlayer = toPlayer.magnitude;

            if (distToPlayer < 2.0f)
            {
                //Debug.Log("attack range!");

                Melee(distToPlayer);
            }

            toPlayer = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
            toPlayer = toPlayer.normalized;

            //Debug.DrawLine(transform.position, transform.position + toPlayer);

            m_v = toPlayer.z;
            m_h = toPlayer.x;
        }
        //Patrolling
        else
        {
            Vector3 toCurPoint = m_patrolPoints[m_curPatrolPoint].position - transform.position;

            float distToPoint = toCurPoint.magnitude;

            //Check dist to patrol point
            if (distToPoint < 0.5f)
            {
                //Randomly choose new patrol point
                m_curPatrolPoint = Random.Range(0, m_patrolPoints.Count);
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
                //Check path
                if (!m_goodPath)
                {
                    m_goodPath = m_goblinNavAgent.CalculatePath(m_patrolPoints[m_curPatrolPoint].position, m_goblinPath);
                    m_curPathPoint = 0;
                }
                //Traverse path
                else
                {
                    Vector3 toPathPoint = m_goblinPath.corners[m_curPathPoint] - transform.position;

                    float distToPathPoint = toPathPoint.magnitude;

                    //Check dist to corner
                    if (distToPathPoint < 0.5f)
                    {
                        //Proceed to next corner
                        m_curPathPoint++;

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

    void Melee (float dist)
    {
        if (m_goblinAnimatorState.IsName("GoblinSwipe") || m_goblinAnimatorState.IsName("GoblinPunch"))
        {
            return;
        }

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
