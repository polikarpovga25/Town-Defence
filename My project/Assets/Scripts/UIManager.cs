using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameManager game;

    public GameObject startScreen;
    public GameObject gameScreen;
    public GameObject resultScreen;
    public GameObject pauseScreen;

    public TMP_Text wheatText;
    public TMP_Text peasantsText;
    public TMP_Text warriorsText;
    public TMP_Text waveEnemiesText;
    public TMP_Text timerText;
    public TMP_Text logText;
    public TMP_Text resultTitleText;

    public GameObject floatingTextPrefab;

    private int visualWheat = 0;
    private bool isGamePaused = false;
    private Coroutine wheatAnimationCoroutine;
    private Coroutine logAnimationCoroutine;
    private Vector2 logStartAnchoredPos;
    private bool hasStoredLogPos = false;

    public void PressStart()
    {
        if (startScreen != null) startScreen.SetActive(false);
        if (gameScreen != null) gameScreen.SetActive(true);
        if (pauseScreen != null) pauseScreen.SetActive(false);
        if (game != null) game.StartGame();
    }

    public void PressRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        
        if (pauseScreen != null) pauseScreen.SetActive(isGamePaused);
        
        if (isGamePaused)
        {
            Time.timeScale = 0f;
            if (game != null) game.SetPauseState(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (game != null) game.SetPauseState(false);
        }
    }

    public void InitAllTexts(int w, int p, int war, int e, int pCost, int wCost)
    {
        visualWheat = w;
        if (wheatText != null) wheatText.text = "Пшеница: " + visualWheat;
        UpdateAllTexts(w, p, war, e, pCost, wCost);
    }

    public void UpdateAllTexts(int w, int p, int war, int e, int pCost, int wCost)
    {
        if (peasantsText != null) peasantsText.text = "Крестьяне: " + p + " (Цена: " + pCost + ")";
        if (warriorsText != null) warriorsText.text = "Воины: " + war + " (Цена: " + wCost + ")";
        if (waveEnemiesText != null) waveEnemiesText.text = "Врагов в волне: " + e;
    }

    public void AnimateWheatChange(int targetWheat)
    {
        if (wheatAnimationCoroutine != null) StopCoroutine(wheatAnimationCoroutine);
        wheatAnimationCoroutine = StartCoroutine(SmoothWheatCount(targetWheat));
    }

    public void SpawnFloatingTextAt(Vector3 position, string text, Color color)
    {
        if (floatingTextPrefab == null || gameScreen == null) return;

        GameObject fText = Instantiate(floatingTextPrefab, gameScreen.transform);
        fText.transform.position = position;
        
        TMP_Text tmp = fText.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }
        StartCoroutine(FlyAndFadeUnitText(fText, 40f));
    }

    public void ShowLog(string text)
    {
        if (logText == null) return;

        RectTransform logRt = logText.GetComponent<RectTransform>();
        if (!hasStoredLogPos)
        {
            logStartAnchoredPos = logRt.anchoredPosition;
            hasStoredLogPos = true;
        }

        logText.text = text;
        logText.color = new Color(logText.color.r, logText.color.g, logText.color.b, 1f);
        logRt.anchoredPosition = logStartAnchoredPos;

        if (logAnimationCoroutine != null) StopCoroutine(logAnimationCoroutine);
        logAnimationCoroutine = StartCoroutine(FlyAndFadeLog(logRt, logText));
    }

    IEnumerator SmoothWheatCount(int target)
    {
        while (visualWheat != target)
        {
            if (visualWheat < target) visualWheat++;
            else visualWheat--;

            if (wheatText != null) wheatText.text = "Пшеница: " + visualWheat;
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator FlyAndFadeUnitText(GameObject textObj, float speed)
    {
        float elapsed = 0f;
        RectTransform rt = textObj.GetComponent<RectTransform>();
        TMP_Text tmp = textObj.GetComponent<TMP_Text>();
        
        while (elapsed < 1.5f)
        {
            if (Time.timeScale > 0)
            {
                elapsed += Time.deltaTime;
                if (rt != null) rt.anchoredPosition += new Vector2(0f, speed * Time.deltaTime);
                if (tmp != null) tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1.5f - elapsed);
            }
            yield return null;
        }
        Destroy(textObj);
    }

    IEnumerator FlyAndFadeLog(RectTransform rectTrans, TMP_Text textComp)
    {
        float elapsed = 0f;
        Color baseColor = textComp.color;

        while (elapsed < 4.5f) 
        {
            if (Time.timeScale > 0)
            {
                elapsed += Time.deltaTime;
                rectTrans.anchoredPosition += new Vector2(0f, 15f * Time.deltaTime); 
                textComp.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - (elapsed / 4.5f));
            }
            yield return null;
        }
        textComp.text = "";
    }

    public void UpdateTimerText(int seconds)
    {
        if (timerText != null) timerText.text = "До атаки: " + seconds + "с";
    }

    public void EndGame(bool win)
    {
        if (gameScreen != null) gameScreen.SetActive(false);
        if (resultScreen != null) resultScreen.SetActive(true);

        if (resultTitleText != null)
        {
            if (win) resultTitleText.text = "ПОБЕДА! Стратег спас город!";
            else resultTitleText.text = "ПОРАЖЕНИЕ! Поселение пало...";
        }
    }
}
