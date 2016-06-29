using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GoblinStateInfo))]
[RequireComponent(typeof(GoblinMovementController))]
public class GoblinAI : MonoBehaviour
{
    public GameManager m_gameManager;

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

    }
}
