using UnityEngine;
using UnityEngine.InputSystem;

namespace Mission3
{
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody playerRb;
        public InputAction jumpAction;

        public float jumpForce = 10.0f;
        public float gravityModifier = 2.0f;
        public bool isOnGround = true;

        public bool gameOver = false;
        private Animator playerAnim;

        public ParticleSystem explosionParticle;
        public ParticleSystem dirtParticle;
        public AudioClip jumpSound;
        public AudioClip crashSound;
        private AudioSource playerAudio;

        void Start()
        {
            playerRb = GetComponent<Rigidbody>();
            Physics.gravity = new Vector3(0, -9.81f * gravityModifier, 0);
            playerAnim = GetComponent<Animator>();
            playerAudio = GetComponent<AudioSource>();
        }

        void OnEnable()
        {
            jumpAction.Enable();
        }

        void OnDisable()
        {
            jumpAction.Disable();
        }

        void Update()
        {
            if (jumpAction.triggered && isOnGround && !gameOver)
            {
                playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isOnGround = false;
                playerAnim.SetTrigger("Jump_trig");
                dirtParticle.Stop();
                playerAudio.PlayOneShot(jumpSound, 1.0f);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                isOnGround = true;
                dirtParticle.Play();
            }
            else if (collision.gameObject.CompareTag("Obstacle"))
            {
                gameOver = true;
                Debug.Log("Game Over!");
                playerAnim.SetBool("Death_b", true);
                playerAnim.SetInteger("DeathType_int", 1);
                explosionParticle.Play();
                dirtParticle.Stop();
                playerAudio.PlayOneShot(crashSound, 1.0f);
            }
        }
    }
}
