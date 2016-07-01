using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GoblinStateInfo))]
[RequireComponent(typeof(GoblinMovementController))]
public class GoblinAI : MonoBehaviour
{
    public GameManager m_gameManager;

    public GameObject m_player;

    private GoblinStateInfo m_goblinState;
    private GoblinMovementController m_goblinMover;

    private float m_v, m_h;

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

        m_goblinState = GetComponent<GoblinStateInfo>();
        m_goblinMover = GetComponent<GoblinMovementController>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_gameManager.m_paused)
        {
            return;
        }

        Think();

        m_goblinMover.Move(m_v, m_h);
    }

    void Think ()
    {
        Vector3 toPlayer = m_player.transform.position - transform.position;

        float distToPlayer = toPlayer.magnitude;

        toPlayer = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
        toPlayer = toPlayer.normalized;

        //Debug.DrawLine(transform.position, transform.position + toPlayer);

        m_v = toPlayer.z;
        m_h = toPlayer.x;
    }
}
