using UnityEngine;

public class LevelManager : MonoBehaviour
{
    void Start()
    {
        // Când jucătorul intră în scena cu orașul, pornim muzica satului.
        // Asigură-te că "VillageMusic" este numele exact pe care l-ai pus în lista din AudioManager!
        AudioManager.instance.PlayMusic("VillageMusic");
    }
}