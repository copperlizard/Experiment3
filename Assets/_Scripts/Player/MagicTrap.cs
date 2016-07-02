using UnityEngine;
using System.Collections;

public class MagicTrap : MonoBehaviour
{
    public GameObject m_trapEffect, m_triggeredEffect;

    public float m_triggerTime, m_trapTime;

    private bool m_triggered = false;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void OnEnable ()
    {
        m_triggered = false;

        m_triggeredEffect.SetActive(false);
        m_trapEffect.SetActive(true);

        StartCoroutine(TriggerTimer(m_triggerTime));
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.attachedRigidbody != null && !m_triggered)
        {
            TriggerTrap();
        }

    }

    private void TriggerTrap ()
    {
        m_triggered = true;

        m_trapEffect.SetActive(false);
        m_triggeredEffect.SetActive(true);

        StartCoroutine(Trap(m_trapTime));
    }

    IEnumerator Trap (float time)
    {
        float startTime = Time.time;
        while (Time.time < startTime + time)
        {
            RaycastHit[] hits = Physics.CapsuleCastAll(transform.position - transform.up * 0.25f, transform.position + transform.up * 2.0f, 1.0f, transform.up, 0.0f);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].rigidbody != null)
                {
                    float distMod = Mathf.Clamp(1.0f - ((hits[i].transform.position - transform.position).magnitude / 3.0f), 0.0f, 1.0f);

                    //hits[i].rigidbody.AddForce(transform.up * (20.0f + 20.0f * distMod) * Time.deltaTime, ForceMode.Impulse);
                    hits[i].rigidbody.AddExplosionForce(6000.0f * Time.deltaTime, transform.position, 2.0f, 1.0f, ForceMode.Impulse);

                    if (hits[i].transform.tag == "Goblin" && LayerMask.LayerToName(hits[i].transform.gameObject.layer) == "PlayerBubble")
                    {
                        GoblinStateInfo thisGoblin = hits[i].transform.GetComponentInParent<GoblinStateInfo>();

                        thisGoblin.m_health = Mathf.Clamp(thisGoblin.m_health - 1.0f * Time.deltaTime, 0.0f, 1.0f);
                    }
                }
            }

            yield return null;
        }

        m_triggeredEffect.SetActive(false);
        gameObject.SetActive(false);
    }

    IEnumerator TriggerTimer (float time)
    {
        yield return new WaitForSeconds(time);
        TriggerTrap();
    }
}
