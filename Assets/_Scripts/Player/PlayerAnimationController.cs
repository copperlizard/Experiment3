using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerStateInfo))]
[RequireComponent(typeof(AudioSource))]
public class PlayerAnimationController : MonoBehaviour
{
    public Camera m_cam;
    
    public List<ObjectPool> m_firedArrows = new List<ObjectPool>();
    public ObjectPool m_magicBolts;
    public ObjectPool m_magicTraps;

    public GameObject m_bow, m_magic, m_magicSweep, m_drawnArrow, m_drawingArrow;

    public ParticleSystem m_jumpEffect;

    public Transform m_leftHandDrawnArrowTransform;

    public Vector3 m_magic1AimLine = new Vector3(-.4f, 0.0f, 1.014f), m_magic2AimLine = new Vector3(-.2f, 0.0f, 1.014f), m_magic3AimLine = new Vector3(0.0f, 0.0f, 1.0f);

    public float m_jumpForce = 1.0f, m_jumpChargeRate = 0.1f, m_animSpeedMultiplier = 1.0f, m_MoveSpeedMultiplier = 1.0f, 
        m_crouchSpeedModifier = 1.0f, m_sprintSpeedModifier = 1.0f, m_runCycleLegOffset = 0.2f, m_stationaryTurnSpeed = 180.0f, 
        m_movingTurnSpeed = 360.0f, m_standingHeight = 2.0f, m_crouchHeight = 1.7f, m_bowAimSlerpRate = 0.5f;

    private OrbitCam m_orbitCam;
    private PlayerStateInfo m_playerState;

    private List<GoblinStateInfo> m_sweptGoblins = new List<GoblinStateInfo>();

    private Animator m_playerAnimator;
    private Transform m_spineTransform, m_leftShoulderTransform, m_rightShoulderTransform;

    private Rigidbody m_playerRigidBody;

    private CapsuleCollider m_playerCollider;

    private ParticleSystem.EmissionModule m_jumpEffectEmission;
    private ParticleSystem.MinMaxCurve m_jumpEffectEmissionRate;

    private ParticleSystem[] m_bowEffects;

    private ParticleSystem.EmissionModule[] m_bowEffectEmmisions;
    private ParticleSystem.MinMaxCurve m_bowEffectEmmisionRate;

    private ParticleSystem.ForceOverLifetimeModule[] m_bowEffectForces;
    private ParticleSystem.MinMaxCurve m_bowEffectForceCurve;

    private AudioSource m_footStepsSoundEffectSource, m_jumpSoundEffectSource, m_bowSoundEffectSource;

    private Quaternion m_lastBowAimDeltaRot;

    private Vector3 m_magicAimLine;

    private float m_turn, m_jumpCharge;

    private bool m_fireLock = false, m_jumpLock = false;

