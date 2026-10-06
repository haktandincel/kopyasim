using System;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ExamPages : MonoBehaviour
{   

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

[Header("JSON soru havuzu")]
public TextAsset questionsJson;

[Min(1)]
public int examQuestionCount = 10;




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

    [Header("Kağıttaki yazılar")]
    public TMP_Text questionText;
    public TMP_Text optionAText;
    public TMP_Text optionBText;
    public TMP_Text optionCText;
    public TMP_Text optionDText;
    public TMP_Text pageNumberText;

    public char secilenSik = 'A';

    public GameObject yuvarlakA;
    public GameObject yuvarlakB;
    public GameObject yuvarlakC;

    public GameObject yuvarlakD;

    public Question[] questions = new Question[10];

    public GameObject pencil;

    private int currentPage;
    private int[] selectedAnswers;

    public int CurrentPage => currentPage;

    private void Start()
{
    if (!LoadRandomQuestions())
    {
        Debug.LogError("Sorular JSON'dan yüklenemedi.", this);
        enabled = false;
        return;
    }

    if (questionText == null ||
        optionAText == null || optionBText == null ||
        optionCText == null || optionDText == null ||
        yuvarlakA == null || yuvarlakB == null ||
        yuvarlakC == null || yuvarlakD == null)
    {
        Debug.LogError("Yazı veya yuvarlak referansları eksik.", this);
        enabled = false;
        return;
    }

    selectedAnswers = new int[questions.Length];

    for (int i = 0; i < selectedAnswers.Length; i++)
        selectedAnswers[i] = -1;

    currentPage = 0;
    secilenSik = 'A';

    RefreshPage();

    Debug.Log($"{questions.Length} soru yüklendi.", this);
}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            NextPage();
        else if (Input.GetKeyDown(KeyCode.Q))
            PreviousPage();


        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (secilenSik == 'B')
            {
                secilenSik = 'A';
                pencil.transform.position = new Vector3(-0.0599999987f,1.11000001f,-0.569999993f);
                pencil.transform.rotation = Quaternion.Euler(-50.3f, 178.336f, -122.212f);
            }
            else if (secilenSik == 'D')
            {
                secilenSik = 'C';
                pencil.transform.position = new Vector3(-0.0599999987f,1.11000001f,-0.519999981f);
                pencil.transform.rotation = Quaternion.Euler(-50.3f, 178.336f, -122.212f);

            }           
        }
        
            
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (secilenSik == 'A')
            {
                secilenSik = 'B';
                pencil.transform.position = new Vector3(-0.230000004f,1.11000001f,-0.569999993f);
                pencil.transform.rotation = Quaternion.Euler(-50.3f, 178.336f, -122.212f);

            }
            
            else if (secilenSik == 'C')
            {
                secilenSik = 'D';
                pencil.transform.position = new Vector3(-0.230000004f,1.11000001f,-0.49000001f);
                pencil.transform.rotation = Quaternion.Euler(-50.3f, 178.336f, -122.212f);
            }           
        }
            
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (secilenSik == 'C')
            {
                secilenSik = 'A';
                pencil.transform.position = new Vector3(-0.0599999987f,1.11000001f,-0.569999993f);
                pencil.transform.rotation = Quaternion.Euler(-50.3f, 178.336f, -122.212f);

            }
            else if (secilenSik == 'D')
            {
                secilenSik = 'B';
                pencil.transform.position = new Vector3(-0.230000004f,1.11000001f,-0.569999993f);
                pencil.transform.rotation = Quaternion.Euler(-50.3f, 178.336f, -122.212f);

            }
        }       
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (secilenSik == 'A')
            {
                secilenSik = 'C';
                pencil.transform.position = new Vector3(-0.0599999987f,1.11000001f,-0.519999981f);
                pencil.transform.rotation = Quaternion.Euler(-50.3f, 178.336f, -122.212f);
            }
            else if (secilenSik == 'B')
            {
                
                secilenSik = 'D';
                pencil.transform.position = new Vector3(-0.230000004f,1.11000001f,-0.49000001f);
                pencil.transform.rotation = Quaternion.Euler(-50.3f, 178.336f, -122.212f);
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            switch (secilenSik)
            {
                case 'A':
                    SelectAnswer(0);
                    break;
                case 'B':
                    SelectAnswer(1);
                    break;
                case 'C':
                    SelectAnswer(2);
                    break;
                case 'D':
                    SelectAnswer(3);
                    break;
            }
        }
    }

    public void NextPage()
    {
        if (selectedAnswers == null ||
            currentPage >= questions.Length - 1)
            return;

        currentPage++;
        RefreshPage();
    }

    public void PreviousPage()
    {
        if (selectedAnswers == null || currentPage <= 0)
            return;

        currentPage--;
        RefreshPage();
    }

    public void SelectAnswer(int answerIndex)
    {
        if (selectedAnswers == null ||
            answerIndex < 0 || answerIndex > 3)
            return;

        selectedAnswers[currentPage] = answerIndex;
        RefreshPage();
    }

    private void RefreshPage()
{
    Question q = questions[currentPage];

    questionText.text = $"Soru {currentPage + 1}\n\n{q.question}";

    optionAText.text = $"A) {q.optionA}";
    optionBText.text = $"B) {q.optionB}";
    optionCText.text = $"C) {q.optionC}";
    optionDText.text = $"D) {q.optionD}";

    int answer = selectedAnswers[currentPage];

    yuvarlakA.SetActive(answer == 0);
    yuvarlakB.SetActive(answer == 1);
    yuvarlakC.SetActive(answer == 2);
    yuvarlakD.SetActive(answer == 3);

    if (pageNumberText != null)
    {
        pageNumberText.text =
            $"{currentPage + 1} / {questions.Length}";
    }
}

//     private string FormatOption(int index, string letter, string text)
// {
//     string mark = selectedAnswers[currentPage] == index
//         ? "[<s>///</s>]"
//         : "[   ]";

//     return $"{mark} {letter}) {text}";
// }

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
        Debug.LogError($"JSON okunamadı: {exception.Message}", this);
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
                item.options, item.correctAnswer
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
            0, categoryPool.Count
        );

        examQuestions.Add(categoryPool[randomIndex]);
    }

    // Seçilen soruların sayfa sırasını karıştır.
    for (int i = examQuestions.Count - 1; i > 0; i--)
    {
        int randomIndex = UnityEngine.Random.Range(0, i + 1);

        Question temporary = examQuestions[i];
        examQuestions[i] = examQuestions[randomIndex];
        examQuestions[randomIndex] = temporary;
    }

    questions = examQuestions.ToArray();
    currentPage = 0;

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