using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using UnityEngine.UI.Extensions;
using System.Linq;

public class TrainerModeChecklistsController : MonoBehaviour
{
    //Connect to internal database if in editor otherwise connect to production database.
    private readonly string connString = Application.isEditor ? trainerMode.TrainerModeNamespace.homeConnString :
        trainerMode.TrainerModeNamespace.connString;

    private DataTable employeeDataTable = new DataTable();
    private DataTable jobDataTable = new DataTable();
    private DataTable trainerDataTable = new DataTable();
    private DataTable trainingHistoryDataTable = new DataTable();

    public Transform togglePrefab;
    public Transform clone;

    private string currentDepartment;
    private string currentDepartmentID;
    private int currentEmployee;
    private int currentTrainer;

    public GameObject checklistList;

    public GameObject trainedOrIntroduced;

    public Dropdown departmentDropdown;
    public Dropdown jobDropdown;
    public Dropdown employeeDropdown;
    public Dropdown trainerDropdown;

    public Scrollbar scrollbar;

    public Button beginButton;
    public Button submitButton;

    public Text incompleteFieldsError;
    public Text checklistOverloadError;
    public Text connectionError;
    public Text connectionErrorText;
    public Text debug;
    public Text successfulUpdateText;

    public Button serverUpdateRetryButton;

    private List<string> departmentList = new List<string>();
    private List<string> departmentIDList = new List<string>();

    List<bool> introducedBoolList = new List<bool>();
    List<bool> introducedBoolListUpdate = new List<bool>();

    List<bool> trainedBoolList = new List<bool>();
    List<bool> trainedBoolListUpdate = new List<bool>();

    List<string> currentEmployeeList = new List<string>();
    List<string> currentTrainerList = new List<string>();
    List<string> currentJobList = new List<string>();
    List<string> currentJobIDList = new List<string>();
    List<string> currentTaskNameList = new List<string>();
    List<int> currentTaskList = new List<int>();