    // Use this for initialization
    void Start ()
    {
        if (m_cam == null)
        {
            m_cam = Camera.main;
        }
        m_orbitCam = m_cam.GetComponent<OrbitCam>();
        
        m_playerState = GetComponent<PlayerStateInfo>();

        m_playerAnimator = GetComponent<Animator>();
        m_spineTransform = m_playerAnimator.GetBoneTransform(HumanBodyBones.Spine);
        m_leftShoulderTransform = m_playerAnimator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        m_rightShoulderTransform = m_playerAnimator.GetBoneTransform(HumanBodyBones.RightShoulder);

        if (m_leftShoulderTransform == null)
        {
            Debug.Log("no left shoulder");
        }
        if (m_rightShoulderTransform == null)
        {
            Debug.Log("no right shoulder");
        }

        m_playerRigidBody = GetComponent<Rigidbody>();

        m_drawnArrow.SetActive(false);
        m_drawingArrow.SetActive(false);

        m_magicAimLine = m_magic1AimLine;

        m_playerCollider = GetComponent<CapsuleCollider>();

        m_jumpEffectEmission = m_jumpEffect.emission;
        m_jumpEffectEmissionRate = new ParticleSystem.MinMaxCurve(0.0f);
        m_jumpEffectEmission.rate = m_jumpEffectEmissionRate;

        m_jumpSoundEffectSource = m_jumpEffect.GetComponentInParent<AudioSource>();

        
        m_bowEffects = m_bow.GetComponentsInChildren<ParticleSystem>();

        m_bowEffectEmmisions = new ParticleSystem.EmissionModule[m_bowEffects.Length];

        for (int i = 0; i < m_bowEffects.Length; i++)
        {
            m_bowEffectEmmisions[i] = m_bowEffects[i].emission;
        }

        m_bowEffectForces = new ParticleSystem.ForceOverLifetimeModule[m_bowEffects.Length];

        for (int i = 0; i < m_bowEffects.Length; i++)
        {
            m_bowEffectForces[i] = m_bowEffects[i].forceOverLifetime;
        }

        m_jumpEffectEmissionRate = m_jumpEffectEmission.rate;

        m_bowSoundEffectSource = m_bow.GetComponent<AudioSource>();
        
        m_footStepsSoundEffectSource = GetComponent<AudioSource>();        
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateAnimator();

        //Dispose of unused arrows
        if ((m_drawnArrow.activeInHierarchy || m_drawingArrow.activeInHierarchy) && (!m_playerState.m_aiming || !m_playerState.m_armed))
        {
            ArrowCanceled();
        }

        //Manage "weapon" visibility
        if (m_playerState.m_armed)
        {
            if (!m_bow.activeInHierarchy)
            {
                m_bow.SetActive(true);
            }

            if (m_magic.activeInHierarchy)
            {
                m_magic.SetActive(false);
            }            
        }
        else if (!m_playerState.m_armed)
        {
            if (m_bow.activeInHierarchy)
            {
                m_bow.SetActive(false);
            }            

            if (m_playerState.m_aiming)
            {
                if (!m_magic.activeInHierarchy)
                {
                    m_magic.SetActive(true);
                }
            }
            else
            {
                if (m_magic.activeInHierarchy)
                {
                    m_magic.SetActive(false);
                }
            }
        }        
	}

