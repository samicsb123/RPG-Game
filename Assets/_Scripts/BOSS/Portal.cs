using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Setări Portal")]
    public string sceneToLoad; // Numele scenei în care pleacă

    [Tooltip("Scrie numele exact al obiectului de Spawn din URMĂTOAREA scenă unde vrei să apară playerul")]
    public string targetSpawnPointName;

    // Variabila statică: Magia care supraviețuiește schimbării de scenă!
    public static string nextSpawnPointName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Salvăm numele punctului de destinație înainte ca scena să se șteargă
            nextSpawnPointName = targetSpawnPointName;

            Debug.Log($"Teleportare! Mergem în {sceneToLoad} și căutăm spawn-ul: {targetSpawnPointName}");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}