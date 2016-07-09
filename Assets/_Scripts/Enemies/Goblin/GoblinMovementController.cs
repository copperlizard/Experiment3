using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GoblinStateInfo))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class GoblinMovementController : MonoBehaviour
{
    public TerrainManager m_terrainManager;
    
    [System.Serializable]
    public struct SFX
    {
        [HideInInspector]
        public AudioSource m_footSoundEffectsSource;
        public AudioClip m_miscSurface, m_grassSurface, m_mudSurface, m_rockSurface;
    }
    public SFX m_footSoundEffects;

    public float m_groundCheckDist = 0.2f;

    private GoblinStateInfo m_goblinState;

    private Rigidbody m_goblinRigidbody;

    private RaycastHit m_groundHit;

    private Vector3 m_groundNormal;

    private float m_sprintInputModifier = 2.0f;

    // Use this for initialization
    void Start ()
    {
        m_goblinState = GetComponent<GoblinStateInfo>();
        m_goblinRigidbody = GetComponent<Rigidbody>();

        if (m_terrainManager == null)
        {
            m_terrainManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TerrainManager>();
        }
        
        m_footSoundEffects.m_footSoundEffectsSource = GetComponent<AudioSource>();        
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void FixedUpdate ()
    {
        m_goblinState.m_grounded = CheckGround() ^ (m_goblinState.m_swept || m_goblinState.m_gravLocked);

        if (m_goblinState.m_grounded && !m_goblinState.m_swept && !m_goblinState.m_gravLocked)
        {
            if (m_groundHit.collider.gameObject.name == "Terrain")
            {
                int surface = m_terrainManager.GetActiveTerrainTextureId(m_groundHit.point);

                switch (surface)
                {
                    case 0:
                        m_footSoundEffects.m_footSoundEffectsSource.clip = m_footSoundEffects.m_mudSurface;
                        break;
                    case 1:
                        m_footSoundEffects.m_footSoundEffectsSource.clip = m_footSoundEffects.m_rockSurface;
                        break;
                    case 3:
                        m_footSoundEffects.m_footSoundEffectsSource.clip = m_footSoundEffects.m_grassSurface;
                        break;
                    default:
                        m_footSoundEffects.m_footSoundEffectsSource.clip = m_footSoundEffects.m_miscSurface;
                        break;
                }
            }
            else
            {
                m_footSoundEffects.m_footSoundEffectsSource.clip = m_footSoundEffects.m_miscSurface;
            }
        }
    }

    public void Move(float v, float h)
    {
        Vector3 move = new Vector3(h, 0.0f, v).normalized;

        Debug.DrawLine(transform.position, transform.position + move, Color.yellow);

        // Apply move speed modifier
        if (m_goblinState.m_sprinting)
        {
            move *= m_sprintInputModifier;
        }        

        if (m_goblinState.m_grounded)
        {
            NormalMove(move);
        }
        else
        {
            AirMove(move);
        }

    }

    private bool CheckGround ()
    {        
        if (Physics.Raycast(transform.position + (Vector3.up * m_groundCheckDist * 0.5f), Vector3.down, out m_groundHit, m_groundCheckDist, ~LayerMask.GetMask("Goblin", "PlayerBubble")))
        {
            m_groundNormal = m_groundHit.normal;
            return true;
        }

        return false;
    }

    private void NormalMove (Vector3 move)
    {
        if (move.magnitude > 0.0f)
        {
            // Look "forward"
            Quaternion lookRot = Quaternion.LookRotation(move);
            m_goblinState.m_turnTarAng = lookRot.eulerAngles.y;

            // Account for "hills"
            move = Vector3.ProjectOnPlane(move, m_groundNormal);
        }
        else
        {
            //Stop rotating
            m_goblinState.m_turnTarAng = transform.rotation.eulerAngles.y;
        }

        m_goblinState.m_forwardAmount = move.magnitude;
        m_goblinState.m_sidewaysAmount = 0.0f;
    }

    private void AirMove (Vector3 move)
    {
        //m_goblinRigidbody.AddForce(move * 100.0f);
    }
}
