using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TeacherPatrol : MonoBehaviour
{
    [Serializable]
    public class Student
    {
        public Animator animator;
        public Transform standPoint; // Öğretmenin duracağı yer
        public Transform lookTarget; // Öğrencinin bakılacak noktası
    }


    private bool externallyPaused;

public void PausePatrol()
{
    externallyPaused = true;

    LowerHand();
    StopMoving();

    agent.updateRotation = true;
}

public void ResumePatrol()
{
    externallyPaused = false;

    ScheduleNextRaise();
    ChooseNextPoint();
}

    private enum State
    {
        Patrol,
        PatrolWait,
        GoingToStudent,
        HelpingStudent
    }

    [Header("Öğretmen")]
    public Animator animator;
    public Transform[] patrolPoints;

    [Header("Devriye beklemesi")]
    public float minWait = 1f;
    public float maxWait = 3f;

    [Header("Öğrenciler")]
    public Student[] students;

    [Header("Parmak kaldırma aralığı")]
    public float minRaiseInterval = 10f;
    public float maxRaiseInterval = 20f;

    [Header("Öğrenciyle ilgilenme")]
    public float minHelpTime = 4f;
    public float maxHelpTime = 7f;
    public float turnSpeed = 180f;

    // Kopya sisteminden bu değeri okuyabilirsin.
    public bool IsDistracted => state == State.HelpingStudent;

    private NavMeshAgent agent;
    private State state;
    private Student activeStudent;

    private readonly List<Student> validStudents = new List<Student>();

    private int currentPoint = -1;
    private float waitTimer;
    private float nextRaiseTime;
    private Vector3 lastPosition;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;

        if (!agent.isOnNavMesh ||
            patrolPoints == null || patrolPoints.Length < 2)
        {
            Debug.LogError("NavMesh ve devriye noktalarını kontrol et.", this);
            enabled = false;
            return;
        }

        foreach (Transform point in patrolPoints)
        {
            if (point == null)
            {
                Debug.LogError("Boş devriye noktası var.", this);
                enabled = false;
                return;
            }
        }

        if (students != null)
        {
            foreach (Student student in students)
            {
                if (student == null ||
                    student.animator == null ||
                    student.standPoint == null ||
                    student.lookTarget == null)
                    continue;

                student.animator.SetBool("handRaised", false);
                validStudents.Add(student);
            }
        }

        ScheduleNextRaise();
        ChooseNextPoint();
    }

    private void Update()
    {

        if (externallyPaused)
    return;

        if (!agent.isOnNavMesh)
            return;

        // Öğretmen meşgulken başka öğrenci çağırmaz.
        if ((state == State.Patrol || state == State.PatrolWait) &&
            validStudents.Count > 0 &&
            Time.time >= nextRaiseTime)
        {
            CallTeacher();
        }

        switch (state)
        {
            case State.Patrol:
            case State.GoingToStudent:
                CheckArrival();
                break;

            case State.PatrolWait:
                waitTimer -= Time.deltaTime;

                if (waitTimer <= 0f)
                    ChooseNextPoint();
                break;

            case State.HelpingStudent:
                FaceStudent();
                waitTimer -= Time.deltaTime;

                if (waitTimer <= 0f)
                {
                    LowerHand();
                    ScheduleNextRaise();
                    ChooseNextPoint();
                }
                break;
        }
    }

    private void CallTeacher()
    {
        activeStudent =
            validStudents[UnityEngine.Random.Range(0, validStudents.Count)];

        activeStudent.animator.SetBool("handRaised", true);
        state = State.GoingToStudent;

        MoveTo(activeStudent.standPoint.position);
    }

    private void CheckArrival()
    {
        if (agent.pathPending)
            return;

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning("Hedefe yol bulunamadı; noktayı kontrol et.", this);

            LowerHand();
            ScheduleNextRaise();
            StopMoving();

            state = State.PatrolWait;
            waitTimer = 1f;
            return;
        }

        if (agent.remainingDistance >
            Mathf.Max(agent.stoppingDistance, 0.15f))
            return;

        StopMoving();

        if (state == State.GoingToStudent)
        {
            // Agent'ın dönüşünü kapatıp öğrenciye kendimiz dönüyoruz.
            agent.updateRotation = false;
            state = State.HelpingStudent;
            waitTimer = UnityEngine.Random.Range(minHelpTime, maxHelpTime);
        }
        else
        {
            state = State.PatrolWait;
            waitTimer = UnityEngine.Random.Range(minWait, maxWait);
        }
    }

    private void FaceStudent()
    {
        if (activeStudent == null || activeStudent.lookTarget == null)
            return;

        Vector3 direction =
            activeStudent.lookTarget.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    private void ChooseNextPoint()
    {
        int nextPoint;

        if (currentPoint < 0)
        {
            nextPoint = UnityEngine.Random.Range(0, patrolPoints.Length);
        }
        else
        {
            nextPoint = UnityEngine.Random.Range(0, patrolPoints.Length - 1);

            if (nextPoint >= currentPoint)
                nextPoint++;
        }

        currentPoint = nextPoint;
        state = State.Patrol;

        MoveTo(patrolPoints[currentPoint].position);
    }

    private void MoveTo(Vector3 position)
    {
        agent.updateRotation = true;
        agent.isStopped = false;

        if (!agent.SetDestination(position))
        {
            LowerHand();
            ScheduleNextRaise();
            StopMoving();

            state = State.PatrolWait;
            waitTimer = 1f;
        }
    }

    private void StopMoving()
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    private void ScheduleNextRaise()
    {
        nextRaiseTime = Time.time +
            UnityEngine.Random.Range(minRaiseInterval, maxRaiseInterval);
    }

    private void LowerHand()
    {
        if (activeStudent != null && activeStudent.animator != null)
            activeStudent.animator.SetBool("handRaised", false);

        activeStudent = null;
    }

    private void LateUpdate()
    {
        Vector3 movement = transform.position - lastPosition;
        movement.y = 0f;
        lastPosition = transform.position;

        float actualSpeed = Time.deltaTime > 0f
            ? movement.magnitude / Time.deltaTime
            : 0f;

        if (animator != null)
        {
            animator.SetBool("isWalking",
                agent != null &&
                agent.isOnNavMesh &&
                !agent.isStopped &&
                actualSpeed > 0.01f);
        }
    }

    private void OnDisable()
    {
        LowerHand();

        if (animator != null)
            animator.SetBool("isWalking", false);

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            StopMoving();
            agent.updateRotation = true;
        }
    }
}