    private void UpdateAnimator ()
    {
        m_turn = m_playerState.m_turnTarAng - transform.rotation.eulerAngles.y;

        if (Mathf.Abs(m_turn) > 180.0f)
        {
            if (transform.rotation.eulerAngles.y < m_playerState.m_turnTarAng)
            {
                m_turn = m_playerState.m_turnTarAng - (transform.rotation.eulerAngles.y + 360.0f);
            }
            else
            {
                m_turn = (m_playerState.m_turnTarAng + 360.0f) - transform.rotation.eulerAngles.y;
            }
        }
        m_turn /= 180.0f;

        if (!m_playerState.m_surfing)
        {
            RotatePlayer(m_turn);
        }        

        PlayerJump();

        //Adjust playerbubble collider height for crouching
        if (m_playerState.m_crouching && m_playerCollider.height > m_crouchHeight)
        {
            m_playerCollider.height = m_crouchHeight;
            m_playerCollider.center = new Vector3(0.0f, m_crouchHeight * 0.5f, 0.0f);
        }
        else if (!m_playerState.m_crouching && m_playerCollider.height < m_standingHeight)
        {
            m_playerCollider.height = m_standingHeight;
            m_playerCollider.center = new Vector3(0.0f, m_standingHeight * 0.5f, 0.0f);
        }

        // update the animator parameters
        m_playerAnimator.SetLayerWeight(1, (m_playerState.m_aiming && !m_playerState.m_sprinting) ? Mathf.Lerp(m_playerAnimator.GetLayerWeight(1), 1.0f, 0.1f) : Mathf.Lerp(m_playerAnimator.GetLayerWeight(1), 0.0f, 0.1f)); //set aiming layer weight
        m_playerAnimator.SetFloat("Forward", m_playerState.m_forwardAmount, 0.1f, Time.deltaTime);
        m_playerAnimator.SetFloat("Sideways", m_playerState.m_sidewaysAmount, 0.1f, Time.deltaTime);
        m_playerAnimator.SetFloat("Turn", m_turn, 0.1f, Time.deltaTime);
        m_playerAnimator.SetFloat("Jump", m_playerRigidBody.velocity.y);
        m_playerAnimator.SetFloat("JumpCharge", m_jumpCharge);
        m_playerAnimator.SetBool("OnGround", m_playerState.m_grounded);
        m_playerAnimator.SetBool("Aiming", m_playerState.m_aiming);
        m_playerAnimator.SetBool("Crouch", m_playerState.m_crouching);
        m_playerAnimator.SetBool("Armed", m_playerState.m_armed);
        m_playerAnimator.SetBool("Surfing", m_playerState.m_surfing);

        if (m_playerState.m_surfing)
        {
            m_playerAnimator.SetLayerWeight(0, Mathf.Lerp(m_playerAnimator.GetLayerWeight(0), 0.0f, 0.1f)); //override locomotion layer
            m_playerAnimator.SetLayerWeight(1, Mathf.Lerp(m_playerAnimator.GetLayerWeight(1), 0.0f, 0.1f)); //override aiming layer
            m_playerAnimator.SetLayerWeight(2, Mathf.Lerp(m_playerAnimator.GetLayerWeight(2), 1.0f, 0.1f));
        }
        else
        {
            m_playerAnimator.SetLayerWeight(0, Mathf.Lerp(m_playerAnimator.GetLayerWeight(0), 1.0f, 0.1f)); //restore locomotion layer
            
            m_playerAnimator.SetLayerWeight(2, Mathf.Lerp(m_playerAnimator.GetLayerWeight(2), 0.0f, 0.1f));
        }

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        float runCycle =
            Mathf.Repeat(
                m_playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_runCycleLegOffset, 1);
        float jumpLeg = (runCycle < 0.5f ? 1 : -1) * m_playerState.m_forwardAmount;
        if (m_playerState.m_grounded)
        {
            m_playerAnimator.SetFloat("JumpLeg", jumpLeg);
        }
        
        //Foot sounds
        if ((runCycle <= 0.05f || (runCycle >= 0.475f && runCycle <= 0.525f)) && !m_footStepsSoundEffectSource.isPlaying && m_playerState.m_grounded)
        {
            m_footStepsSoundEffectSource.pitch = Random.Range(0.9f, 1.1f);
            m_footStepsSoundEffectSource.PlayOneShot(m_footStepsSoundEffectSource.clip, Mathf.Lerp(0.0f, 1.0f, m_playerRigidBody.velocity.magnitude));
        }        

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (m_playerState.m_grounded && (m_playerState.m_forwardAmount != 0 || m_playerState.m_sidewaysAmount != 0))
        {
            m_playerAnimator.speed = m_animSpeedMultiplier;
        }
        else
        {
            // don't use that while airborne or turning in place
            m_playerAnimator.speed = 1;
        }

        //Manual state changes...

        AnimatorStateInfo torsoStateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);
        
        if (m_playerState.m_armed && m_playerState.m_firing && !m_fireLock && torsoStateInfo.IsName("ArmedDraw") && torsoStateInfo.normalizedTime >= 1)
        {
            //Debug.Log("charging/firing arrow!");

            m_fireLock = true;
            StartCoroutine(ChargeArrow());
        }

