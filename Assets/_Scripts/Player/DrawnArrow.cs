using UnityEngine;
using System.Collections;

public class DrawnArrow : MonoBehaviour
{
    public Transform m_leftHandArrowTran, m_rightHandArrowTran;

	// Use this for initialization
	void Start ()
    {

	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 arrowLine = m_leftHandArrowTran.position - m_rightHandArrowTran.position;

        transform.position = m_rightHandArrowTran.position + (arrowLine * 0.5f);
        transform.rotation = Quaternion.LookRotation(arrowLine);

        m_leftHandArrowTran.rotation = transform.rotation;
        m_rightHandArrowTran.rotation = transform.rotation;

        Debug.DrawLine(transform.position, transform.position + transform.forward);

        Debug.DrawLine(m_rightHandArrowTran.position, m_rightHandArrowTran.position + m_rightHandArrowTran.forward, Color.blue);
	}    
}
