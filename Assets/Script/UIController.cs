using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject StartUI;

    public GameObject GameUI;

    public GameObject EndUI;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ActiveEndUI();
        }
    }

    public void ReturnMainUI()
    {
        SetActiveState_UI(true,false,false);
        //ScoreController.Instance.EndGame();
    }
    public void StartEasyGameUI()
    {
        SetActiveState_UI(false, true, false);
        ScoreController.Instance.StartEasyGame();
    }
    public void StartNormalGameUI()
    {
        SetActiveState_UI(false, true, false);
        ScoreController.Instance.StartNormalGame();
    }
    public void StartHardGameUI()
    {
        SetActiveState_UI(false, true, false);
        ScoreController.Instance.StartHardGame();
    }
    public void ActiveEndUI()
    {
        SetActiveState_UI(false, false, true);
        ScoreController.Instance.EndGame();
    }


    private void SetActiveState_UI(bool Start=false,bool Game=false,bool End = false)
    {
        StartUI.SetActive(Start);
        GameUI.SetActive(Game);
        EndUI.SetActive(End);
    }
    public void ExitClick()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
