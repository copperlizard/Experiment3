using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [HideInInspector]
    static public ObjectPool m_thisPool;
    public GameObject m_pooledObject;
    public int m_poolStartSize = 10, m_grownPoolSize = 20, m_maxPoolSize = 1000;
    public bool m_grows = true;

    private List<GameObject> m_objects;
    private int m_lastGot;      

    void Awake()
    {
        m_thisPool = this;
    }

	// Use this for initialization
	void Start ()
    {
        m_objects = new List<GameObject>();

        for(int i = 0; i < m_poolStartSize; i++)
        {
            GameObject obj = (GameObject)Instantiate(m_pooledObject);
            obj.SetActive(false);
            m_objects.Add(obj);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public GameObject GetObject()
    {
        if (m_grows && m_objects.Count > m_grownPoolSize)
        {
            //Debug.Log("try to clean");
            CleanPool();
        }

        for (int i = 0; i < m_objects.Count; i++)
        {
            if(!m_objects[i].activeInHierarchy)
            {
                m_lastGot = i;
                return m_objects[i];
            }
        }

        if(m_grows && m_objects.Count <= m_maxPoolSize)
        {
            GameObject obj = (GameObject)Instantiate(m_pooledObject);
            m_objects.Add(obj);
            return obj;
        }
        else if(!m_grows)
        {
            if(m_lastGot == m_objects.Count - 1)
            {
                m_lastGot = 0;
                return m_objects[0];
            }
            else
            {
                m_lastGot++;
                return m_objects[m_lastGot];
            }
        }

        return null;
    }

    public void CleanPool()
    {
        if (m_objects.Count <= m_grownPoolSize)
        {
            //Debug.Log("no need to clean");
            return;
        }

        List<GameObject> objsToDestroy = new List<GameObject>();

        for (int i = 0; i < m_objects.Count; i++)
        {
            if (!m_objects[i].activeInHierarchy)
            {
                objsToDestroy.Add(m_objects[i]);
                m_objects.Remove(m_objects[i]);

                //Debug.Log("object removed from m_objects");
            }
        }

        foreach(GameObject obj in objsToDestroy)
        {
            Destroy(obj);

            //Debug.Log("Destoryed object");
        }        
    }
}
