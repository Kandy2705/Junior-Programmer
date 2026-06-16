using UnityEngine;
using System.Collections;

namespace Mission4
{
    public class PlayerController : MonoBehaviour
    {
        public float playerSpeed = 150.0f;

        private InputSystem_Actions controls;
        private Rigidbody playerRb;
        private GameObject focalPoint;

        public bool hasPowerUp = false;

        private float powerUpStrength = 15.0f;

        public GameObject powerupIndicator;

        private Vector2 moveInput;
        private Coroutine powerupCoroutine;

        private void Awake()
        {
            controls = new InputSystem_Actions();

            playerRb = GetComponent<Rigidbody>();
            focalPoint = GameObject.Find("Focal Point");
        }

        private void Start()
        {
            if (powerupIndicator != null)
            {
                powerupIndicator.SetActive(false);
            }
        }

        private void OnEnable()
        {
            controls.Player.Enable();
        }

        private void OnDisable()
        {
            controls.Player.Disable();
        }

        private void Update()
        {
            moveInput = controls.Player.Move.ReadValue<Vector2>();

            if (powerupIndicator != null)
            {
                powerupIndicator.transform.position = transform.position + new Vector3(0, -0.5f, 0);
            }
        }

        private void FixedUpdate()
        {
            if (focalPoint == null || playerRb == null) return;

            float forwardInput = moveInput.y;

            playerRb.AddForce(
                focalPoint.transform.forward * forwardInput * playerSpeed,
                ForceMode.Force
            );
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Powerup"))
            {
                hasPowerUp = true;
                Destroy(other.gameObject);

                if (powerupIndicator != null)
                {
                    powerupIndicator.SetActive(true);
                }

                if (powerupCoroutine != null)
                {
                    StopCoroutine(powerupCoroutine);
                }

                powerupCoroutine = StartCoroutine(PowerupCountdownRoutine());
            }
        }

        private IEnumerator PowerupCountdownRoutine()
        {
            yield return new WaitForSeconds(7);

            hasPowerUp = false;

            if (powerupIndicator != null)
            {
                powerupIndicator.SetActive(false);
            }

            powerupCoroutine = null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy") && hasPowerUp)
            {
                Rigidbody enemyRigidbody = collision.gameObject.GetComponent<Rigidbody>();

                if (enemyRigidbody == null) return;

                Vector3 awayFromPlayer = collision.gameObject.transform.position - transform.position;

                enemyRigidbody.AddForce(
                    awayFromPlayer.normalized * powerUpStrength,
                    ForceMode.Impulse
                );

                Debug.Log("Collided with: " + collision.gameObject.name + " with powerup set to " + hasPowerUp);
            }
        }
    }
}