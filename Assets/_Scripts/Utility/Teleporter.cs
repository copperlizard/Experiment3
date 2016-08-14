using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    public int m_destSceneIndex;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Player")
        {
            DataManager.m_this.SavePlayerState();
            SceneManager.LoadScene(m_destSceneIndex);
        }
    }
}
