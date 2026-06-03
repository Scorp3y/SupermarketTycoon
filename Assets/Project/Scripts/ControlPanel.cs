using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlPanel : MonoBehaviour
{
    public GameObject settingPanel;
    public MainCamera cameraController;
    void Start()
    {

        settingPanel.SetActive(false);

        if (cameraController == null)
        {
            cameraController = Camera.main.GetComponent<MainCamera>(); 
        }
    }

    void LockCamera(bool locked)
    {
        if (cameraController != null)
        {
            cameraController.enabled = !locked; 
        }
    }

    public void OpenPanelStore()
    {

        settingPanel.SetActive(false);
        LockCamera(true);
    }

    public void ExitPanelStore()
    {

        settingPanel.SetActive(false);
        LockCamera(false);
    }

    public void TransitionStore()
    {

        settingPanel.SetActive(false);
        LockCamera(true);
    }

    public void TransitionBuild()
    {
        settingPanel.SetActive(false);
        LockCamera(true);
    }

    public void OpenPanelWarehouse()
    {

        settingPanel.SetActive(false);
        LockCamera(true);
    }

    public void ExitWarehouse() 
    { 
        settingPanel.SetActive(false);
        LockCamera(false);
    }

    public void OpenSetting()
    {
       settingPanel.SetActive(true);
        LockCamera(true);
    }

    public void ExitSetting ()
    {
        settingPanel.SetActive(false);
        LockCamera(false);
    }

    public void BackMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void ExitGame()
    {
        Application.Quit();
    }

}
