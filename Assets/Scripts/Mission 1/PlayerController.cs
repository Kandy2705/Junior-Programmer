using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Movement tuning (editable in Inspector)
    public float speed = 5.0f;
    public float turnSpeed = 100f;
    // Input System action exposed in the Inspector for binding (e.g., WASD or arrow keys)
    public InputAction moveAction;
    // Current input value (x=left/right, y=forward/backward), kept private for internal use
    private Vector2 moveInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        //Move the vehicle forward
        transform.Translate(Vector3.forward * Time.deltaTime * speed * moveInput.y);
        //Rotate the vehicle
        transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * moveInput.x);
    }
}
