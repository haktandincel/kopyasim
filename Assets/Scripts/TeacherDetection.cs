using TMPro;
using UnityEngine;

public class TeacherDetection : MonoBehaviour
{
    [Header("Referanslar")]
    public SeatedLean playerLean;
    public TeacherPatrol teacherPatrol;

    

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

    private void Start()
    {
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

    private void LateUpdate()
    {
        if (gameOver)
            return;

        UpdateWarning();

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
        caughtThisLean = true;
        seenTimer = 0f;
        warningCount++;

        if (warningCount == 1)
        {
            
            Debug.Log("Hoca: Önüne dön! Bir daha görmeyeyim.");

            if (warningText != null)
            {
                warningText.text =
                    "Önüne dön! Bir daha görmeyeyim.\nUyarı: 1 / 2";

                warningText.gameObject.SetActive(true);
                warningTimer = 3f;
            }
        }
        else
        {
            EndGame();
        }
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

        Debug.Log("İkinci kez yakalandın. Oyun bitti.");
    }
}