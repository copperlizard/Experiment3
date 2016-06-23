using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ArrowsPanel : MonoBehaviour
{
    public GameObject m_player;

    public Image m_arrow1, m_arrow2, m_arrow3;

    private PlayerStateInfo m_playerState;

    // Use this for initialization
    void Start ()
    {
        if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        m_playerState = m_player.GetComponent<PlayerStateInfo>();

        switch (m_playerState.m_arrowMode)
        {
            case 0:
                m_arrow1.color = Color.white;
                m_arrow2.color = Color.gray;
                m_arrow3.color = Color.gray;
                break;
            case 1:
                m_arrow1.color = Color.gray;
                m_arrow2.color = Color.white;
                m_arrow3.color = Color.gray;
                break;
            case 2:
                m_arrow1.color = Color.gray;
                m_arrow2.color = Color.gray;
                m_arrow3.color = Color.white;
                break;
            default:
                m_arrow1.color = Color.gray;
                m_arrow2.color = Color.gray;
                m_arrow3.color = Color.gray;
                break;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        switch (m_playerState.m_arrowMode)
        {
            case 0:
                m_arrow1.color = Color.white;
                m_arrow2.color = Color.gray;
                m_arrow3.color = Color.gray;
                break;
            case 1:
                m_arrow1.color = Color.gray;
                m_arrow2.color = Color.white;
                m_arrow3.color = Color.gray;
                break;
            case 2:
                m_arrow1.color = Color.gray;
                m_arrow2.color = Color.gray;
                m_arrow3.color = Color.white;
                break;
            default:
                m_arrow1.color = Color.gray;
                m_arrow2.color = Color.gray;
                m_arrow3.color = Color.gray;
                break;
        }
    }
}
