using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraOffsetSlider : MonoBehaviour
{
    public bool m_x = true, m_y = false, m_z = false;

    private Slider m_thisSlider;

    private OrbitCam m_orbCam;

	// Use this for initialization
	void Awake ()
    {
        m_thisSlider = GetComponent<Slider>();        

        m_orbCam = Camera.main.gameObject.GetComponent<OrbitCam>();

	    if (m_x)
        {           
            if (m_y || m_z)
            {
                m_y = false;
                m_z = false;
            }
            //Vector3 zoomTarShift = m_orbCam.m_zoomTarShift;

            //m_thisSlider.value = zoomTarShift.x;
            m_thisSlider.value = m_orbCam.m_zoomTarShift.x;
        }
        else if (m_y)
        {
            if (m_x || m_z)
            {
                m_x = false;
                m_z = false;
            }

            m_thisSlider.value = m_orbCam.m_zoomTarShift.y;
        }
        else if (m_z)
        {
            if (m_x || m_y)
            {
                m_x = false;
                m_y = false;
            }

            m_thisSlider.value = m_orbCam.m_zoomTarShift.z;
        }        
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void SetCameraOffset ()
    {
        if (m_x)
        {
            m_orbCam.m_zoomTarShift = new Vector3(m_thisSlider.value, m_orbCam.m_zoomTarShift.y, m_orbCam.m_zoomTarShift.z);
        }
        else if (m_y)
        {
            m_orbCam.m_zoomTarShift = new Vector3(m_orbCam.m_zoomTarShift.x, m_thisSlider.value, m_orbCam.m_zoomTarShift.z);
        }
        else if (m_z)
        {
            m_orbCam.m_zoomTarShift = new Vector3(m_orbCam.m_zoomTarShift.x, m_orbCam.m_zoomTarShift.y, m_thisSlider.value);
        }
    }
}
