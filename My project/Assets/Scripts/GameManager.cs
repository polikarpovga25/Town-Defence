using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public UIManager ui;

    public GameObject peasantPrefab;
    public GameObject warriorPrefab;
    public Transform gameScreenTransform;

    public int wheat = 300; 
    public int peasants = 5;
    public int warriors = 2;
    public int enemies = 3;

    public int peasantCost = 20;
    public int warriorCost = 50;

    private float harvestTimer = 5f;
    private float consumeTimer = 4f;
    private float waveTimer = 45f;
    private float eventTimer = 20f;

    private float peasantHireTimer = -1f;
    private float warriorHireTimer = -1f;

    private int currentWave = 0;
    private bool gameStarted = false;

    private List<GameObject> peasantList = new List<GameObject>();
    private List<GameObject> warriorList = new List<GameObject>();

    public void StartGame()
    {
        peasantList.Clear();
        warriorList.Clear();
        gameStarted = true;
        
        for (int i = 0; i < peasants; i++) peasantList.Add(SpawnVisualUnit(peasantPrefab));
        for (int i = 0; i < warriors; i++) warriorList.Add(SpawnVisualUnit(warriorPrefab));

        ui.UpdateAllTexts(wheat, peasants, warriors, enemies);
    }

    void Update()
    {
        if (!gameStarted) return;

        harvestTimer -= Time.deltaTime;
        if (harvestTimer <= 0)
        {
            wheat += peasants * 10;
            harvestTimer = 5f;
            ui.UpdateAllTexts(wheat, peasants, warriors, enemies);
        }

        consumeTimer -= Time.deltaTime;
        if (consumeTimer <= 0)
        {
            int eatAmount = warriors * 5;
            if (wheat >= eatAmount)
            {
                wheat -= eatAmount;
            }
            else
            {
                wheat = 0;
                Starve();
            }
            consumeTimer = 4f;
            ui.UpdateAllTexts(wheat, peasants, warriors, enemies);
        }

        waveTimer -= Time.deltaTime;
        ui.UpdateTimerText((int)waveTimer);
        if (waveTimer <= 0)
        {
            Attack();
            waveTimer = 45f;
        }

        eventTimer -= Time.deltaTime;
        if (eventTimer <= 0)
        {
            RandomEvent();
            eventTimer = 20f;
        }

        HandleHiring();
    }

    void Starve()
    {
        if (warriors > 0)
        {
            warriors--;
            RemoveLastUnit(warriorList);
            ui.ShowLog("Воин умер от голода!");
        }
        else if (peasants > 0)
        {
            peasants--;
            RemoveLastUnit(peasantList);
            ui.ShowLog("Крестьянин умер от голода!");
        }
        CheckLose();
    }

    public void BuyPeasant()
    {
        if (peasantHireTimer < 0 && wheat >= peasantCost)
        {
            wheat -= peasantCost;
            peasantHireTimer = 3f;
            ui.UpdateAllTexts(wheat, peasants, warriors, enemies);
        }
    }

    public void BuyWarrior()
    {
        if (warriorHireTimer < 0 && wheat >= warriorCost)
        {
            wheat -= warriorCost;
            warriorHireTimer = 6f;
            ui.UpdateAllTexts(wheat, peasants, warriors, enemies);
        }
    }

    void HandleHiring()
    {
        if (peasantHireTimer >= 0)
        {
            peasantHireTimer -= Time.deltaTime;
            ui.peasantSlider.value = (3f - peasantHireTimer) / 3f;
            if (peasantHireTimer < 0)
            {
                peasants++;
                peasantList.Add(SpawnVisualUnit(peasantPrefab));
                ui.peasantSlider.value = 0;
                ui.UpdateAllTexts(wheat, peasants, warriors, enemies);
            }
        }

        if (warriorHireTimer >= 0)
        {
            warriorHireTimer -= Time.deltaTime;
            ui.warriorSlider.value = (6f - warriorHireTimer) / 6f;
            if (warriorHireTimer < 0)
            {
                warriors++;
                warriorList.Add(SpawnVisualUnit(warriorPrefab));
                ui.warriorSlider.value = 0;
                ui.UpdateAllTexts(wheat, peasants, warriors, enemies);
            }
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
            ui.ShowLog("Волна " + currentWave + " полностью отбита!");
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
            ui.ShowLog("Враги ворвались! Деревня понесла потери.");
        }

        enemies = Mathf.RoundToInt(enemies * 1.5f);
        ui.UpdateAllTexts(wheat, peasants, warriors, enemies);

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
                wheat = Mathf.Max(0, wheat - 30);
                ui.ShowLog("Наводнение уничтожило часть пшеницы!");
            }
            else
            {
                wheat += 40;
                ui.ShowLog("Отличная погода принесла дополнительный урожай!");
            }
            ui.UpdateAllTexts(wheat, peasants, warriors, enemies);
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
                Vector2 testPos = new Vector2(Random.Range(-200f, 200f), Random.Range(-100f, 100f));
                Rect testRect = new Rect(testPos - size / 2f, size);
                bool hasOverlap = false;

                foreach (GameObject p in peasantList)
                {
                    if (p != null)
                    {
                        Rect otherRect = new Rect((Vector2)p.GetComponent<RectTransform>().anchoredPosition - size / 2f, size);
                        if (testRect.Overlaps(otherRect)) { hasOverlap = true; break; }
                    }
                }

                if (!hasOverlap)
                {
                    foreach (GameObject w in warriorList)
                    {
                        if (w != null)
                        {
                            Rect otherRect = new Rect((Vector2)w.GetComponent<RectTransform>().anchoredPosition - size / 2f, size);
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

            if (!positionFound) safePos = new Vector2(Random.Range(-200f, 200f), Random.Range(-100f, 100f));
            
            rt.anchoredPosition = safePos;
        }
        return unit;
    }

    void RemoveLastUnit(List<GameObject> list)
    {
        if (list.Count > 0)
        {
            int lastIndex = list.Count - 1;
            if (list[lastIndex] != null)
            {
                Destroy(list[lastIndex]);
            }
            list.RemoveAt(lastIndex);
        }
    }
}
