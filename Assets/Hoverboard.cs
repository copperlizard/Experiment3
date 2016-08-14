using UnityEngine;
using System.Collections;

public class Hoverboard : MonoBehaviour
{
    public GameObject m_player;
    private PlayerStateInfo m_playerState;

    private bool m_playerOnBoard = false, m_playerSurfing = false, m_surfLocked = false;

	// Use this for initialization
	void Start ()
    {
	    if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        m_playerState = m_player.GetComponent<PlayerStateInfo>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Get on/off board
	    if (m_playerOnBoard)
        {
            if (!m_surfLocked)
            {
                if (Input.GetButton("Interact"))
                {
                    m_playerSurfing = !m_playerSurfing;
                    m_playerState.m_surfing = m_playerSurfing; //set player state
                    m_surfLocked = true;
                }
            }
            else
            {
                if (!Input.GetButton("Interact"))
                {
                    m_surfLocked = false;
                }
            }
        }

        //Control board
        if (m_playerSurfing)
        {

        }
	}

    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Player")
        {
            m_playerOnBoard = true;
        }
    }

    void OnTriggerExit (Collider other)
    {
        if (other.tag == "Player")
        {
            m_playerOnBoard = false;
        }
    }
}
