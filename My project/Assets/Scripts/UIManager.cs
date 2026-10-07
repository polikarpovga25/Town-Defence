using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameManager game;

    public GameObject startScreen;
    public GameObject gameScreen;
    public GameObject resultScreen;

    public TMP_Text wheatText;
    public TMP_Text peasantsText;
    public TMP_Text warriorsText;
    public TMP_Text waveEnemiesText;
    public TMP_Text timerText;
    public TMP_Text logText;
    public TMP_Text resultTitleText;

    public Button hirePeasantButton;
    public Button hireWarriorButton;

    public Slider peasantSlider;
    public Slider warriorSlider;

    public void PressStart()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (gameScreen != null) gameScreen.SetActive(true);
        if (game != null) game.StartGame();
    }

    public void PressRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UpdateAllTexts(int w, int p, int war, int e)
    {
        if (wheatText != null) wheatText.text = "Пшеница: " + w;
        if (peasantsText != null) peasantsText.text = "Крестьяне: " + p;
        if (warriorsText != null) warriorsText.text = "Воины: " + war;
        if (waveEnemiesText != null) waveEnemiesText.text = "Врагов в волне: " + e;

        if (game != null)
        {
            if (hirePeasantButton != null) hirePeasantButton.interactable = (w >= game.peasantCost);
            if (hireWarriorButton != null) hireWarriorButton.interactable = (w >= game.warriorCost);
        }
    }

    public void UpdateTimerText(int seconds)
    {
        if (timerText != null) timerText.text = "До атаки: " + seconds + "с";
    }

    public void ShowLog(string text)
    {
        if (logText != null) logText.text = text;
    }

    public void EndGame(bool win)
    {
        if (gameScreen != null) gameScreen.SetActive(false);
        if (resultScreen != null) resultScreen.SetActive(true);

        if (resultTitleText != null)
        {
            if (win) resultTitleText.text = "ПОБЕДА! Город защищен!";
            else resultTitleText.text = "ПОРАЖЕНИЕ! Город уничтожен...";
        }
    }
}
