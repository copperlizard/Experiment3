using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GoblinHealthDisplay : MonoBehaviour
{
    public Camera m_cam;

    public Slider m_healthSlider;

    private GoblinStateInfo m_goblinState;

	// Use this for initialization
	void Start ()
    {
	    if (m_cam == null)
        {
            m_cam = Camera.main;
        }

        m_goblinState = GetComponentInParent<GoblinStateInfo>();

        m_healthSlider.gameObject.SetActive(false);      
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_goblinState.m_health < 1.0f)
        {
            if (!m_healthSlider.gameObject.activeInHierarchy)
            {
                m_healthSlider.gameObject.SetActive(true);
            }

            m_healthSlider.value = m_goblinState.m_health;
            transform.rotation = Quaternion.LookRotation((m_cam.transform.position - transform.position).normalized);
        }        
	}
}
