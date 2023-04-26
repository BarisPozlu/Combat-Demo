using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningHandler : MonoBehaviour
{
    [SerializeField] private GameObject[] marks;
    private GameObject mark;

    private void ChooseRandomMark()
    {
        mark = marks[Random.Range(0, marks.Length)];
    }

    public void ShowMark()
    {
        ChooseRandomMark();
        mark.SetActive(true);
    }

    public void HideMark()
    {
        mark.SetActive(false);
    }
}