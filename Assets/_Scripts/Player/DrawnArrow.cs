using UnityEngine;
using System.Collections;

public class DrawnArrow : MonoBehaviour
{
    public Transform m_leftHandArrowTran, m_rightHandArrowTran;

    public GameObject m_player;

    private PlayerStateInfo m_playerState;

    private ParticleSystem[] m_arrows;

    private ParticleSystem.EmissionModule[] m_arrowEmitters;

    private ParticleSystem.MinMaxCurve[] m_arrowEmisionRates;

    private float m_emissisonFactor1, m_emissisonFactor2, m_emissisonFactor3;

    // Use this for initialization
    void Start ()
    {
        if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
        }

        m_playerState = m_player.GetComponent<PlayerStateInfo>();
        
        m_arrows = transform.GetChild(0).gameObject.GetComponentsInChildren<ParticleSystem>();

        m_arrowEmitters = new ParticleSystem.EmissionModule[m_arrows.Length];

        for (int i = 0; i < m_arrows.Length; i++)
        {
            m_arrowEmitters[i] = m_arrows[i].emission;
        }

        m_arrowEmisionRates = new ParticleSystem.MinMaxCurve[m_arrows.Length];

        for (int i = 0; i < m_arrows.Length; i++)
        {            
            m_arrowEmisionRates[i] = m_arrowEmitters[i].rate;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 arrowLine = m_leftHandArrowTran.position - m_rightHandArrowTran.position;

        transform.position = m_rightHandArrowTran.position + (arrowLine * 0.5f);
        transform.rotation = Quaternion.LookRotation(arrowLine);

        m_leftHandArrowTran.rotation = transform.rotation;
        m_rightHandArrowTran.rotation = transform.rotation;

        //Debug.DrawLine(transform.position, transform.position + transform.forward);
        //Debug.DrawLine(m_rightHandArrowTran.position, m_rightHandArrowTran.position + m_rightHandArrowTran.forward, Color.blue);

        switch(m_playerState.m_arrowMode)
        {
            case 0:
                m_emissisonFactor1 = Mathf.Lerp(m_emissisonFactor1, 1.0f, 0.3f);
                m_emissisonFactor2 = Mathf.Lerp(m_emissisonFactor2, 0.0f, 0.3f);
                m_emissisonFactor3 = Mathf.Lerp(m_emissisonFactor3, 0.0f, 0.3f);
                break;
            case 1:
                m_emissisonFactor1 = Mathf.Lerp(m_emissisonFactor1, 0.0f, 0.3f);
                m_emissisonFactor2 = Mathf.Lerp(m_emissisonFactor2, 1.0f, 0.3f);
                m_emissisonFactor3 = Mathf.Lerp(m_emissisonFactor3, 0.0f, 0.3f);
                break;
            case 2:
                m_emissisonFactor1 = Mathf.Lerp(m_emissisonFactor1, 0.0f, 0.3f);
                m_emissisonFactor2 = Mathf.Lerp(m_emissisonFactor2, 0.0f, 0.3f);
                m_emissisonFactor3 = Mathf.Lerp(m_emissisonFactor3, 1.0f, 0.3f);
                break;
            default:                
                break;
        }

        //Modify arrow emitters
        m_arrowEmisionRates[0].constantMax = 30.0f * m_emissisonFactor1;
        m_arrowEmisionRates[1].constantMax = 30.0f * m_emissisonFactor2;
        m_arrowEmisionRates[2].constantMax = 30.0f * m_emissisonFactor3;

        m_arrowEmitters[0].rate = m_arrowEmisionRates[0];
        m_arrowEmitters[1].rate = m_arrowEmisionRates[1];
        m_arrowEmitters[2].rate = m_arrowEmisionRates[2];
    }    
}
