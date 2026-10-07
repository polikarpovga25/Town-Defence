using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public UIManager ui;

    public GameObject peasantPrefab;
    public GameObject warriorPrefab;
    public Transform gameScreenTransform;

    public int wheat = 250; 
    public int peasants = 3;
    public int warriors = 1;
    public int enemies = 3;

    public int peasantCost = 15;
    public int warriorCost = 30;

    private float harvestTimer = 3f;
    private float consumeTimer = 4f; 
    private float waveTimer = 15f;
    private float currentWaveMaxTime = 15f;
    private float eventTimer = 15f;

    private int currentWave = 0;
    private bool gameStarted = false;
    private bool isPaused = false;

    private List<KeyValuePair<GameObject, float>> peasantList = new List<KeyValuePair<GameObject, float>>();
    private List<KeyValuePair<GameObject, float>> warriorList = new List<KeyValuePair<GameObject, float>>();

    public void StartGame()
    {
        peasantList.Clear();
        warriorList.Clear();
        gameStarted = true;
        isPaused = false;
        
        for (int i = 0; i < peasants; i++) peasantList.Add(new KeyValuePair<GameObject, float>(SpawnVisualUnit(peasantPrefab), Time.time));
        for (int i = 0; i < warriors; i++) warriorList.Add(new KeyValuePair<GameObject, float>(SpawnVisualUnit(warriorPrefab), Time.time));

        ui.InitAllTexts(wheat, peasants, warriors, enemies, peasantCost, warriorCost);
    }

    void Update()
    {
        if (!gameStarted || isPaused) return;

        harvestTimer -= Time.deltaTime;
        if (harvestTimer <= 0)
        {
            int addedWheat = 0;
            harvestTimer = 3f;

            foreach (var p in peasantList)
            {
                if (p.Key != null)
                {
                    addedWheat += 12;
                    ui.SpawnFloatingTextAt(p.Key.transform.position, "+12", Color.green);
                }
            }

            wheat += addedWheat;
            ui.AnimateWheatChange(wheat);
            ui.UpdateAllTexts(wheat, peasants, warriors, enemies, peasantCost, warriorCost);
        }

        consumeTimer -= Time.deltaTime;
        if (consumeTimer <= 0)
        {
            consumeTimer = 3f; 
            int eatAmount = 0;

            foreach (var p in peasantList)
            {
                if (p.Key != null && Time.time - p.Value >= 5f)
                {
                    eatAmount += 3;
                    ui.SpawnFloatingTextAt(p.Key.transform.position, "-3", Color.red);
                }
            }

            foreach (var w in warriorList)
            {
                if (w.Key != null && Time.time - w.Value >= 5f)
                {
                    eatAmount += 12;
                    ui.SpawnFloatingTextAt(w.Key.transform.position, "-12", Color.red);
                }
            }

            if (wheat >= eatAmount)
            {
                wheat -= eatAmount;
                ui.AnimateWheatChange(wheat);
            }
            else
            {
                if (wheat > 0) ui.AnimateWheatChange(0);
                wheat = 0;
                Starve();
            }
            ui.UpdateAllTexts(wheat, peasants, warriors, enemies, peasantCost, warriorCost);
        }

        waveTimer -= Time.deltaTime;
        ui.UpdateTimerText((int)waveTimer);
        if (waveTimer <= 0)
        {
            Attack();
        }

        eventTimer -= Time.deltaTime;
        if (eventTimer <= 0)
        {
            RandomEvent();
            eventTimer = 15f;
        }
    }

    void Starve()
    {
        if (warriors > 0)
        {
            warriors--;
            RemoveLastUnit(warriorList);
            ui.ShowLog("Воин погиб от голода!");
        }
        else if (peasants > 0)
        {
            peasants--;
            RemoveLastUnit(peasantList);
            ui.ShowLog("Крестьянин погиб от голода!");
        }
        CheckLose();
    }

    public void BuyPeasant()
    {
        if (!gameStarted || isPaused) return;

        if (wheat >= peasantCost)
        {
            wheat -= peasantCost;
            peasants++;
            
            GameObject newUnit = SpawnVisualUnit(peasantPrefab);
            peasantList.Add(new KeyValuePair<GameObject, float>(newUnit, Time.time));
            
            ui.SpawnFloatingTextAt(newUnit.transform.position, "-" + peasantCost, Color.red);
            ui.AnimateWheatChange(wheat);
            
            peasantCost += 2;
            ui.UpdateAllTexts(wheat, peasants, warriors, enemies, peasantCost, warriorCost);
            ui.ShowLog("Нанят крестьянин!");
        }
        else
        {
            ui.ShowLog("Недостаточно пшеницы!");
        }
    }

    public void BuyWarrior()
    {
        if (!gameStarted || isPaused) return;

        if (wheat >= warriorCost)
        {
            wheat -= warriorCost;
            warriors++;
            
            GameObject newUnit = SpawnVisualUnit(warriorPrefab);
            warriorList.Add(new KeyValuePair<GameObject, float>(newUnit, Time.time));
            
            ui.SpawnFloatingTextAt(newUnit.transform.position, "-" + warriorCost, Color.red);
            ui.AnimateWheatChange(wheat);
            
            warriorCost += 5;
            ui.UpdateAllTexts(wheat, peasants, warriors, enemies, peasantCost, warriorCost);
            ui.ShowLog("Нанят воин!");
        }
        else
        {
            ui.ShowLog("Недостаточно пшеницы!");
        }
    }

    void Attack()
    {
        currentWave++;
        int attackEnemies = enemies;

        if (warriors >= attackEnemies)
        {
            for (int i = 0; i < attackEnemies; i++) RemoveLastUnit(warriorList);
            warriors -= attackEnemies;
            attackEnemies = 0;
            ui.ShowLog("Волна " + currentWave + " отбита!");
        }
        else
        {
            int currentWarriorsCount = warriors;
            for (int i = 0; i < currentWarriorsCount; i++) RemoveLastUnit(warriorList);
            attackEnemies -= warriors;
            warriors = 0;
        }

        if (attackEnemies > 0)
        {
            int toKill = attackEnemies * 3;
            for (int i = 0; i < toKill; i++) RemoveLastUnit(peasantList);
            peasants -= toKill;
            if (peasants < 0) peasants = 0;
            
            int stolenWheat = Mathf.RoundToInt(wheat * 0.5f);
            wheat -= stolenWheat;
            ui.AnimateWheatChange(wheat);
            
            ui.ShowLog("Враги прорвались! Разграбление амбара.");
        }

        currentWaveMaxTime += 15f;
        waveTimer = currentWaveMaxTime;

        enemies = Mathf.RoundToInt(enemies * 1.8f);
        ui.UpdateAllTexts(wheat, peasants, warriors, enemies, peasantCost, warriorCost);

        if (currentWave >= 5 && peasants > 0)
        {
            gameStarted = false;
            ui.EndGame(true);
        }

        CheckLose();
    }

    void RandomEvent()
    {
        if (Random.value < 0.4f)
        {
            if (Random.value < 0.5f)
            {
                int lost = Mathf.Min(wheat, 50);
                wheat -= lost;
                ui.AnimateWheatChange(wheat);
                ui.ShowLog("Чума на полях уничтожила пшеницу!");
            }
            else
            {
                wheat += 40;
                ui.AnimateWheatChange(wheat);
                ui.ShowLog("Найдена заброшенная телега с припасами.");
            }
            ui.UpdateAllTexts(wheat, peasants, warriors, enemies, peasantCost, warriorCost);
        }
    }

    void CheckLose()
    {
        if (peasants <= 0 && warriors <= 0)
        {
            gameStarted = false;
            ui.EndGame(false);
        }
    }

    public void SetPauseState(bool state)
    {
        isPaused = state;
    }

    GameObject SpawnVisualUnit(GameObject prefab)
    {
        if (gameScreenTransform == null || prefab == null) return null;
        
        GameObject unit = Instantiate(prefab, gameScreenTransform);
        RectTransform rt = unit.GetComponent<RectTransform>();
        
        if (rt != null)
        {
            Vector2 size = new Vector2(40f, 40f); 
            Vector2 safePos = Vector2.zero;
            bool positionFound = false;

            for (int attempt = 0; attempt < 100; attempt++)
            {
                Vector2 testPos = new Vector2(Random.Range(-400f, 400f), Random.Range(-250f, 250f));
                Rect testRect = new Rect(testPos - size / 2f, size);
                bool hasOverlap = false;

                foreach (var p in peasantList)
                {
                    if (p.Key != null)
                    {
                        Rect otherRect = new Rect((Vector2)p.Key.GetComponent<RectTransform>().anchoredPosition - size / 2f, size);
                        if (testRect.Overlaps(otherRect)) { hasOverlap = true; break; }
                    }
                }

                if (!hasOverlap)
                {
                    foreach (var w in warriorList)
                    {
                        if (w.Key != null)
                        {
                            Rect otherRect = new Rect((Vector2)w.Key.GetComponent<RectTransform>().anchoredPosition - size / 2f, size);
                            if (testRect.Overlaps(otherRect)) { hasOverlap = true; break; }
                        }
                    }
                }

                if (!hasOverlap)
                {
                    safePos = testPos;
                    positionFound = true;
                    break;
                }
            }

            if (!positionFound) safePos = new Vector2(Random.Range(-400f, 400f), Random.Range(-250f, 250f));
            
            rt.anchoredPosition = safePos;
        }
        return unit;
    }

    void RemoveLastUnit(List<KeyValuePair<GameObject, float>> list)
    {
        if (list.Count > 0)
        {
            int lastIndex = list.Count - 1;
            if (list[lastIndex].Key != null)
            {
                Destroy(list[lastIndex].Key);
            }
            list.RemoveAt(lastIndex);
        }
    }
}