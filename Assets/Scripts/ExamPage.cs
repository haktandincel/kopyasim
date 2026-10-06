using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ExamPages : MonoBehaviour
{

    [Header("İsim yazarken kalem")]

public float letterStep = 0.008f;
public float maxWritingDistance = 0.3f;

public GameObject AdiSoyadi;

public bool HasStarted => !introOpen && selectedAnswers != null;

private float nextNameFocusTime;


public float writingMoveSpeed = 0.15f;

public Vector3 namePencilStart = new Vector3(0.370999992f,1.05980003f,-0.485700011f);


public Vector3 letterOffset = new Vector3(
    -0.0135f,
    -0.00075f,
    -0.0013f
);

    [Header("Başlangıç kağıdı")]
public TMP_InputField nameInput;

// Buraya yalnızca FPS kamera ve SeatedLean scriptlerini bağla.
public Behaviour[] controlsToDisableBeforeStart;

public string PlayerName { get; private set; }

private bool introOpen = true;
private bool nameSubmitted;
private int nameSubmittedFrame;
private bool[] previousControlStates;

    [Serializable]
    public class JsonRoot
    {
        public JsonCategory[] categories;
    }

    [Serializable]
    public class JsonCategory
    {
        public string category;
        public JsonQuestion[] questions;
    }

    [Serializable]
    public class JsonQuestion
    {
        public int id;
        public string type;
        public string question;
        public string[] options;
        public string correctAnswer;
    }

    [Serializable]
    public class Question
    {
        [TextArea(2, 6)]
        public string question;

        public string optionA;
        public string optionB;
        public string optionC;
        public string optionD;

        [Tooltip("0 = A, 1 = B, 2 = C, 3 = D")]
        [Range(0, 3)]
        public int correctAnswer;
    }

    [Header("JSON soru havuzu")]
    public TextAsset questionsJson;

    [Header("Kağıttaki yazılar")]
    public TMP_Text questionText;
    public TMP_Text optionAText;
    public TMP_Text optionBText;
    public TMP_Text optionCText;
    public TMP_Text optionDText;

    [Header("İşaretler")]
    public GameObject yuvarlakA;
    public GameObject yuvarlakB;
    public GameObject yuvarlakC;
    public GameObject yuvarlakD;

    [Header("Kalem")]
    public GameObject pencil;
    public char secilenSik = 'A';

    [Header("Oyun başlangıcında seçilen sorular")]
    public Question[] questions;

    [Header("Sınav bitince kapatılacak scriptler")]
    public Behaviour[] controlsToDisableOnFinish;

    private int currentPage;
    private int[] selectedAnswers;

    private bool confirmationOpen;
    private bool examFinished;

    public int CurrentPage => currentPage;
    public bool IsFinished => examFinished;


    private void LateUpdate()
{
    if (!introOpen || nameSubmitted || nameInput == null)
        return;

    if (!Application.isFocused || !nameInput.gameObject.activeInHierarchy)
        return;

    if (nameInput.isFocused)
        return;

    // Time.timeScale = 0 iken de çalışır.
    if (Time.unscaledTime < nextNameFocusTime)
        return;

    nextNameFocusTime = Time.unscaledTime + 0.1f;

    if (EventSystem.current == null)
    {
        Debug.LogError("Sahnede aktif EventSystem yok.", this);
        return;
    }

    nameInput.enabled = true;
    nameInput.interactable = true;
    nameInput.readOnly = false;

    EventSystem.current.SetSelectedGameObject(nameInput.gameObject);
    nameInput.ActivateInputField();
}

    private void ShowIntroPage()
{
    introOpen = true;
    nameSubmitted = false;
    PlayerName = "";

    // Hoca, animasyonlar ve oyun zamanı beklesin.
    Time.timeScale = 0f;

    // Fareyle bakışı ve uzanmayı da kapat.
    if (controlsToDisableBeforeStart != null)
    {
        previousControlStates =
            new bool[controlsToDisableBeforeStart.Length];

        for (int i = 0; i < controlsToDisableBeforeStart.Length; i++)
        {
            Behaviour control = controlsToDisableBeforeStart[i];

            if (control == null || control == this)
                continue;

            previousControlStates[i] = control.enabled;
            control.enabled = false;
        }
    }

    questionText.text = "";

    optionAText.text = "A) Başla";
    optionBText.text = "";
    optionCText.text = "Adını soyadını yaz";
optionDText.text = "↓ Başla";

    HideCircles();
    pencil.SetActive(true);



    nameInput.gameObject.SetActive(true);
    nameInput.interactable = true;
    nameInput.readOnly = false;
    nameInput.contentType = TMP_InputField.ContentType.Standard;
    nameInput.lineType = TMP_InputField.LineType.SingleLine;
    nameInput.characterLimit = 40;
    nameInput.richText = false;
    nameInput.textComponent.richText = false;
    nameInput.SetTextWithoutNotify("");

    nameInput.onSubmit.AddListener(OnNameSubmitted);

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    FocusNameAgain();}

private IEnumerator FocusNameInput()
{
    // Diğer başlangıç işlemleri tamamlansın.
    yield return null;

    if (!introOpen || nameSubmitted || nameInput == null)
        yield break;

    if (EventSystem.current == null)
    {
        Debug.LogError("Sahneye UI > Event System ekle.", this);
        yield break;
    }

    nameInput.gameObject.SetActive(true);
    nameInput.enabled = true;
    nameInput.interactable = true;
    nameInput.readOnly = false;

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    EventSystem.current.SetSelectedGameObject(null);
    EventSystem.current.SetSelectedGameObject(nameInput.gameObject);

    nameInput.ActivateInputField();

    yield return null;

    if (introOpen && !nameSubmitted)
    {
        nameInput.caretPosition = nameInput.text.Length;
        nameInput.ForceLabelUpdate();
    }
}

private void OnNameSubmitted(string value)
{
    if (!introOpen || nameSubmitted)
        return;

    PlayerName = value;

    // Kalem Başla seçeneğine geçiyor.
    nameSubmitted = true;
    nameSubmittedFrame = Time.frameCount;

    nameInput.readOnly = true;
    nameInput.DeactivateInputField();
    nameInput.interactable = false;

    if (EventSystem.current != null)
        EventSystem.current.SetSelectedGameObject(null);

    questionText.text = "";

    optionAText.text = "A) Başla";
    optionBText.text = "";
    optionCText.text = "Enter: Başla";
    optionDText.text = "↑ Ad soyad";

    pencil.SetActive(true);
    SetFocusedOption('A');
}


private void UpdateWritingPencil()
{    
    Vector3 targetPosition =
        namePencilStart +
        letterOffset * nameInput.text.Length;

    pencil.transform.position = Vector3.MoveTowards(
        pencil.transform.position,
        targetPosition,
        writingMoveSpeed * Time.unscaledDeltaTime
    );

}

private void FocusNameAgain()
{
    nameSubmitted = false;

    nameInput.interactable = true;
    nameInput.readOnly = false;

    optionCText.text = "Adını soyadını yaz";
    optionDText.text = "↓ Başla";

    pencil.SetActive(true);
    pencil.transform.position = new Vector3(0.401800007f,1.06309998f,-0.482300013f);
    
    pencil.transform.rotation = Quaternion.Euler(-50.3f,178.336f,-122.212f);

    StartCoroutine(FocusNameInput());
}

private void HandleIntroInput()
{
    // Ad soyad alanındayız.
    if (!nameSubmitted)
    {
        UpdateWritingPencil();

        if (Input.GetKeyDown(KeyCode.DownArrow))
            OnNameSubmitted(nameInput.text);

        return;
    }

    // Alan değiştirilen karede Enter yeniden işlenmesin.
    if (Time.frameCount <= nameSubmittedFrame)
        return;

    // Başla'dan tekrar ad soyad alanına dön.
    if (Input.GetKeyDown(KeyCode.UpArrow) ||
        Input.GetKeyDown(KeyCode.Q))
    {
        FocusNameAgain();
        return;
    }

    if (Input.GetKeyDown(KeyCode.Return) ||
        Input.GetKeyDown(KeyCode.KeypadEnter))
    {
        BeginExam();
    }
}

private void BeginExam()
{
   if (!introOpen || !nameSubmitted)
        return;

    if (string.IsNullOrWhiteSpace(nameInput.text))
    {
        questionText.text = "";
        FocusNameAgain();
        return;
    }

    PlayerName = nameInput.text.Trim();

    // Buradan sonra mevcut kodun devam etsin:
    introOpen = false;

    AdiSoyadi.SetActive(false);

    

    nameInput.onSubmit.RemoveListener(OnNameSubmitted);
    nameInput.DeactivateInputField();

    if (EventSystem.current != null)
        EventSystem.current.SetSelectedGameObject(null);

    nameInput.gameObject.SetActive(false);

    currentPage = 0;

    pencil.SetActive(true);
    SetFocusedOption('A');
    RefreshPage();

    if (controlsToDisableBeforeStart != null &&
        previousControlStates != null)
    {
        for (int i = 0; i < controlsToDisableBeforeStart.Length; i++)
        {
            Behaviour control = controlsToDisableBeforeStart[i];

            if (control != null && control != this)
                control.enabled = previousControlStates[i];
        }
    }

    Time.timeScale = 1f;

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}

    private void Start()
    {
        if (questionText == null ||
            optionAText == null || optionBText == null ||
            optionCText == null || optionDText == null ||
            yuvarlakA == null || yuvarlakB == null ||
            yuvarlakC == null || yuvarlakD == null ||
            pencil == null)
        {
            Debug.LogError(
                "Yazı, yuvarlak veya kalem referansları eksik.",
                this
            );

            enabled = false;
            return;
        }

        if (!LoadRandomQuestions())
        {
            enabled = false;
            return;
        }

        selectedAnswers = new int[questions.Length];

        for (int i = 0; i < selectedAnswers.Length; i++)
            selectedAnswers[i] = -1;

        currentPage = 0;
        confirmationOpen = false;
        examFinished = false;

        
        if (nameInput == null)
{
    Debug.LogError("Name Input alanını bağla.", this);
    enabled = false;
    return;
}

ShowIntroPage();
    }

    private void Update()
    {

        if (examFinished)
{
    if (Input.GetKeyDown(KeyCode.R))
    {
        Time.timeScale = 1f;

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager
                .GetActiveScene().buildIndex
        );
    }

    return;
}

        if (introOpen)
{
    HandleIntroInput();
    return;
}

        if (examFinished || Time.timeScale == 0f)
            return;

        if (confirmationOpen)
        {
            HandleFinishInput();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            NextPage();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            PreviousPage();
            return;
        }

        HandleOptionMovement();

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SelectAnswer(secilenSik - 'A');
        }
    }

    private void HandleOptionMovement()
    {
        // Şık düzeni:
        // A    B
        // C    D

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (secilenSik == 'B')
                SetFocusedOption('A');
            else if (secilenSik == 'D')
                SetFocusedOption('C');
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (secilenSik == 'A')
                SetFocusedOption('B');
            else if (secilenSik == 'C')
                SetFocusedOption('D');
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (secilenSik == 'C')
                SetFocusedOption('A');
            else if (secilenSik == 'D')
                SetFocusedOption('B');
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (secilenSik == 'A')
                SetFocusedOption('C');
            else if (secilenSik == 'B')
                SetFocusedOption('D');
        }
    }

    private void SetFocusedOption(char option)
    {
        secilenSik = option;

        if (pencil == null)
            return;

        Vector3 position;

        switch (option)
        {
            case 'A':
                position = new Vector3(0.442999989f,1.06299996f,-0.158999994f);
                break;

            case 'B':
                position = new Vector3(0.290199995f,1.05830002f,-0.169200003f);
                break;

            case 'C':
                position = new Vector3(0.470499992f,1.06359994f,-0.079400003f);
                break;

            case 'D':
                position = new Vector3(0.292899996f,1.06500006f,-0.0703999996f);
                break;

            default:
                return;
        }

        pencil.transform.SetPositionAndRotation(
            position,
            Quaternion.Euler(-50.3f, 178.336f, -122.212f)
        );
    }

    public void NextPage()
    {
        if (selectedAnswers == null ||
            examFinished || confirmationOpen)
            return;

        if (currentPage >= questions.Length - 1)
        {
            ShowFinishQuestion();
            return;
        }

        currentPage++;
        RestoreFocus();
        RefreshPage();
    }

    public void PreviousPage()
    {
        if (selectedAnswers == null || examFinished)
            return;

        if (confirmationOpen)
        {
            CancelFinish();
            return;
        }

        if (currentPage <= 0)
            return;

        currentPage--;
        RestoreFocus();
        RefreshPage();
    }

    private void RestoreFocus()
    {
        int answer = selectedAnswers[currentPage];

        SetFocusedOption(
            answer >= 0 ? (char)('A' + answer) : 'A'
        );
    }

    public void SelectAnswer(int answerIndex)
    {
        if (selectedAnswers == null ||
            examFinished || confirmationOpen ||
            answerIndex < 0 || answerIndex > 3)
            return;

        selectedAnswers[currentPage] = answerIndex;
        RefreshPage();
    }

    private void RefreshPage()
    {
        Question q = questions[currentPage];

        questionText.text =
            $" {currentPage + 1}) {q.question}";

        optionAText.text = $"A) {q.optionA}";
        optionBText.text = $"B) {q.optionB}";
        optionCText.text = $"C) {q.optionC}";
        optionDText.text = $"D) {q.optionD}";

        int answer = selectedAnswers[currentPage];

        yuvarlakA.SetActive(answer == 0);
        yuvarlakB.SetActive(answer == 1);
        yuvarlakC.SetActive(answer == 2);
        yuvarlakD.SetActive(answer == 3);
    }

    private void ShowFinishQuestion()
    {
        confirmationOpen = true;

        questionText.text = "Sınavı bitirmek ister misin?";

        optionAText.text = "A) Evet";
        optionBText.text = "B) Hayır";
        optionCText.text = "";
        optionDText.text = "";

        HideCircles();
        SetFocusedOption('A');
    }

    private void HandleFinishInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SetFocusedOption('A');
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            SetFocusedOption('B');

        if (Input.GetKeyDown(KeyCode.Q) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            CancelFinish();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (secilenSik == 'A')
                FinishExam();
            else if (secilenSik == 'B')
                CancelFinish();
        }
    }

    public void CancelFinish()
    {
        if (examFinished || !confirmationOpen)
            return;

        confirmationOpen = false;

        RestoreFocus();
        RefreshPage();
    }

    public void FinishExam()
    {
        if (!confirmationOpen || examFinished ||
            selectedAnswers == null)
            return;

        examFinished = true;
        confirmationOpen = false;

        int correct = GetCorrectCount();
        int blank = 0;

        foreach (int answer in selectedAnswers)
        {
            if (answer == -1)
                blank++;
        }

        int wrong = questions.Length - correct - blank;

        int score = Mathf.RoundToInt(
            correct * 100f / questions.Length
        );

        questionText.text = "yeni oyun için R tuşuna bas";

        optionAText.text = $"Doğru: {correct}";
        optionBText.text = $"Yanlış: {wrong}";
        optionCText.text = $"Boş: {blank}";
        optionDText.text = $"Puan: {score} / 100";

        HideCircles();
        pencil.SetActive(false);

        if (controlsToDisableOnFinish != null)
        {
            foreach (Behaviour control in controlsToDisableOnFinish)
            {
                if (control != null)
                    control.enabled = false;
            }
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideCircles()
    {
        yuvarlakA.SetActive(false);
        yuvarlakB.SetActive(false);
        yuvarlakC.SetActive(false);
        yuvarlakD.SetActive(false);
    }

    private bool LoadRandomQuestions()
    {
        if (questionsJson == null)
        {
            Debug.LogError("JSON dosyasını bağla.", this);
            return false;
        }

        JsonRoot data;

        try
        {
            data = JsonUtility.FromJson<JsonRoot>(questionsJson.text);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"JSON okunamadı: {exception.Message}",
                this
            );

            return false;
        }

        if (data == null || data.categories == null ||
            data.categories.Length == 0)
        {
            Debug.LogError("JSON içinde kategori bulunamadı.", this);
            return false;
        }

        List<Question> examQuestions = new List<Question>();

        foreach (JsonCategory category in data.categories)
        {
            if (category == null || category.questions == null)
            {
                Debug.LogError("Boş kategori bulundu.", this);
                return false;
            }

            List<Question> categoryPool = new List<Question>();

            foreach (JsonQuestion item in category.questions)
            {
                if (item == null ||
                    item.type != "text" ||
                    string.IsNullOrWhiteSpace(item.question) ||
                    item.options == null ||
                    item.options.Length != 4)
                    continue;

                int correctIndex = Array.IndexOf(
                    item.options,
                    item.correctAnswer
                );

                if (correctIndex < 0)
                    continue;

                categoryPool.Add(new Question
                {
                    question = item.question,
                    optionA = item.options[0],
                    optionB = item.options[1],
                    optionC = item.options[2],
                    optionD = item.options[3],
                    correctAnswer = correctIndex
                });
            }

            if (categoryPool.Count == 0)
            {
                Debug.LogError(
                    $"{category.category} kategorisinde uygun soru yok.",
                    this
                );

                return false;
            }

            int randomIndex = UnityEngine.Random.Range(
                0,
                categoryPool.Count
            );

            examQuestions.Add(categoryPool[randomIndex]);
        }

        // Her kategoriden seçilen birer sorunun sırasını karıştır.
        for (int i = examQuestions.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            Question temporary = examQuestions[i];
            examQuestions[i] = examQuestions[randomIndex];
            examQuestions[randomIndex] = temporary;
        }

        questions = examQuestions.ToArray();
        return true;
    }

    public int GetCorrectCount()
    {
        if (selectedAnswers == null)
            return 0;

        int count = 0;

        for (int i = 0; i < questions.Length; i++)
        {
            if (selectedAnswers[i] == questions[i].correctAnswer)
                count++;
        }

        return count;
    }
}