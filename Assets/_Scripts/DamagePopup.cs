using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Setări Animație")]
    public float vitezaZbor = 2f;
    public float timpPanaDispare = 0.5f;

    [Header("Design Text")]
    public Color culoareNormala = Color.green;
    public Color culoareCritica = Color.magenta; // Magenta este un mov aprins perfect pentru jocuri

    private TextMeshPro textMesh;

    void Awake()
    {
        // Preluăm componenta de text
        textMesh = GetComponent<TextMeshPro>();
    }

    // NOU: Funcția primește acum și un Adevărat/Fals care ne spune dacă e critică
    public void SeteazaDamage(int damage, bool esteCritic)
    {
        textMesh.text = damage.ToString();

        if (esteCritic)
        {
            textMesh.color = culoareCritica;
            textMesh.fontSize += 2; // Mărim fontul puțin la loviturile critice
        }
        else
        {
            textMesh.color = culoareNormala;
        }

        Destroy(gameObject, timpPanaDispare);
    }

    void Update()
    {
        transform.position += new Vector3(0, vitezaZbor * Time.deltaTime, 0);
    }
}