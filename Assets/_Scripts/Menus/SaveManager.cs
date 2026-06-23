using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    [Header("Referințe Personaj")]
    public PlayerStats playerStats;
    public GameObject pauseMenuPanel;

    [Header("Referințe Inventar & Toolbar")]
    public List<InventorySlot> toateSloturileInventar = new List<InventorySlot>();
    public List<InventorySlot> toateSloturileToolbar = new List<InventorySlot>();
    public List<GameObject> itemPrefabs = new List<GameObject>();

    [Header("Setări Auto-Save")]
    public float autoSaveInterval = 60f;
    private float autoSaveTimer = 0f;

    private string saveFilePath;

    [System.Serializable]
    private class ItemSaveData
    {
        public string numePrefab;
        public int nivelUpgrade;
        public int slotIndex;
        public int cantitate; // NOU: Salvează câte poțiuni/iteme ai
    }

    [System.Serializable]
    private class SaveData
    {
        public float x, y, z;
        public int level, xp, xpToNextLevel, gold, points;
        public int str, dex, vit, hp;

        public List<ItemSaveData> inventarIteme = new List<ItemSaveData>();
        public List<ItemSaveData> toolbarIteme = new List<ItemSaveData>();
        public int weaponUpgrade;
    }

    void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/savefile.json";
    }

    void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        LoadGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        if (Time.timeScale > 0)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                SaveGame();
                autoSaveTimer = 0f;
            }
        }
    }

    public void TogglePauseMenu()
    {
        bool isPaused = pauseMenuPanel.activeSelf;
        if (!isPaused)
        {
            SaveGame();
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        data.x = transform.position.x; data.y = transform.position.y; data.z = transform.position.z;
        data.level = playerStats.level; data.xp = playerStats.currentXP; data.xpToNextLevel = playerStats.xpToNextLevel;
        data.gold = playerStats.gold; data.points = playerStats.statPointsAvailable;
        data.str = playerStats.strength; data.dex = playerStats.dexterity; data.vit = playerStats.vitality;
        data.hp = playerStats.currentHealth;

        // INVENTAR
        for (int i = 0; i < toateSloturileInventar.Count; i++)
        {
            DraggableItem item = toateSloturileInventar[i].GetComponentInChildren<DraggableItem>();
            if (item != null)
            {
                ItemSaveData itemData = new ItemSaveData();
                itemData.numePrefab = item.gameObject.name.Replace("(Clone)", "").Trim();
                itemData.nivelUpgrade = item.nivelUpgrade;
                itemData.slotIndex = i;

                // Salvăm cantitatea (Dacă în DraggableItem ai alt nume, schimbă "cantitate" aici)
                itemData.cantitate = item.cantitate;

                data.inventarIteme.Add(itemData);
            }
        }

        // TOOLBAR
        for (int i = 0; i < toateSloturileToolbar.Count; i++)
        {
            DraggableItem item = toateSloturileToolbar[i].GetComponentInChildren<DraggableItem>();
            if (item != null)
            {
                ItemSaveData itemData = new ItemSaveData();
                itemData.numePrefab = item.gameObject.name.Replace("(Clone)", "").Trim();
                itemData.nivelUpgrade = item.nivelUpgrade;
                itemData.slotIndex = i;

                itemData.cantitate = item.cantitate;

                data.toolbarIteme.Add(itemData);
            }
        }

        if (EquipmentManager.instance != null && EquipmentManager.instance.slotArma != null)
        {
            DraggableItem arma = EquipmentManager.instance.slotArma.GetComponentInChildren<DraggableItem>();
            if (arma != null) data.weaponUpgrade = arma.nivelUpgrade;
        }

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Joc salvat complet (inclusiv cantitatea)!");
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath)) return;

        string json = File.ReadAllText(saveFilePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // ==========================================
        // REZOLVARE BUG TELEPORTARE: 
        // Am ascuns linia de mai jos ca să nu te mai mute forțat 
        // la coordonatele vechi din sat când intri în alte hărți.
        // ==========================================
        // transform.position = new Vector3(data.x, data.y, data.z);

        playerStats.level = data.level; playerStats.currentXP = data.xp; playerStats.xpToNextLevel = data.xpToNextLevel;
        playerStats.gold = data.gold; playerStats.statPointsAvailable = data.points;
        playerStats.strength = data.str; playerStats.dexterity = data.dex; playerStats.vitality = data.vit;

        playerStats.RecalculateStats();
        playerStats.currentHealth = data.hp;

        playerStats.UpdateUI();
        playerStats.UpdateHPText();
        if (playerStats.healthBar != null) playerStats.healthBar.SetHealth(data.hp);

        GolesteToateSloturile();

        // ÎNCĂRCARE INVENTAR
        foreach (ItemSaveData itemData in data.inventarIteme)
        {
            GameObject prefab = GasestePrefabDupaNume(itemData.numePrefab);
            if (prefab != null && itemData.slotIndex < toateSloturileInventar.Count)
            {
                GameObject obiectNou = Instantiate(prefab, toateSloturileInventar[itemData.slotIndex].transform);
                DraggableItem scriptItem = obiectNou.GetComponent<DraggableItem>();
                if (scriptItem != null)
                {
                    scriptItem.nivelUpgrade = itemData.nivelUpgrade;

                    // Încărcăm cantitatea înapoi
                    scriptItem.cantitate = itemData.cantitate;

                    scriptItem.ActualizeazaText();
                }
            }
        }

        // ÎNCĂRCARE TOOLBAR
        foreach (ItemSaveData itemData in data.toolbarIteme)
        {
            GameObject prefab = GasestePrefabDupaNume(itemData.numePrefab);
            if (prefab != null && itemData.slotIndex < toateSloturileToolbar.Count)
            {
                GameObject obiectNou = Instantiate(prefab, toateSloturileToolbar[itemData.slotIndex].transform);
                DraggableItem scriptItem = obiectNou.GetComponent<DraggableItem>();
                if (scriptItem != null)
                {
                    scriptItem.nivelUpgrade = itemData.nivelUpgrade;

                    // Încărcăm cantitatea înapoi
                    scriptItem.cantitate = itemData.cantitate;

                    scriptItem.ActualizeazaText();
                }
            }
        }

        if (EquipmentManager.instance != null && EquipmentManager.instance.slotArma != null)
        {
            DraggableItem arma = EquipmentManager.instance.slotArma.GetComponentInChildren<DraggableItem>();
            if (arma != null)
            {
                arma.nivelUpgrade = data.weaponUpgrade;
                arma.ActualizeazaText();
            }
        }
    }

    void GolesteToateSloturile()
    {
        foreach (var slot in toateSloturileInventar)
        {
            foreach (Transform copil in slot.transform)
            {
                // NOU: Verificăm dacă obiectul este un item. Distrugem DOAR itemele!
                if (copil.GetComponent<DraggableItem>() != null)
                {
                    Destroy(copil.gameObject);
                }
            }
        }
        foreach (var slot in toateSloturileToolbar)
        {
            foreach (Transform copil in slot.transform)
            {
                if (copil.GetComponent<DraggableItem>() != null)
                {
                    Destroy(copil.gameObject);
                }
            }
        }
    }

    GameObject GasestePrefabDupaNume(string nume)
    {
        foreach (GameObject prefab in itemPrefabs)
        {
            if (prefab.name == nume) return prefab;
        }
        Debug.LogWarning("SaveManager: Nu am găsit niciun prefab numit " + nume + " în lista Item Prefabs!");
        return null;
    }

    public void GoToMainMenu()
    {
        SaveGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        SaveGame();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}