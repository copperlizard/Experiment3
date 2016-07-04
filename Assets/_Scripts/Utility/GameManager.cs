using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject m_player, m_pauseMenu;

    public List<GameObject> m_goblins = new List<GameObject>();
    
    [HideInInspector]
    public bool m_paused = false;

    private List<GoblinStateInfo> m_goblinStates = new List<GoblinStateInfo>();

    private PlayerStateInfo m_playerState;

    private bool m_pauseLock = false;

    // Use this for initialization
    void Start ()
    {
        if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }
        
        m_playerState = m_player.GetComponent<PlayerStateInfo>();

        for (int i = 0; i < m_goblins.Count; i++)
        {
            m_goblinStates.Add(m_goblins[i].GetComponent<GoblinStateInfo>());
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!m_pauseLock && Input.GetButton("Pause"))
        {
            m_pauseLock = true;
            m_paused = !m_paused;
        }
        else if (!Input.GetButton("Pause"))
        {
            m_pauseLock = false;
        }

        if (m_paused && !m_pauseMenu.activeInHierarchy)
        {
            m_pauseMenu.SetActive(true);
            Time.timeScale = 0.0f;

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else if (!m_paused && m_pauseMenu.activeInHierarchy)
        {
            m_pauseMenu.SetActive(false);
            Time.timeScale = 1.0f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!m_paused)
        {
            for (int i = 0; i < m_goblinStates.Count; i++)
            {
                if (m_goblinStates[i].m_health <= 0.0f)
                {
                    //Goblin died
                    m_goblinStates[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void PauseButton ()
    {
        m_paused = !m_paused;        
    }

    public void GameQuit ()
    {
        if (Application.isPlaying)
        {
            Application.Quit();
        }
    }
}