        if (!m_playerState.m_armed && m_playerState.m_aiming && m_playerState.m_firing && !m_fireLock && torsoStateInfo.IsName("Unarmed"))
        {
            //Debug.Log("casting magic!");

            m_fireLock = true;
            StartCoroutine(CastMagic());
        }
    }

    IEnumerator ChargeArrow ()
    {        
        m_playerAnimator.Play("ArmedCharging", 1);

        AnimatorStateInfo stateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);

        while (!stateInfo.IsName("ArmedCharging"))
        {
            stateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);

            //Debug.Log("waiting for charge animation");

            yield return null;
        }
                
        while (stateInfo.IsName("ArmedCharging"))
        {   
            stateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);

            m_playerState.m_arrowCharge = (stateInfo.normalizedTime > 1.0f) ? 1.0f : stateInfo.normalizedTime;

            //Add bow effects            
            m_bowSoundEffectSource.pitch = 0.5f + 0.15f * m_playerState.m_arrowCharge;
            m_bowEffectEmmisionRate.constantMax = 20.0f + (80.0f * m_playerState.m_arrowCharge);
            m_bowEffectForceCurve.constantMax = 2.0f + 3.0f * m_playerState.m_arrowCharge;
            
            for (int i = 0; i < m_bowEffectEmmisions.Length; i++)
            {
                m_bowEffectEmmisions[i].rate = m_bowEffectEmmisionRate;
                m_bowEffectForces[i].y = m_bowEffectForceCurve;                  
            }
            
            if (!m_playerState.m_firing)
            {
                break;
            }

            yield return null;
        }        

        while (m_playerState.m_firing && m_playerState.m_aiming)
        {
            //Debug.Log("full charge");

            yield return null;
        }
        
        //Reset bow effects
        m_bowSoundEffectSource.pitch = 0.5f;
        m_bowEffectEmmisionRate.constantMax = 20.0f;
        m_bowEffectForceCurve.constantMax = 2.0f;

        for (int i = 0; i < m_bowEffectEmmisions.Length; i++)
        {
            m_bowEffectEmmisions[i].rate = m_bowEffectEmmisionRate;
            m_bowEffectForces[i].y = m_bowEffectForceCurve;
        }
        
        if (m_playerState.m_aiming)
        {
            m_playerAnimator.Play("ArmedFire", 1);
        }               
        
        m_fireLock = false;
    }

    IEnumerator CastMagic ()
    {
        switch(m_playerState.m_magicMode)
        {
            case 0:
                m_playerAnimator.Play("MagicAttack1", 1);

                AnimatorStateInfo stateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);

                while (!stateInfo.IsName("MagicAttack1Hold"))
                {
                    stateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);             
                    yield return null;
                }

                while (stateInfo.IsName("MagicAttack1Hold"))
                {
                    stateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);
                                        
                    if (!m_playerState.m_firing || !m_playerState.m_aiming || m_playerState.m_magicMode != 0)
                    {                                                
                        break;
                    }
                    //SHOOTS BOLTS WITH ANIMATION EVENT/S!
                    yield return null;
                }

                m_playerAnimator.Play("MagicAttack1Release", 1);

                //Debug.Log("releasing hold!");

                break;
            case 1:
                m_playerAnimator.Play("MagicAttack2", 1);
                break;
            case 2:
                m_playerAnimator.Play("MagicAttack3", 1);
                break;
        }

        m_fireLock = false;
        yield return null;
    }

    //Animation event
    void FireMagicBolt ()
    {
        GameObject bolt = m_magicBolts.GetObject();

        Rigidbody boltRB = bolt.GetComponent<Rigidbody>();

        Vector3 toTar = (m_orbitCam.m_hitCurrent) ? m_orbitCam.m_hit.point - m_magic.transform.position : (m_orbitCam.transform.position + m_orbitCam.transform.forward * 10.0f) - m_magic.transform.position;

        bolt.transform.position = m_magic.transform.position;
        bolt.transform.rotation = Quaternion.LookRotation(toTar);

        bolt.SetActive(true);
        boltRB.velocity = toTar.normalized * 10.0f;
    }

    void FireMagicSweep ()
    {
        StartCoroutine(MagicSweep());
    }

    IEnumerator MagicSweep ()
    {
        m_magicSweep.SetActive(true);

        AnimatorStateInfo state = m_playerAnimator.GetCurrentAnimatorStateInfo(1);

        while(state.IsName("MagicAttack2") && state.normalizedTime < 0.55f && m_playerState.m_aiming)
        {
            state = m_playerAnimator.GetCurrentAnimatorStateInfo(1);

            RaycastHit[] hits = Physics.CapsuleCastAll(m_magicSweep.transform.position, m_magicSweep.transform.position + m_magicSweep.transform.forward * 6.0f, 0.5f, m_magicSweep.transform.forward, 0.0f, ~LayerMask.GetMask("Player", "Arrows"));
            
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].rigidbody != null && hits[i].transform.tag != "Player")
                {
                    //Gobin swept
                    if (hits[i].transform.tag == "Goblin" && LayerMask.LayerToName(hits[i].transform.gameObject.layer) == "PlayerBubble")
                    {
                        //Debug.Log("Goblin hit " + hits[i].transform.name);
                                     
                        GoblinStateInfo thisGoblin = hits[i].transform.GetComponent<GoblinStateInfo>();
                        Rigidbody thisRigidbody = hits[i].transform.GetComponent<Rigidbody>();

                        if (!thisGoblin.m_swept)
                        {
                            //Debug.Log("new goblin hit!");

                            thisGoblin.m_swept = true;
                            m_sweptGoblins.Add(thisGoblin);
                        }

                        if (thisRigidbody != null)
                        {
                            thisRigidbody.AddExplosionForce(6000.0f * Time.deltaTime, m_magicSweep.transform.position, 6.5f, 0.8f, ForceMode.Impulse);                            
                        }                        
                        
                        thisGoblin.m_health = Mathf.Clamp(thisGoblin.m_health - 1.2f * Time.deltaTime, 0.0f, 1.0f);                   
                    }
                    else
                    {
                        //Debug.Log("Not Goblin hit " + hits[i].transform.name);

                        hits[i].rigidbody.AddExplosionForce(1500.0f * Time.deltaTime, m_magicSweep.transform.position, 6.5f, 1.0f, ForceMode.Impulse);
                    }                 
                }
            }           

            yield return null;
        }

        //Free goblins
        if (m_sweptGoblins.Count > 0)
        {
            for (int i = 0; i < m_sweptGoblins.Count; i++)
            {
                m_sweptGoblins[i].m_swept = false;
            }
        }
        m_sweptGoblins.Clear();

        m_magicSweep.SetActive(false);

        //Debug.Log("done sweeping!");        
    }

    void FireMagicTrap ()
    {
        //Debug.Log("firing trap!");

        GameObject trap = m_magicTraps.GetObject();

        Vector3 toTar = m_orbitCam.m_hit.point - transform.position;

        //Debug.Log("toTar.magnitude == " + toTar.magnitude.ToString());

        //Debug.Log("hit current == " + m_orbitCam.m_hitCurrent.ToString());
             
        if (toTar.magnitude > 20.0f || !m_orbitCam.m_hitCurrent)
        {
            //Debug.Log("dropping trap!");

            RaycastHit hit;
            if (Physics.Raycast(m_orbitCam.transform.position + m_orbitCam.transform.forward * 20.0f, -transform.up, out hit))
            {
                trap.transform.position = hit.point;
                trap.transform.up = hit.normal;                
            }                        
        }
        else
        {
            //Debug.Log("placing trap!");

            trap.transform.position = m_orbitCam.m_hit.point;
            trap.transform.up = m_orbitCam.m_hit.normal;
        }

        trap.SetActive(true);

        //Debug.DrawLine(transform.position, transform.position + toTar, Color.blue, 0.5f);
        //Debug.DrawLine(transform.position, trap.transform.position, Color.red, 0.5f);        
    }

    private void OnAnimatorMove ()
    {
        //Maybe not neccessary
        if (m_playerState.m_surfing)
        {
            return;
        }

        if (m_playerState.m_grounded && Time.deltaTime > 0)
        {
            Vector3 v = (m_playerAnimator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

            //Debug.Log("v == " + v.ToString());

            // Apply move speed modifier
            if (m_playerState.m_crouching)
            {
                v *= m_crouchSpeedModifier;
            }
            else if (m_playerState.m_sprinting)
            {
                v *= m_sprintSpeedModifier;
            }

            if (!m_playerState.m_gravLocked)
            {
                // we preserve the existing y part of the current velocity.
                v.y = m_playerRigidBody.velocity.y;
                m_playerRigidBody.velocity = v;
            }
            else
            {
                // preserve player velocity                
            }
        }
    }

    private void OnAnimatorIK (int layer)
    {
        //Update magic aim line
        switch (m_playerState.m_magicMode)
        {
            case 0:
                m_magicAimLine = Vector3.Lerp(m_magicAimLine, m_magic1AimLine, 0.1f);
                break;
            case 1:
                m_magicAimLine = Vector3.Lerp(m_magicAimLine, m_magic2AimLine, 0.1f);
                break;
            case 2:
                break;
            default:
                break;         
        }

        //Bow aim
        if (m_playerState.m_aiming && m_playerState.m_armed)
        {
            Vector3 tar = m_orbitCam.m_hit.point;
            Vector3 toTar = tar - m_rightShoulderTransform.position;

            if (Vector3.Dot(transform.forward, toTar) < 0.0f)
            {
                //Target out of player COV (wait for rotate)
                return;
            }

            //Debug.DrawLine(m_rightShoulderTransform.position, m_rightShoulderTransform.position + toTar, Color.red);
            //Debug.DrawLine(m_cam.transform.position, m_cam.transform.position + m_cam.transform.forward * 10.0f, Color.yellow);

            toTar = m_spineTransform.InverseTransformDirection(toTar);
            
            Quaternion deltaRot = Quaternion.FromToRotation(m_spineTransform.InverseTransformDirection(m_leftHandDrawnArrowTransform.forward), toTar);            

            AnimatorStateInfo stateInfo = m_playerAnimator.GetCurrentAnimatorStateInfo(1);

            if ((stateInfo.IsName("ArmedDraw") && stateInfo.normalizedTime < 1.0f) || stateInfo.IsName("ArmedFire"))
            {
                Vector3 shoulders = m_leftShoulderTransform.position - m_rightShoulderTransform.position;
                shoulders = m_spineTransform.InverseTransformDirection(shoulders);
                deltaRot = Quaternion.FromToRotation(shoulders, toTar) * Quaternion.Euler(0.0f, 3.0f, 11.0f);
            }

            m_lastBowAimDeltaRot = Quaternion.Slerp(m_lastBowAimDeltaRot, deltaRot, m_bowAimSlerpRate);

            //m_spineTransform.localRotation *= deltaRot;
            m_spineTransform.localRotation *= m_lastBowAimDeltaRot;

            m_playerAnimator.SetBoneLocalRotation(HumanBodyBones.Spine, m_spineTransform.localRotation);
        }
        //Magic aim
        else if (m_playerState.m_aiming) //maybe not when sprinting???
        {
            m_playerAnimator.SetLookAtPosition(m_orbitCam.m_hit.point);
            m_playerAnimator.SetLookAtWeight(1.0f);
            
            Vector3 m_aimPos = m_rightShoulderTransform.position + ((m_rightShoulderTransform.position - m_leftShoulderTransform.position) * 0.5f);

            Vector3 tar = m_orbitCam.m_hit.point;
            Vector3 toTar = tar - m_aimPos;

            if (Vector3.Dot(transform.forward, toTar) < 0.0f)
            {
                //Target out of player COV (wait for rotate)                
                return;
            }

            toTar = m_spineTransform.InverseTransformDirection(toTar);

            Quaternion deltaRot = Quaternion.FromToRotation(m_magicAimLine, toTar);
            
            m_spineTransform.localRotation *= deltaRot;

            m_playerAnimator.SetBoneLocalRotation(HumanBodyBones.Spine, m_spineTransform.localRotation);
        }
        //Not aiming
        else
        {
            //m_playerAnimator.SetLookAtWeight(0.0f);

            Vector3 toTar = m_orbitCam.m_hit.point - transform.position;

            m_playerAnimator.SetLookAtWeight(Mathf.SmoothStep(0.5f, 1.0f, Mathf.Clamp(Vector3.Dot(toTar, transform.forward), 0.0f, 1.0f)));
            m_playerAnimator.SetLookAtPosition(m_orbitCam.m_hit.point);            
        }
    }

    void RotatePlayer (float ang)
    {
        float turnSpeed = Mathf.Lerp(m_stationaryTurnSpeed, m_movingTurnSpeed, m_playerState.m_forwardAmount);

        if (m_playerState.m_aiming)
        {
            turnSpeed *= 4.0f;
            m_turn *= 4.0f;
        }

        transform.Rotate(0, ang * turnSpeed * Time.deltaTime, 0);
    }

    void PlayerJump()
    {
        if (m_playerState.m_jumping && !m_jumpLock)
        {
            m_jumpLock = true;
            StartCoroutine(ChargeJump());
        }        
    }

    IEnumerator ChargeJump ()
    {
        m_jumpEffect.Play();

        while(m_playerState.m_jumping)
        {
            m_jumpCharge = Mathf.SmoothStep(m_jumpCharge, 1.0f, m_jumpChargeRate);
            m_jumpSoundEffectSource.pitch = m_jumpCharge;

            m_jumpEffectEmissionRate.constantMax = 50.0f * m_jumpCharge;
            m_jumpEffectEmission.rate = m_jumpEffectEmissionRate;

            //Debug.Log("m_jumpCharge == " + m_jumpCharge.ToString());

            yield return null;
        }

        if (m_playerState.m_grounded)
        {
            m_playerState.m_grounded = false;
            m_playerState.m_jumping = false;
            m_playerRigidBody.AddForce(Vector3.up * (m_jumpForce * Mathf.Clamp(m_jumpCharge, 0.2f, 1.0f)), ForceMode.VelocityChange);            
        }

        m_jumpCharge = 0.0f;
        m_jumpSoundEffectSource.pitch = m_jumpCharge;
        m_jumpLock = false;        
        m_jumpEffectEmissionRate.constantMax = 0.0f;
        m_jumpEffectEmission.rate = m_jumpEffectEmissionRate;
        m_jumpEffect.Stop();

        yield return null;
    }

    void DrawingArrow ()
    {
        m_drawingArrow.SetActive(true);
    }

    void ArrowDrawn ()
    {
        m_drawingArrow.SetActive(false);
        m_drawnArrow.SetActive(true);
    }

    void ArrowFired ()
    {
        m_drawnArrow.SetActive(false);

        GameObject arrow = m_firedArrows[m_playerState.m_arrowMode].GetObject();
        Rigidbody arrowRB = arrow.GetComponent<Rigidbody>();

        if (arrow.activeInHierarchy)
        {
            arrow.SetActive(false);
        }

        arrow.transform.position = m_drawnArrow.transform.position;
        arrow.transform.rotation = m_drawnArrow.transform.rotation;

        arrow.SetActive(true);
                
        //arrowRB.velocity = arrow.transform.forward * (30.0f + 30.0f * m_playerState.m_arrowCharge);
        arrowRB.velocity = (m_orbitCam.m_hit.point - arrow.transform.position).normalized * (30.0f + 60.0f * m_playerState.m_arrowCharge);

        m_playerState.m_arrowCharge = 0.0f;
    }

    void ArrowCanceled ()
    {
        //Debug.Log("arrow canceled!");

        if (m_drawnArrow.activeInHierarchy)
        {
            m_drawnArrow.SetActive(false);
        }

        if (m_drawingArrow.activeInHierarchy)
        {
            m_drawingArrow.SetActive(false);
        }       
    }
}