    //METHODS
    private void Awake()
    {
        //When the scene begins, load data from the server and fill in dropdowns with information.
        DataLoad();
        for (int i = 0; i < jobDataTable.Rows.Count; i++)
        {
            if (departmentList.Contains(jobDataTable.Rows[i]["Department_Name"]))
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
        //Make sure datatables are empty
        employeeDataTable.Clear();
        jobDataTable.Clear();
        trainerDataTable.Clear();
        trainingHistoryDataTable.Clear();
        
        //Attempt to connect to the server. If its you can't, log the error.
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
                connectionErrorText.text = e.ToString();
                return;
            }
            //Fill datatables with corresponding information
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
    
    //After relevant objects are loaded from the Awake() method, the Start() method runs
    private void Start()
    {
        DataLoad();
        trainedOrIntroduced.gameObject.SetActive(false);
       
        //Make buttons listen for methods
        departmentDropdown.onValueChanged.AddListener(DropdownPopulate);
        jobDropdown.onValueChanged.AddListener(delegate { ChecklistPopulate(); });
        employeeDropdown.onValueChanged.AddListener(delegate { ChecklistPopulate(); });
        trainerDropdown.onValueChanged.AddListener(delegate { ChecklistPopulate(); });
        submitButton.onClick.AddListener(ChecklistUpdate);
        beginButton.onClick.AddListener(CreateChecklists);
        serverUpdateRetryButton.onClick.AddListener(ChecklistUpdate);
    }
    
    //When the department is chosen, fill in the appropriate dropdowns with jobs, employees, and trainers in that department
    private void DropdownPopulate(int a)
    {
        if (PlayerPrefs.GetInt("Connection") == 0)
        {
            return;
        }
        incompleteFieldsError.gameObject.SetActive(false);
        trainedOrIntroduced.gameObject.SetActive(false);

        currentJobList.Clear();
        currentJobIDList.Clear();
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
                if (currentDepartmentID == jobDataTable.Rows[i]["Department_ID"].ToString() && !currentJobIDList.Contains(jobDataTable.Rows[i]["Training_Job_ID"].ToString()))
                {
                    currentJobList.Add(jobDataTable.Rows[i]["Training_Job_Name"].ToString());
                    currentJobIDList.Add(jobDataTable.Rows[i]["Training_Job_ID"].ToString());
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
            jobDropdown.AddOptions(currentJobList);
            trainerDropdown.AddOptions(currentTrainerList);
        }
    }
    
    //Create the checklist when the go button is hit
    private void ChecklistPopulate()
    {
        //Do not run if connection did not work
        if (PlayerPrefs.GetInt("Connection") == 0)
        {
            return;
        }
        checklistOverloadError.gameObject.SetActive(false);

        incompleteFieldsError.gameObject.SetActive(false);
        submitButton.interactable = false;

        //Destroy any current checklists
        for (int i = 0; i < checklistList.transform.childCount; i++)
        {
            Destroy(checklistList.transform.GetChild(i).gameObject);
        }
        StartCoroutine(BeginButtonInteractable());
        trainedOrIntroduced.gameObject.SetActive(false);
    }
    
    //Make the begin button interactable on the next frame if all relevant fields are filled out
    IEnumerator BeginButtonInteractable()
    {
        yield return new WaitForFixedUpdate();

        if (departmentDropdown.value != 0 && jobDropdown.value != 0 && trainerDropdown.value != 0 && employeeDropdown.value != 0 && checklistList.transform.childCount == 0 && employeeDropdown.options[employeeDropdown.value].text != trainerDropdown.options[trainerDropdown.value].text)
        {
            beginButton.interactable = true;
        }
        else
        {
            beginButton.interactable = false;
        }
    }
    
    //Instantiate the checklists
    private void CreateChecklists()
    {
        beginButton.interactable = false;
        
        //Clear current name containers
        currentTaskNameList.Clear();
        currentTaskList.Clear();
        
        //Loop through rows in the job table and add them if they are in the department
        for (int i = 0; i < jobDataTable.Rows.Count; i++)
        {
            if (jobDataTable.Rows[i]["Training_Job_Name"].ToString() == jobDropdown.options[jobDropdown.value].text && jobDataTable.Rows[i]["Requirement_Type_ID"].ToString() == "1")
            {
                currentTaskNameList.Add(jobDataTable.Rows[i]["Training_Task"].ToString());
                currentTaskList.Add(int.Parse(jobDataTable.Rows[i]["Training_Task_ID"].ToString()));
            }
        }

        checklistOverloadError.gameObject.SetActive(false);

        trainedBoolList.Clear();
        trainedBoolListUpdate.Clear();
        introducedBoolList.Clear();
        introducedBoolListUpdate.Clear();

        currentEmployee = int.Parse(employeeDropdown.options[employeeDropdown.value].text);
        currentTrainer = int.Parse(trainerDropdown.options[trainerDropdown.value].text);

        //For each checklist, check only the last time they have been checked the last time then check the box if they were,
        //leave it unchecked if not
        trainingHistoryDataTable.Columns["Training_Date"].DataType = Type.GetType("System.DateTime");
        trainingHistoryDataTable.DefaultView.Sort = "Training_Date DESC";
        DataRow[] dr = trainingHistoryDataTable.Select($"Trainee = '{currentEmployee.ToString()}' and Training_Job_Name = '{jobDropdown.options[jobDropdown.value].text}' and Requirement_Type_ID = '1'","Training_Date DESC");

        for (int i = 0; i < currentTaskList.Count; i++)
        {
            //Check create a list of whether the latest record see that the person was trained
            bool introduced = false;
            bool trained = false;
            bool added = false;
            for (int j = 0; j < dr.Length; j++)
            {
                if (dr[j]["Training_Task_ID"].ToString() == currentTaskList[i].ToString())
                {
                    if(dr[j]["Training_Result_PF"].ToString() == "P")
                    {
                        added = true;
                        trained = true;
                    }
                    if(dr[j]["Training_Introduction_PF"].ToString() == "P")
                    {
                        added = true;
                        introduced = true;
                    }
                    if(introduced == true || trained == true)
                    {
                        TrainedBoolListFiller(introduced, trained);
                        break;
                    }
                }
            }
            if (added == false)
            {
                TrainedBoolListFiller(false,false);
            }
        }
        
        //Instantiate the checklist based on the previous information
        for (int i = 0; i < currentTaskList.Count; i++)
        {
            clone = Instantiate(togglePrefab, new Vector3(140, 1368 + (i * -125), 0), Quaternion.identity).transform;
            clone.SetParent(checklistList.transform);

            clone.transform.GetChild(1).gameObject.GetComponent<Text>().text = currentTaskNameList[i];

            //Have the toggles be ON/OFF depending on the database information
            if(introducedBoolList[i] == false)
            {
                clone.GetChild(2).GetComponent<Toggle>().isOn = false;
            }
            else
            {
                clone.GetChild(2).GetComponent<Toggle>().isOn = true;
            }
            if (trainedBoolList[i] == false)
            {
                clone.gameObject.GetComponent<Toggle>().isOn = false;
            }
            else
            {
                clone.gameObject.GetComponent<Toggle>().isOn = true;
            }
            
            clone.gameObject.GetComponent<Toggle>().onValueChanged.AddListener(TrainedBoolUpdate);
            clone.GetChild(2).GetComponent<Toggle>().onValueChanged.AddListener(TrainedBoolUpdate);
        }
        trainedOrIntroduced.gameObject.SetActive(true);
        StartCoroutine(ScrollSet());
    }

    private void TrainedBoolListFiller(bool introduced, bool trained)
    {
        if (trained == true)
        {
            trainedBoolList.Add(true);
            trainedBoolListUpdate.Add(true);
        }
        else
        {
            trainedBoolList.Add(false);
            trainedBoolListUpdate.Add(false);
        }
        if(introduced == true)
        {
            introducedBoolList.Add(true);
            introducedBoolListUpdate.Add(true);
        }
        else
        {
            introducedBoolList.Add(false);
            introducedBoolListUpdate.Add(false);
        }
    }
    IEnumerator ScrollSet()
    {
        yield return new WaitForFixedUpdate();
        scrollbar.value = 1;
    }
    
    //When the checkmarks are clicked, update the list to reflect the changes
    private void TrainedBoolUpdate(bool t)
    {
        for (int i = 0; i < checklistList.transform.childCount; i++)
        {
            if (checklistList.transform.GetChild(i).gameObject.GetComponent<Toggle>().isOn == true)
            {
                trainedBoolListUpdate[i] = true;
            }
            else
            {
                trainedBoolListUpdate[i] = false;
            }
            if(checklistList.transform.GetChild(i).GetChild(2).GetComponent<Toggle>().isOn == true)
            {
                introducedBoolListUpdate[i] = true;
            }
            else
            {
                introducedBoolListUpdate[i] = false;
            }
        }
        if (!trainedBoolList.SequenceEqual(trainedBoolListUpdate) || !introducedBoolList.SequenceEqual(introducedBoolListUpdate))
        {
            submitButton.interactable = true;
        }
        else
        {
            submitButton.interactable = false;
        }
    }

    //When "update server" is clicked, save the information to the server
    private void ChecklistUpdate()
    {
        //If dropdown fields are incomplete do not update sql table
        if (departmentDropdown.value == 0 || jobDropdown.value == 0 || employeeDropdown.value == 0 || trainerDropdown.value == 0)
        {
            incompleteFieldsError.gameObject.SetActive(true);
            return;
        }
        //Otherwise, update the table
        else
        {
            connectionError.gameObject.SetActive(false);

            currentTrainer = int.Parse(trainerDropdown.options[trainerDropdown.value].text);
            currentEmployee = int.Parse(employeeDropdown.options[employeeDropdown.value].text);

            using (SqlConnection sqlconn = new SqlConnection(connString))
            {
                //Test the Connection
                try
                {
                    sqlconn.Open();
                }
                catch
                {
                    connectionError.gameObject.SetActive(true);
                    return;
                }
                //Insert new record Query
                string checklistUpdate = "INSERT INTO Training_History (Training_Job_ID, Training_Task_ID, Trainee, Trainer, Training_Date, Training_Introduction_PF, Training_Result_PF)" +
                    " VALUES (@Job_ID, @Task_ID, @Trainee_ID, @Trainer_ID, @Training_Date, @Training_Introduction_PF, @Training_Result_PF)";

                //Only update if the checkbox is different from before
                for (int i = 0; i < trainedBoolList.Count; i++)
                {
                    //Check if the box is different from its original value
                    if (trainedBoolList[i] != trainedBoolListUpdate[i] || introducedBoolList[i] != introducedBoolListUpdate[i])
                    {
                        using (SqlCommand cmd = sqlconn.CreateCommand())
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Job_ID", currentJobIDList[jobDropdown.value - 1]);
                            cmd.Parameters.AddWithValue("@Task_ID", currentTaskList[i]);
                            cmd.Parameters.AddWithValue("@Trainee_ID", currentEmployee);
                            cmd.Parameters.AddWithValue("@Trainer_ID", currentTrainer);
                            cmd.Parameters.AddWithValue("@Training_Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@Training_Introduction_PF", introducedBoolListUpdate[i] ? "P" : "F");
                            cmd.Parameters.AddWithValue("@Training_Result_PF", trainedBoolListUpdate[i] ? "P" : "F");
                            cmd.CommandText = checklistUpdate;
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
                    }
                }
                sqlconn.Close();
                StartCoroutine(UpdateText());
                ChecklistPopulate();
                DataLoad();
            }
        }
    }

    //Helper function for make "successful update" text fade away
    IEnumerator UpdateText()
    {
        successfulUpdateText.color = new Color(.21f, .92f, .14f, 1);
        for (float i = 2; i >= 0; i -= Time.deltaTime/2)
        {
            // set color with i as alpha
            successfulUpdateText.color = new Color(.21f, .92f, .14f, i);
            yield return null;
        }
    }
}
