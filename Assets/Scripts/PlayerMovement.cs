using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform cameraTransform;

    private CharacterController controller;
    private float verticalVelocity;
    private const float Gravity = -9.81f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            GameObject playerChair = GameObject.FindWithTag("PlayerChair");
            if (playerChair == null)
    {
        Debug.LogWarning("PlayerChair etiketli obje bulunamadı.");
        return;
    }
            this.gameObject.transform.position = playerChair.transform.position+ new Vector3(0, 1, 0);
            this.gameObject.transform.rotation = playerChair.transform.rotation;
        }


        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement = Vector3.ClampMagnitude(
            forward * vertical + right * horizontal, 1f
        );

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += Gravity * Time.deltaTime;

        Vector3 velocity = movement * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }
}