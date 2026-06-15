using UnityEngine;
using UnityEngine.InputSystem;

namespace Mission3
{
    public class PlayerControllerX : MonoBehaviour
    {
        public bool gameOver;

        public float floatForce;
        private float gravityModifier = 1.5f;
        private Rigidbody playerRb;

        public InputAction floatAction;

        public ParticleSystem explosionParticle;
        public ParticleSystem fireworksParticle;

        private AudioSource playerAudio;
        public AudioClip moneySound;
        public AudioClip explodeSound;

        public float maxHeight = 16.0f;
        private bool isLowEnough;

        public float bounceForce = 10.0f;
        public AudioClip bounceSound;


        // Start is called before the first frame update
        void Start()
        {
            playerRb = GetComponent<Rigidbody>();
            Physics.gravity *= gravityModifier;
            playerAudio = GetComponent<AudioSource>();
            floatAction.Enable();

            // Apply a small upward force at the start of the game
            playerRb.AddForce(Vector3.up * 5, ForceMode.Impulse);

        }

        // Update is called once per frame
        void Update()
        {
            isLowEnough = transform.position.y < maxHeight;

            // While space is pressed and player is low enough, float up
            if (floatAction.IsPressed() && !gameOver && isLowEnough)
            {
                playerRb.AddForce(Vector3.up * floatForce);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            // if player collides with bomb, explode and set gameOver to true
            if (other.gameObject.CompareTag("Bomb"))
            {
                explosionParticle.Play();
                playerAudio.PlayOneShot(explodeSound, 1.0f);
                gameOver = true;
                Debug.Log("Game Over!");
                Destroy(other.gameObject);
            }

            // if player collides with money, fireworks
            else if (other.gameObject.CompareTag("Money"))
            {
                fireworksParticle.transform.position = transform.position;
                fireworksParticle.Play();
                playerAudio.PlayOneShot(moneySound, 1.0f);
                Destroy(other.gameObject);

            }

            if (other.gameObject.CompareTag("Ground"))
            {
                playerRb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
                playerAudio.PlayOneShot(bounceSound);
            }

        }

    }
}
