using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class TeacherDetection : MonoBehaviour
{
    [Header("Referanslar")]
    public SeatedLean playerLean;
    public TeacherPatrol teacherPatrol;

    public float approachSpeed = 4f;

    public Animator animator;

    [Header("Oyuncuya yaklaşma")]
public Transform playerStandPoint;

public float caughtAnimationDuration = 3f;
public float turnSpeed = 180f;
public float approachTimeout = 20f;

private NavMeshAgent agent;
private bool approachingPlayer;

    

    public Transform eyes;
    public Transform playerHead;

    [Header("Görüş")]
    public float viewDistance = 8f;

    [Range(0f, 360f)]
    public float viewAngle = 100f;

    // Sadece görüşü engelleyen nesnelerin layer'ları.
    public LayerMask obstacleMask;

    public float detectionTime = 0.4f;

    [Header("Ekran")]
    public TMP_Text warningText;
    public GameObject gameOverPanel;

    [Header("Oyun bitince kapatılacak scriptler")]
    public Behaviour[] controlsToDisable;

    private int warningCount;
    private float seenTimer;
    private float warningTimer;
    private bool caughtThisLean;
    private bool gameOver;

    public HitScreenEffect hitScreenEffect;

// Animasyon Event'i bu metodu çağıracak.
public void OnTeacherHit()
{
    if (hitScreenEffect != null)
        hitScreenEffect.Flash();
}

    private void Start()
    {

        agent = GetComponent<NavMeshAgent>();

if (agent == null || teacherPatrol == null ||
    animator == null || playerStandPoint == null)
{
    Debug.LogError("Agent, Patrol, Animator ve durma noktasını bağla.", this);
    enabled = false;
    return;
}

        if (playerLean == null || eyes == null || playerHead == null)
        {
            Debug.LogError("Hocanın görüş referanslarını bağla.", this);
            enabled = false;
            return;
        }

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
{
    if (gameOver && Input.GetKeyDown(KeyCode.R))
    {
        Time.timeScale = 1f;

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager
                .GetActiveScene().buildIndex
        );
    }
}


    private void LateUpdate()
    {
        if (gameOver)
            return;

        UpdateWarning();


        if (approachingPlayer)
        {
            return;
        }

        // Merkeze dönünce yeni bir yakalanma mümkün olur.
        if (!playerLean.IsLeaning)
        {
            caughtThisLean = false;
            seenTimer = 0f;
            return;
        }

        if (caughtThisLean)
            return;

        // Öğrenciyle ilgilenirken oyuncuyu kontrol etmez.
        if (teacherPatrol != null && teacherPatrol.IsDistracted)
        {
            seenTimer = 0f;
            return;
        }

        if (!CanSeePlayer())
        {
            seenTimer = 0f;
            return;
        }

        seenTimer += Time.deltaTime;

        if (seenTimer >= detectionTime)
            CatchPlayer();
    }

    private bool CanSeePlayer()
    {
        Vector3 direction = playerHead.position - eyes.position;
        float distance = direction.magnitude;

        if (distance > viewDistance)
            return false;

        if (distance < 0.001f)
            return true;

        // Görüş açısını yatay düzlemde ölç.
        Vector3 flatDirection =
            Vector3.ProjectOnPlane(direction, Vector3.up);

        Vector3 flatForward =
            Vector3.ProjectOnPlane(eyes.forward, Vector3.up);

        if (Vector3.Angle(flatForward, flatDirection) > viewAngle * 0.5f)
            return false;

        // Arada duvar/sıra varsa oyuncuyu göremez.
        bool blocked = Physics.Raycast(
            eyes.position,
            direction.normalized,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        return !blocked;
    }

    private void CatchPlayer()
{
    if (approachingPlayer || gameOver)
        return;

    caughtThisLean = true;
    seenTimer = 0f;
    warningCount++;

    approachingPlayer = true;
    StartCoroutine(ApproachPlayer());
}

private IEnumerator ApproachPlayer()
{
    teacherPatrol.PausePatrol();
    float normalSpeed = agent.speed;
    agent.speed = approachSpeed;

    if (!agent.isOnNavMesh)
    {
        AbortApproach();
        yield break;
    }

    // Hedefin tamamen ulaşılabilir olduğundan emin ol.
    NavMeshPath path = new NavMeshPath();

    if (!agent.CalculatePath(playerStandPoint.position, path) ||
        path.status != NavMeshPathStatus.PathComplete)
    {
        AbortApproach();
        yield break;
    }

    agent.updateRotation = true;
    agent.isStopped = false;

    if (!agent.SetPath(path))
    {
        AbortApproach();
        yield break;
    }

    // Agent'ın yolu güncellemesini bekle.
    yield return null;

    float travelTimer = 0f;

    while (true)
    {
        if (!agent.isOnNavMesh)
        {
            AbortApproach();
            yield break;
        }

        if (!agent.pathPending)
        {
            if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                AbortApproach();
                yield break;
            }

            if (agent.remainingDistance <=
                Mathf.Max(agent.stoppingDistance, 0.15f))
                break;
        }

        travelTimer += Time.deltaTime;

        if (travelTimer >= approachTimeout)
        {
            AbortApproach();
            yield break;
        }

        yield return null;
    }

    // Yanına geldi: hareketi tamamen durdur.
    agent.isStopped = true;
    agent.ResetPath();
    agent.velocity = Vector3.zero;
    agent.updateRotation = false;

    animator.SetBool("isWalking", false);

    // Oyuncuya dön.
    float turnTimer = 0f;

    while (turnTimer < 2f)
    {
        Vector3 direction = playerHead.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.rotation, targetRotation) < 2f)
            break;

        turnTimer += Time.deltaTime;
        yield return null;
    }

    // Animasyon yalnızca yanına geldikten sonra başlar.
    animator.SetTrigger("CaughtPlayer");

    if (warningText != null)
    {
        warningText.text = warningCount == 1
            ? "Önüne dön! Bir daha görmeyeyim.\nUyarı: 1 / 2"
            : "Seni tekrar yakaladım! Sınavın bitti. Tekrar başlatmak için R tuşuna bas.";

        warningText.gameObject.SetActive(true);
        warningTimer = caughtAnimationDuration;
    }

    yield return new WaitForSeconds(caughtAnimationDuration);

    if (warningCount >= 2)
    {

        EndGame();
        yield break;
    }

    agent.speed = normalSpeed;
    teacherPatrol.ResumePatrol();
    approachingPlayer = false;
}

private void AbortApproach()
{
    Debug.LogWarning(
        "Hoca oyuncuya ulaşamadı. Durma noktasını ve NavMesh'i kontrol et.",
        this
    );

    // Tamamlanmayan karşılaşmayı uyarı olarak sayma.
    warningCount = Mathf.Max(0, warningCount - 1);

    if (agent.isOnNavMesh)
        teacherPatrol.ResumePatrol();

    approachingPlayer = false;
}

    private void UpdateWarning()
    {
        if (warningTimer <= 0f)
            return;

        warningTimer -= Time.deltaTime;

        if (warningTimer <= 0f && warningText != null)
            warningText.gameObject.SetActive(false);
    }

    private void EndGame()
    {
        gameOver = true;

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        if (controlsToDisable != null)
        {
            foreach (Behaviour control in controlsToDisable)
            {
                if (control != null)
                    control.enabled = false;
            }
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("İkinci kez yakalandın. Oyun bitti. Tekrar başlatmak için R tuşuna bas.");
        
        
    }
}