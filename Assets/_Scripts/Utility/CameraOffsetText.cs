using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraOffsetText : MonoBehaviour
{
    private OrbitCam m_orbCam;

    private Text m_thisText;

	// Use this for initialization
	void Awake ()
    {
        m_thisText = GetComponent<Text>();

        m_orbCam = Camera.main.gameObject.GetComponent<OrbitCam>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_thisText.text = "AimCamOffset: " + m_orbCam.m_zoomTarShift.ToString();
	}
}
