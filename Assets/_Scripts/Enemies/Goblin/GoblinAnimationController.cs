using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(GoblinStateInfo))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class GoblinAnimationController : MonoBehaviour
{
    public float m_animSpeedMultiplier = 1.0f, m_MoveSpeedMultiplier = 1.0f, m_runCycleLegOffset = 0.2f, m_stationaryTurnSpeed = 180.0f, m_movingTurnSpeed = 360.0f;

    private GoblinStateInfo m_goblinState;
    private Animator m_goblinAnimator;
    private Rigidbody m_goblinRigidBody;
    private AudioSource m_footStepsSoundEffectSource;

    //private Vector3 m_lastV;

    private float m_turn;

	// Use this for initialization
	void Start ()
    {
        m_goblinState = GetComponent<GoblinStateInfo>();

        m_goblinAnimator = GetComponent<Animator>();

        m_goblinRigidBody = GetComponent<Rigidbody>();

        m_footStepsSoundEffectSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator ()
    {
        m_turn = m_goblinState.m_turnTarAng - transform.rotation.eulerAngles.y;

        if (Mathf.Abs(m_turn) > 180.0f)
        {
            if (transform.rotation.eulerAngles.y < m_goblinState.m_turnTarAng)
            {
                m_turn = m_goblinState.m_turnTarAng - (transform.rotation.eulerAngles.y + 360.0f);
            }
            else
            {
                m_turn = (m_goblinState.m_turnTarAng + 360.0f) - transform.rotation.eulerAngles.y;
            }
        }
        m_turn /= 180.0f;

        RotateGoblin(m_turn);
                
        // update the animator parameters
        m_goblinAnimator.SetFloat("Forward", m_goblinState.m_forwardAmount, 0.1f, Time.deltaTime);
        m_goblinAnimator.SetFloat("Turn", m_turn, 0.1f, Time.deltaTime);
        m_goblinAnimator.SetFloat("Jump", m_goblinRigidBody.velocity.y);
        m_goblinAnimator.SetBool("OnGround", m_goblinState.m_grounded);
        
        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        float runCycle =
            Mathf.Repeat(
                m_goblinAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_runCycleLegOffset, 1);
        float jumpLeg = (runCycle < 0.5f ? 1 : -1) * m_goblinState.m_forwardAmount;
        if (m_goblinState.m_grounded)
        {
            m_goblinAnimator.SetFloat("JumpLeg", jumpLeg);
        }

        if ((runCycle <= 0.05f || (runCycle >= 0.475f && runCycle <= 0.525f)) && !m_footStepsSoundEffectSource.isPlaying && m_goblinState.m_grounded)
        {
            m_footStepsSoundEffectSource.pitch = Random.Range(0.9f, 1.1f);
            m_footStepsSoundEffectSource.PlayOneShot(m_footStepsSoundEffectSource.clip, Mathf.Lerp(0.0f, 1.0f, m_goblinRigidBody.velocity.magnitude));
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (m_goblinState.m_grounded && (m_goblinState.m_forwardAmount != 0 || m_goblinState.m_sidewaysAmount != 0))
        {
            m_goblinAnimator.speed = m_animSpeedMultiplier;
        }
        else
        {
            // don't use that while airborne or turning in place
            m_goblinAnimator.speed = 1;
        }
    }

    private void OnAnimatorMove()
    {
        if (m_goblinState.m_grounded && Time.deltaTime > 0)
        {
            Vector3 v = (m_goblinAnimator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
            
            if (!m_goblinState.m_gravLocked && !m_goblinState.m_swept)
            {
                //m_goblinRigidBody.velocity -= m_lastV;

                // we preserve the existing y part of the current velocity.
                v.y = m_goblinRigidBody.velocity.y;
                m_goblinRigidBody.velocity = v;

                //m_lastV = v;

                //m_goblinRigidBody.velocity += m_lastV;
            }
            else
            {
                // preserve player velocity
            }
        }
    }

    private void RotateGoblin (float ang)
    {
        float turnSpeed = Mathf.Lerp(m_stationaryTurnSpeed, m_movingTurnSpeed, m_goblinState.m_forwardAmount);
        
        transform.Rotate(0, ang * turnSpeed * Time.deltaTime, 0);
    }
}
