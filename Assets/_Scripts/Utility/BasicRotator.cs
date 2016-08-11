using UnityEngine;
using System.Collections;

public class BasicRotator : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        transform.Rotate(1.5f, 5.5f, 4.5f);
	}
}
