using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour
{
    public bool _isClear    = false;
    public int  _stageCount = 0;

    public TextMeshProUGUI ResultText;
    public TextMeshProUGUI StageCount;

    private void OnEnable()
    {
        Time.timeScale = 0;

        if (_isClear == true)
        {
            ResultText.SetText("Clear!!");
        }
        else
        {
            ResultText.SetText("Failed");
        }
        StageCount.SetText("Stage : " + _stageCount.ToString());
    }

    public void Title()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("TitleScene");
    }

    public void Exit()
    {
        Time.timeScale = 1;
        Application.Quit();
    }

    public void Cleared(bool clear, int stageCount)
    {
        _isClear    = clear;
        _stageCount = stageCount;
    }
}
