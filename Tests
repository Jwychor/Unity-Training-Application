using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data.SqlClient;
using System.Data;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
public class TrainerModeTestsController : MonoBehaviour
{
    private readonly string connString = Application.isEditor ? trainerMode.TrainerModeNamespace.homeConnString :
        trainerMode.TrainerModeNamespace.connString;

    private List<string> departmentList = new List<string>();
    private List<string> departmentIDList = new List<string>();
    private List<string> currentTestsList = new List<string>();
    private List<string> currentTestsIDList = new List<string>();
    private List<string> currentEmployeeList = new List<string>();
    private List<string> currentTrainerList = new List<string>();

    private DataTable employeeDataTable = new DataTable();
    private DataTable jobDataTable = new DataTable();
    private DataTable trainerDataTable = new DataTable();
    private DataTable trainingHistoryDataTable = new DataTable();

    private string currentDepartment;
    private string currentDepartmentID;
    private string currentJobID;
    private string currentTestID;
    private string currentEmployee;
    private string currentTrainer;

    private float numberCorrect = 0;
    private float percentCorrect = 0;
  
    //Memory of answers
    List<int> answersList = new List<int>();

    //List of which questions were wrong
    List<int> answersWrong = new List<int>();

    //Answer key updating based on questionList
    List<int> answerIndexWrong = new List<int>();

    //Answer key updating based on questionList
    List<int> answerKey = new List<int>();

    //Dropdowns
    public Dropdown departmentDropdown;
    public Dropdown jobDropdown;
    public Dropdown employeeDropdown;
    public Dropdown trainerDropdown;

    public Button startButton;
    public Button endButton;
    public Button cancelButton;
    public Button forward;
    public Button backward;
    public Button resultsReviewButton;
    public Button clearResultsButton;
    public Button serverUpdateRetryButton;
    public Button reviewResultsButton;
    public Button hideResultsButton;

    private Toggle answer1;
    private Toggle answer2;
    private Toggle answer3;
    private Toggle answer4;

    public GameObject jobTestList;
    private GameObject selectedQuestion;
    private GameObject questionList;

    public Text successfulUpdateText;
    public Text testNotFinished;
    public Text resultsText;
    public Text queryExceptionError;
    public Text wrongResults;
    public Text connectionExceptionDetails;

    public GameObject connectionError;

    private string wrongDisplay = "";
    private void Awake()
    {
        DataLoad();
        for (int i = 0; i < jobDataTable.Rows.Count; i++)
        {
            if (departmentList.Contains(jobDataTable.Rows[i]["Department_Name"].ToString()))
            {
                continue;
            }
            departmentList.Add(jobDataTable.Rows[i]["Department_Name"].ToString());
            departmentIDList.Add(jobDataTable.Rows[i]["Department_ID"].ToString());
        }
        departmentDropdown.AddOptions(departmentList);
    }

