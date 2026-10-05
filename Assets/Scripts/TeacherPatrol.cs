using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TeacherPatrol : MonoBehaviour
{
    private Vector3 lastPosition;
    public float animationSpeedThreshold = 0.1f;


    public Transform[] patrolPoints;

    public Animator animator;

    public float minWait = 1f;
    public float maxWait = 3f;

    private NavMeshAgent agent;
    private int currentPoint = -1;
    private bool waiting;
    private float waitTimer;

    private void Start()
    {

        lastPosition = transform.position;

        agent = GetComponent<NavMeshAgent>();

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("Öğretmen NavMesh üzerinde değil.", this);
            enabled = false;
            return;
        }

        ChooseNextPoint();
    }


    private void LateUpdate()
{
    Vector3 movement = transform.position - lastPosition;
    movement.y = 0f;

    float actualSpeed = Time.deltaTime > 0f
        ? movement.magnitude / Time.deltaTime
        : 0f;

    lastPosition = transform.position;

    if (animator != null)
    {
        bool isWalking = !waiting &&
                         agent.isOnNavMesh &&
                         !agent.isStopped &&
                         actualSpeed > animationSpeedThreshold;

        animator.SetBool("isWalking", isWalking);
    }
}

private void OnDisable()
{
    if (animator != null)
        animator.SetBool("isWalking", false);
}
    private void Update()
    {

        if (animator != null)
        {
            bool isWalking = agent.isOnNavMesh && agent.velocity.magnitude > 0.5f;
            animator.SetBool("isWalking", isWalking);
        }


        if (!agent.isOnNavMesh)
            return;

        if (waiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                waiting = false;
                ChooseNextPoint();
            }

            return;
        }

        if (agent.pathPending)
            return;

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning("Hedefe ulaşılabilecek yol yok.", this);
            enabled = false;
            return;
        }

        if (agent.remainingDistance <=
            Mathf.Max(agent.stoppingDistance, 0.15f))
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            waiting = true;
            waitTimer = Random.Range(minWait, maxWait);
        }
    }

    private void ChooseNextPoint()
    {
        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            Debug.LogWarning("En az 2 devriye noktası ekle.", this);
            enabled = false;
            return;
        }

        // Önceki hedefi tekrar seçmeden rastgele bir nokta seç.
        int nextPoint = Random.Range(0, patrolPoints.Length - 1);

        if (currentPoint >= 0 && nextPoint >= currentPoint)
            nextPoint++;

        // İlk seçimde bütün noktaları kullan.
        if (currentPoint < 0)
            nextPoint = Random.Range(0, patrolPoints.Length);

        currentPoint = nextPoint;
        agent.isStopped = false;

        Transform target = patrolPoints[currentPoint];

        if (target == null || !agent.SetDestination(target.position))
        {
            Debug.LogWarning("Devriye hedefini kontrol et.", this);
            enabled = false;
        }
    }
}