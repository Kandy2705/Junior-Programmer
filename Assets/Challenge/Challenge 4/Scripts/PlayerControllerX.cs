using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mission4
{
    public class PlayerControllerX : MonoBehaviour
    {
        private Rigidbody playerRb;
        private float speed = 500;
        private GameObject focalPoint;

        public bool hasPowerup;
        public GameObject powerupIndicator;
        public int powerUpDuration = 5;

        public float turboBoost = 20f;
        public ParticleSystem smokeParticle;

        private float normalStrength = 10; // how hard to hit enemy without powerup
        private float powerupStrength = 25; // how hard to hit enemy with powerup

        private InputSystem_Actions controls;

        void Awake()
        {
            controls = new InputSystem_Actions();
        }

        void OnEnable()
        {
            controls.Player.Enable();
        }

        void Start()
        {
            playerRb = GetComponent<Rigidbody>();
            focalPoint = GameObject.Find("Focal Point");
        }

        void Update()
        {
            // Add force to player in direction of the focal point (and camera)
            float verticalInput = controls.Player.Move.ReadValue<Vector2>().y;
            playerRb.AddForce(focalPoint.transform.forward * verticalInput * speed * Time.deltaTime);

            // Set powerup indicator position to beneath player
            powerupIndicator.transform.position = transform.position + new Vector3(0, -0.6f, 0);

            // Turbo boost when pressing Space
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                playerRb.AddForce(focalPoint.transform.forward * turboBoost, ForceMode.Impulse);

                if (smokeParticle != null)
                {
                    smokeParticle.Play();
                }
            }

        }

        // If Player collides with powerup, activate powerup
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Powerup"))
            {
                hasPowerup = true;
                powerupIndicator.SetActive(true);

                Destroy(other.gameObject);

                StartCoroutine(PowerupCooldown());
            }
        }

        // Coroutine to count down powerup duration
        IEnumerator PowerupCooldown()
        {
            yield return new WaitForSeconds(powerUpDuration);
            hasPowerup = false;
            powerupIndicator.SetActive(false);
        }

        // If Player collides with enemy
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Enemy") && hasPowerup)
            {
                Rigidbody enemyRigidbody = other.gameObject.GetComponent<Rigidbody>();
                Vector3 awayFromPlayer = other.gameObject.transform.position - transform.position;
                enemyRigidbody.AddForce(awayFromPlayer * powerupStrength, ForceMode.Impulse);
            }
        }
    }
}