    private void DataLoad()
    {
        employeeDataTable.Clear();
        jobDataTable.Clear();
        trainerDataTable.Clear();
        trainingHistoryDataTable.Clear();

        using (SqlConnection sqlconn = new SqlConnection(connString))
        {
            try
            {
                sqlconn.Open();
                PlayerPrefs.SetInt("Connection", 1);
                connectionError.gameObject.SetActive(false);
            }
            catch (Exception e)
            {
                connectionError.gameObject.SetActive(true);
                PlayerPrefs.SetInt("Connection", 0);
                connectionExceptionDetails.text = e.ToString();
                return;
            }
            using (SqlCommand cmd = sqlconn.CreateCommand())
            {
                cmd.CommandText = trainerMode.TrainerModeNamespace.employeeDataTableQuery;
                cmd.CommandType = CommandType.Text;
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(employeeDataTable);
                }
                cmd.CommandText = trainerMode.TrainerModeNamespace.jobDataTableQuery;
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(jobDataTable);
                }
                cmd.CommandText = trainerMode.TrainerModeNamespace.trainerDataTableQuery;
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(trainerDataTable);
                }
                cmd.CommandText = trainerMode.TrainerModeNamespace.trainingHistoryDataTableQuery;
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(trainingHistoryDataTable);
                }
            }
            sqlconn.Close();
        }
    }

    private void Start()
    {
        DataLoad();
        startButton.onClick.AddListener(StartButtonBehavior);
        startButton.onClick.AddListener(TestBehavior);
        cancelButton.onClick.AddListener(EndButtonBehavior);
        endButton.onClick.AddListener(EndButtonBehavior);
        endButton.onClick.AddListener(Results);
        forward.onClick.AddListener(QuestionIndexBehavior);
        forward.onClick.AddListener(TestBehavior);
        backward.onClick.AddListener(QuestionIndexBehavior);
        backward.onClick.AddListener(TestBehavior);
        departmentDropdown.onValueChanged.AddListener(ListsPopulate);
        jobDropdown.onValueChanged.AddListener(delegate { DropdownChange(); });
        employeeDropdown.onValueChanged.AddListener(delegate { DropdownChange(); });
        trainerDropdown.onValueChanged.AddListener(delegate { DropdownChange(); });
        resultsReviewButton.onClick.AddListener(QuestionReview);
        clearResultsButton.onClick.AddListener(HideResults);
        serverUpdateRetryButton.onClick.AddListener(UpdateServer);
    }

    private void ListsPopulate(int a)
    {
        if (PlayerPrefs.GetInt("Connection") == 0)
        {
            return;
        }
        currentTestsList.Clear();
        currentTestsIDList.Clear();
        currentEmployeeList.Clear();
        currentTrainerList.Clear();

        jobDropdown.ClearOptions();
        trainerDropdown.ClearOptions();
        employeeDropdown.ClearOptions();

        if (departmentDropdown.value != 0)
        {
            jobDropdown.options.Add(new Dropdown.OptionData("Select a Job"));
            employeeDropdown.options.Add(new Dropdown.OptionData("Select an Employee"));
            trainerDropdown.options.Add(new Dropdown.OptionData("Select a Trainer/Supervisor"));

            currentDepartment = departmentDropdown.options[departmentDropdown.value].text;
            currentDepartmentID = departmentIDList[departmentDropdown.value - 1];

            for (int i = 0; i < jobDataTable.Rows.Count; i++)
            {
                if (currentDepartmentID == jobDataTable.Rows[i]["Department_ID"].ToString() && jobDataTable.Rows[i]["Requirement_Type_ID"].ToString() == "2")
                {
                    currentTestsList.Add(jobDataTable.Rows[i]["Training_Task"].ToString());
                    currentTestsIDList.Add(jobDataTable.Rows[i]["Training_Task_ID"].ToString());
                }
            }

            for (int i = 0; i < employeeDataTable.Rows.Count; i++)
            {
                if (currentDepartmentID == employeeDataTable.Rows[i]["Department_ID"].ToString())
                {
                    currentEmployeeList.Add(employeeDataTable.Rows[i]["Employee_Clock_Number"].ToString());
                }
            }

            for (int i = 0; i < trainerDataTable.Rows.Count; i++)
            {
                if (currentDepartmentID == trainerDataTable.Rows[i]["Department_ID"].ToString())
                {
                    currentTrainerList.Add(trainerDataTable.Rows[i]["Trainer"].ToString());
                }
            }

            employeeDropdown.AddOptions(currentEmployeeList);
            jobDropdown.AddOptions(currentTestsList);
            trainerDropdown.AddOptions(currentTrainerList);
        }
    }

    private void DropdownChange()
    {
        //*Needs to have (Requirement type == 2) logic to select the test that was selected's task ID to turn on the correct test
        DataRow[] dr = jobDataTable.Select($"Training_Task = '{jobDropdown.options[jobDropdown.value].text}' and Requirement_Type_ID = '2'");
        currentTestID = dr[0]["Training_Task_ID"].ToString();
        currentJobID = dr[0]["Training_Job_ID"].ToString();

        currentEmployee = employeeDropdown.options[employeeDropdown.value].text;

        currentTrainer = trainerDropdown.options[trainerDropdown.value].text;
        
        StartCoroutine(BeginButtonInteractable());
    }
    IEnumerator BeginButtonInteractable()
    {
        yield return new WaitForFixedUpdate();

        if (departmentDropdown.value != 0 && jobDropdown.value != 0 && trainerDropdown.value != 0 && employeeDropdown.value !=0 && employeeDropdown.options[employeeDropdown.value].text != trainerDropdown.options[trainerDropdown.value].text)
        {
            startButton.interactable = true;
        }
        else
        {
            startButton.interactable = false;
        }
    }
    private void StartButtonBehavior()
    {
        if (PlayerPrefs.GetInt("Connection") == 0 || queryExceptionError.gameObject.activeSelf == true)
        {
            return;
        }
        for (int i = 0; i < jobTestList.transform.childCount; i++)
        {
            if(i == 0)
            {
                continue;
            }
            if (jobTestList.transform.GetChild(i).GetComponent<Text>().text.ToString() == currentJobID)
            {
                questionList = jobTestList.transform.GetChild(i).gameObject;
                break;
            }
        }
        //Turn off all question toggles, change colors back to white, make them interactable
        for (int i = 0; i < questionList.transform.childCount; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Toggle currentAnswer = questionList.transform.GetChild(i).transform.GetChild(2).transform.GetChild(j).GetComponent<Toggle>();
                currentAnswer.isOn = false;
                currentAnswer.interactable = true;
                ColorBlock cb = currentAnswer.colors;
                cb.normalColor = Color.white;
            }
            questionList.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = "Question " + (i + 1).ToString() + "/" + questionList.transform.childCount.ToString();
        }

        //Change to make button void if employee and trainer dropdowns are on index 0 as well
        numberCorrect = 0;
        resultsText.transform.parent.gameObject.SetActive(false);
        connectionError.gameObject.SetActive(false);
        testNotFinished.gameObject.SetActive(false);

        departmentDropdown.interactable = false;
        jobDropdown.interactable = false;
        employeeDropdown.interactable = false;
        trainerDropdown.interactable = false;

        forward.gameObject.SetActive(true);
        startButton.interactable = false;
        cancelButton.interactable = true;
        endButton.interactable = true;
        reviewResultsButton.interactable = false;
        hideResultsButton.interactable = false;
        jobTestList.gameObject.SetActive(true);

        for (int i = 0; i < questionList.transform.childCount; i++)
        {
            questionList.transform.GetChild(i).gameObject.SetActive(false);
        }
        questionList.gameObject.SetActive(true);
        questionList.transform.GetChild(0).gameObject.SetActive(true);

        answersList.Clear();
        answersWrong.Clear();
        answerKey.Clear();

        for (int i = 0; i < questionList.transform.childCount; i++)
        {
            answersList.Add(0);
            answersWrong.Add(0);
            answerKey.Add(int.Parse(questionList.transform.GetChild(i).transform.GetChild(3).gameObject.GetComponent<Text>().text));
        }
    }

    private void EndButtonBehavior()
    {
        //Don't execute if test isn't finished and Submitbutton was hit
        if (EventSystem.current.currentSelectedGameObject.gameObject == endButton.gameObject)
        {
            for (int i = 0; i < answersList.Count; i++)
            {
                if (answersList[i] == 0)
                {
                    testNotFinished.gameObject.SetActive(true);
                    return;
                }
            }
        }
        //Reset objects
        connectionExceptionDetails.gameObject.SetActive(false);
        testNotFinished.gameObject.SetActive(false);
        connectionError.gameObject.SetActive(false);
        forward.gameObject.SetActive(false);
        backward.gameObject.SetActive(false);
        startButton.interactable = true;
        endButton.interactable = false;
        cancelButton.interactable = false;

        //Set dropdowns to be interactable
        departmentDropdown.interactable = true;
        jobDropdown.interactable = true;
        employeeDropdown.interactable = true;
        trainerDropdown.interactable = true;
        DisableAllQuestions();
    }

    private void QuestionIndexBehavior()
    {
        //Move to the next question
        for (int i = 0; i < questionList.transform.childCount; i++)
        {
            if (questionList.transform.GetChild(i).gameObject.activeSelf == true)
            {
                questionList.transform.GetChild(i).gameObject.SetActive(false);
                if(EventSystem.current.currentSelectedGameObject == forward.gameObject)
                {
                    questionList.transform.GetChild(i + 1).gameObject.SetActive(true);
                    if (i == questionList.transform.childCount - 2)
                    {
                        forward.gameObject.SetActive(false);
                    }
                    backward.gameObject.SetActive(true);
                    testNotFinished.gameObject.SetActive(false);
                    break;
                }
                else
                {
                    questionList.transform.GetChild(i - 1).gameObject.SetActive(true);
                    if(i == 1)
                    {
                        backward.gameObject.SetActive(false);
                    }
                    forward.gameObject.SetActive(true);
                    testNotFinished.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    //TEST BEHAVIORS
    private void TestBehavior()
    {
        //Change to include all dropdowns
        if (departmentDropdown.value != 0 || jobDropdown.value != 0 || employeeDropdown.value == 0 || trainerDropdown.value == 0)
        {
            for (int i = 0; i < questionList.transform.childCount; i++)
            {
                if (questionList.transform.GetChild(i).gameObject.activeSelf == true)
                {
                    selectedQuestion = questionList.transform.GetChild(i).gameObject;
                    break;
                }
            }

            answer1 = selectedQuestion.transform.GetChild(2).transform.GetChild(0).gameObject.GetComponent<Toggle>();
            answer2 = selectedQuestion.transform.GetChild(2).transform.GetChild(1).gameObject.GetComponent<Toggle>();
            answer3 = selectedQuestion.transform.GetChild(2).transform.GetChild(2).gameObject.GetComponent<Toggle>();
            answer4 = selectedQuestion.transform.GetChild(2).transform.GetChild(3).gameObject.GetComponent<Toggle>();

            answer1.onValueChanged.RemoveAllListeners();
            answer2.onValueChanged.RemoveAllListeners();
            answer3.onValueChanged.RemoveAllListeners();
            answer4.onValueChanged.RemoveAllListeners();

            answer1.onValueChanged.AddListener(delegate { AnswerChange(answer1); });
            answer2.onValueChanged.AddListener(delegate { AnswerChange(answer2); });
            answer3.onValueChanged.AddListener(delegate { AnswerChange(answer3); });
            answer4.onValueChanged.AddListener(delegate { AnswerChange(answer4); });
        }
    }

    private void AnswerChange(bool a)
    {
        testNotFinished.gameObject.SetActive(false);

        if (answer1.isOn == true)
        {
            answersList[selectedQuestion.transform.GetSiblingIndex()] = 1;
        }
        else if (answer2.isOn == true)
        {
            answersList[selectedQuestion.transform.GetSiblingIndex()] = 2;
        }
        else if (answer3.isOn == true)
        {
            answersList[selectedQuestion.transform.GetSiblingIndex()] = 3;
        }
        else if(answer4.isOn == true)
        {
            answersList[selectedQuestion.transform.GetSiblingIndex()] = 4;
        }
        else
        {
            answersList[selectedQuestion.transform.GetSiblingIndex()] = 0;
        }
    }
    private void Results()
    {
        //Don't execute if test isn't finished
        for (int i = 0; i < answersList.Count; i++)
        {
            if (answersList[i] == 0)
            {
                testNotFinished.gameObject.SetActive(true);
                return;
            }
        }
        testNotFinished.gameObject.SetActive(false);
        connectionExceptionDetails.gameObject.SetActive(false);
        queryExceptionError.gameObject.SetActive(false);

        //Compare and count the number of correct answers and which were wrong
        for (int i = 0; i < answersList.Count; i++)
        {
            if (answersList[i] == answerKey[i])
            {
                numberCorrect += 1;
            }
            else
            {
                answersWrong[i] = 1;
            }
        }
        answerIndexWrong = new List<int>((int)(answersList.Count - numberCorrect));

        for (int i = 0; i < answersWrong.Count; i++)
        {
            if (answersWrong[i] == 1)
            {
                answerIndexWrong.Add(i + 1);
            }
        }

        percentCorrect = Mathf.Round((numberCorrect / answersList.Count) * 100);

        resultsText.text = numberCorrect + "/" + answersList.Count + "   " + percentCorrect.ToString() + "% Correct";

        wrongDisplay = "";

        for (int i = 0; i < answerIndexWrong.Count; i++)
        {
            if (i != answerIndexWrong.Count - 1)
            {
                wrongDisplay += answerIndexWrong[i] + ", ";
            }
            else
            {
                wrongDisplay += answerIndexWrong[i] + ". ";
                if(percentCorrect == 100)
                {
                    wrongDisplay += "Great job!";
                }
                else
                {
                    wrongDisplay += "Please retake this test at least one week from today.";
                }
            }
        }
        if(percentCorrect != 100)
        {
            wrongResults.text = wrongDisplay.Insert(0, "Questions Wrong: ");
        }

        //Display results
        resultsText.transform.parent.gameObject.SetActive(true);
        reviewResultsButton.interactable = true;
        hideResultsButton.interactable = true;
        UpdateServer();
    }

    private void UpdateServer()
    {
        //Submit results to server
        using (SqlConnection sqlconn = new SqlConnection(connString))
        {
            try
            {
                sqlconn.Open();
                connectionError.SetActive(false);
            }
            catch(Exception e)
            {
                connectionError.SetActive(true);
                PlayerPrefs.SetInt("Connection", 0);
                connectionExceptionDetails.text = e.ToString();
                return;
            }
            using (SqlCommand cmd = sqlconn.CreateCommand())
            {
                Debug.Log(currentJobID.ToString() + currentTestID.ToString() + currentEmployee + currentTrainer + percentCorrect.ToString());
                string updateString = "INSERT INTO Training_History (Training_Job_ID, Training_Task_ID, Trainee, Trainer, Training_Date, Training_Result_PF, Training_Score) " +
                    "VALUES (@Job_ID, @Task_ID, @Trainee_ID, @Trainer_ID, @Training_Date, @Training_Result_PF,@Training_Score)";
                cmd.CommandText = updateString;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Job_ID", currentJobID);
                cmd.Parameters.AddWithValue("@Task_ID", currentTestID);
                cmd.Parameters.AddWithValue("@Trainee_ID", currentEmployee);
                cmd.Parameters.AddWithValue("@Trainer_ID", currentTrainer);
                cmd.Parameters.AddWithValue("@Training_Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@training_Result_PF", percentCorrect == 100f ?  "P" : "F");
                cmd.Parameters.AddWithValue("@training_Score", percentCorrect);
                cmd.CommandType = CommandType.Text;
                try
                {
                    cmd.ExecuteNonQuery();
                    StartCoroutine(UpdateText());
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    serverUpdateRetryButton.transform.parent.gameObject.SetActive(true);
                    return;
                }
            }
            sqlconn.Close();
            DataLoad();
        }
    }

    IEnumerator UpdateText()
    {
        successfulUpdateText.color = new Color(.21f, .92f, .14f, 1);
        for (float i = 3; i >= 0; i -= Time.deltaTime / 2)
        {
            // set color with i as alpha
            successfulUpdateText.color = new Color(.21f, .92f, .14f, i);
            yield return null;
        }
    }

    private void DisableAllQuestions()
    {
        for (int i = 0; i < questionList.transform.childCount; i++)
        {
            questionList.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    private void QuestionReview()
    {
        DisableAllQuestions();
        for (int i = 0; i < questionList.transform.childCount; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Toggle currentAnswer = questionList.transform.GetChild(i).transform.GetChild(2).transform.GetChild(j).GetComponent<Toggle>();
                currentAnswer.interactable = false;
                var cb = currentAnswer.colors;
                cb.disabledColor = Color.white;
                currentAnswer.colors = cb;

                if (j + 1 == answerKey[i])
                {
                    cb.disabledColor = Color.green;
                    currentAnswer.colors = cb;
                }
                else if(currentAnswer.isOn && j + 1 != answerKey[i])
                {
                    cb.disabledColor = Color.red;
                    currentAnswer.colors = cb;
                }
            }
        }
        resultsText.transform.parent.gameObject.SetActive(false);
        wrongResults.gameObject.SetActive(false);
        questionList.gameObject.SetActive(true);
        questionList.transform.GetChild(0).gameObject.SetActive(true);
        forward.gameObject.SetActive(true);
    }
    private void HideResults()
    {
        for(int i = 0; i < questionList.transform.childCount; i++)
        {
            questionList.transform.GetChild(i).gameObject.SetActive(false);
        }
        resultsText.transform.parent.gameObject.SetActive(false);
        wrongResults.gameObject.SetActive(false);
        forward.gameObject.SetActive(false);
        backward.gameObject.SetActive(false);
    }
}
