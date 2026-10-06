using System.Collections;
using TMPro;
using UnityEngine;

public class StudentExam : MonoBehaviour
{
    [Header("Ortak sınav")]
    public ExamPages exam;

    [Header("Öğrencinin kağıdındaki yazılar")]
    public TMP_Text questionText;
    public TMP_Text optionAText;
    public TMP_Text optionBText;
    public TMP_Text optionCText;
    public TMP_Text optionDText;

    [Header("Öğrencinin cevap işaretleri")]
    public GameObject yuvarlakA;
    public GameObject yuvarlakB;
    public GameObject yuvarlakC;
    public GameObject yuvarlakD;

    [Header("Öğrencinin başarısı")]
    [Range(0f, 1f)]
    public float correctProbability = 0.8f;

    [Header("Her soruyu çözme süresi")]
    public float minAnswerTime = 3f;
    public float maxAnswerTime = 8f;

    [Header("Cevapladıktan sonra sayfayı çevirme süresi")]
    public float minPageWait = 4f;
    public float maxPageWait = 7f;

    private int[] answers;
    private int[] questionOrder;
    private int currentQuestion = -1;

    private IEnumerator Start()
    {
        if (exam == null ||
            questionText == null ||
            optionAText == null || optionBText == null ||
            optionCText == null || optionDText == null ||
            yuvarlakA == null || yuvarlakB == null ||
            yuvarlakC == null || yuvarlakD == null)
        {
            Debug.LogError(
                $"{name}: Sınav, yazı veya yuvarlak bağlantısı eksik.",
                this
            );

            enabled = false;
            yield break;
        }

        ClearPaper();

        // Oyuncu sınavı başlatana kadar bekle.
        yield return new WaitUntil(() => exam.HasStarted);

        if (exam.IsFinished ||
            exam.questions == null ||
            exam.questions.Length == 0)
        {
            yield break;
        }

        int count = exam.questions.Length;

        answers = new int[count];
        questionOrder = new int[count];

        for (int i = 0; i < count; i++)
        {
            answers[i] = -1;
            questionOrder[i] = i;
        }

        // Her öğrenci için farklı, rastgele soru sırası.
        // Her soru yalnızca bir kez çözülür.
        for (int i = count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            int temporary = questionOrder[i];
            questionOrder[i] = questionOrder[randomIndex];
            questionOrder[randomIndex] = temporary;
        }

        for (int step = 0; step < count; step++)
        {
            if (exam.IsFinished)
                yield break;

            currentQuestion = questionOrder[step];
            RefreshPaper();

            // Soruyu çöz.
            yield return WaitWhileExamRunning(
                RandomDuration(minAnswerTime, maxAnswerTime)
            );

            if (exam.IsFinished)
                yield break;

            answers[currentQuestion] = ChooseAnswer(
                exam.questions[currentQuestion].correctAnswer
            );

            RefreshPaper();

            // Son sorunun cevabı kağıtta kalır.
            if (step == count - 1)
                yield break;

            // İşaret görünür kalsın; sonra sayfayı çevir.
            yield return WaitWhileExamRunning(
                RandomDuration(minPageWait, maxPageWait)
            );
        }
    }

    private IEnumerator WaitWhileExamRunning(float duration)
    {
        float timer = 0f;

        while (timer < duration && !exam.IsFinished)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private float RandomDuration(float minimum, float maximum)
    {
        minimum = Mathf.Max(0.1f, minimum);
        maximum = Mathf.Max(minimum, maximum);

        return Random.Range(minimum, maximum);
    }

    private int ChooseAnswer(int correctAnswer)
    {
        if (Random.value < correctProbability)
            return correctAnswer;

        // Doğru cevap dışındaki üç şıktan birini seç.
        int wrongAnswer = Random.Range(0, 3);

        if (wrongAnswer >= correctAnswer)
            wrongAnswer++;

        return wrongAnswer;
    }

    private void RefreshPaper()
    {
        if (answers == null ||
            currentQuestion < 0 ||
            currentQuestion >= answers.Length)
        {
            return;
        }

        ExamPages.Question question =
            exam.questions[currentQuestion];

        // Oyuncunun kağıdındaki soru numarasıyla aynı numara.
        questionText.text =
            $"Soru {currentQuestion + 1}\n\n{question.question}";

        optionAText.text = $"A) {question.optionA}";
        optionBText.text = $"B) {question.optionB}";
        optionCText.text = $"C) {question.optionC}";
        optionDText.text = $"D) {question.optionD}";

        ShowAnswer(answers[currentQuestion]);
    }

    private void ShowAnswer(int answer)
    {
        yuvarlakA.SetActive(answer == 0);
        yuvarlakB.SetActive(answer == 1);
        yuvarlakC.SetActive(answer == 2);
        yuvarlakD.SetActive(answer == 3);
    }

    private void ClearPaper()
    {
        questionText.text = "";
        optionAText.text = "";
        optionBText.text = "";
        optionCText.text = "";
        optionDText.text = "";

        ShowAnswer(-1);
    }
}