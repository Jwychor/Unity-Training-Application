using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainerMaterialsController : MonoBehaviour
{
    //This script takes names from scene objects, so name the
    //departments/jobs what you wnat them to be displayed as in
    //the editor

    public Transform departmentContainer;
    private Transform currentDepartment;

    public Toggle learningObjectivesToggle;
    public Toggle trainingGuidesToggle;

    private RectTransform currentRect;

    public Dropdown departmentDropdown;
    public Dropdown jobDropdown;

    private List<string> dropdownTextList = new List<string>();

    public Text departmentOverview;

    private void Awake()
    {
        for (int i = 0; i < departmentContainer.childCount; i++)
        {
            if(i == departmentContainer.childCount - 1)
            {
                break;
            }
            dropdownTextList.Add(departmentContainer.GetChild(i).name);
        }
        departmentDropdown.AddOptions(dropdownTextList);
        dropdownTextList.Clear();

        departmentDropdown.onValueChanged.AddListener(JobDropdownFill);
        jobDropdown.onValueChanged.AddListener(delegate { TextEnabler(); });
        trainingGuidesToggle.onValueChanged.AddListener(delegate { TextEnabler(); });
    }

    private void JobDropdownFill(int index)
    {
        for(int i = 0; i < departmentContainer.childCount; i++)
        {
            if(i == departmentContainer.childCount - 1)
            {
                break;
            }
            departmentContainer.GetChild(i).gameObject.SetActive(false);
        }
        if(index != 0)
        {
            //Turn on Training Overview text
            departmentOverview.gameObject.SetActive(true);

            //Store the current department selected
            currentDepartment = departmentContainer.GetChild(index - 1);

            //Fill the dropdown with the names of the objects
            jobDropdown.ClearOptions();
            jobDropdown.options.Add(new Dropdown.OptionData() {text = "Select a Job" });

            for (int i = 0; i < currentDepartment.childCount; i++)
            {
                dropdownTextList.Add(currentDepartment.GetChild(i).name);
            }
            jobDropdown.AddOptions(dropdownTextList);
            dropdownTextList.Clear();
        }
    }

    private void TextEnabler()
    {
        //Turn Off the Department Overview text
        departmentOverview.gameObject.SetActive(false);

        //If job dropdown is on select job, do not run
        if (jobDropdown.value == 0)
        {
            return;
        }
        //Turn off all departments then turn on the current department
        for (int i = 0; i < departmentContainer.childCount; i++)
        {
            departmentContainer.GetChild(i).gameObject.SetActive(false);
        }
        currentDepartment.gameObject.SetActive(true);

        int index = jobDropdown.value - 1;
        //Enable only that job

        for (int i = 0; i < currentDepartment.childCount; i++)
        {
            currentDepartment.GetChild(i).gameObject.SetActive(false);
        }
        currentDepartment.GetChild(index).gameObject.SetActive(true);

        //Enable Learning Objectives or Training Guide depending on toggle state
        //and set set the scroll bar to the current text
        if (learningObjectivesToggle.isOn == true)
        {
            currentDepartment.GetChild(index).GetChild(0).gameObject.SetActive(true);
            currentDepartment.GetChild(index).GetChild(1).gameObject.SetActive(false);
            currentRect = currentDepartment.GetChild(index).GetChild(0).gameObject.GetComponent<RectTransform>();
            currentDepartment.GetComponent<ScrollRect>().content = currentRect;
        }
        else
        {
            currentDepartment.GetChild(index).GetChild(0).gameObject.SetActive(false);
            currentDepartment.GetChild(index).GetChild(1).gameObject.SetActive(true);
            currentRect = currentDepartment.GetChild(index).GetChild(1).gameObject.GetComponent<RectTransform>();
            currentDepartment.GetComponent<ScrollRect>().content = currentRect;
        }
    }
}
