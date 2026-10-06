using UnityEngine;

public class SeatedLean : MonoBehaviour
{
    public Transform viewRoot;

    public Transform cameraTransform;

    [Header("Uzanma sınırları")]
    public float sideLimit = 0.3f;
    public float forwardLimit = 0.35f;
    public float backwardLimit = 0.1f;

    public float leanSpeed = 1f;

    private Vector3 initialPosition;

    public bool IsLeaning =>
    viewRoot != null &&
    Vector3.Distance(viewRoot.localPosition, initialPosition) > 0.05f;

    private void Start()
    {
        if (viewRoot == null)
        {
            Debug.LogError("View Root alanını bağla.", this);
            enabled = false;
            return;
        }

        initialPosition = viewRoot.localPosition;
    }

   private void Update()
{
    float horizontal = 0f;
    float vertical = 0f;

    if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
    if (Input.GetKey(KeyCode.D)) horizontal += 1f;
    if (Input.GetKey(KeyCode.W)) vertical += 1f;
    if (Input.GetKey(KeyCode.S)) vertical -= 1f;

    Vector3 forward = Vector3.ProjectOnPlane(
        cameraTransform.forward, Vector3.up
    );

    if (forward.sqrMagnitude < 0.001f)
        forward = Vector3.Cross(cameraTransform.right, Vector3.up);

    forward.Normalize();

    Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

    // Çapraz uzanmanın daha hızlı/uzak olmasını engelle.
    Vector2 input = Vector2.ClampMagnitude(
        new Vector2(horizontal, vertical), 1f
    );

    float distance = input.y < 0f
        ? backwardLimit
        : forwardLimit;

    Vector3 worldOffset =
        right * input.x * sideLimit +
        forward * input.y * distance;

    Vector3 localOffset = viewRoot.parent != null
        ? viewRoot.parent.InverseTransformVector(worldOffset)
        : worldOffset;

    viewRoot.localPosition = Vector3.MoveTowards(
        viewRoot.localPosition,
        initialPosition + localOffset,
        leanSpeed * Time.deltaTime
    );
}


}