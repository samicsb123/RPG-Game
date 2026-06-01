using UnityEngine;

public class BlacksmithDropZone : MonoBehaviour
{
    public UpgradeManager upgradeManager;

    // Această funcție va fi apelată direct de sabie când este aruncată pe el
    public void PrimesteArma(DraggableItem item)
    {
        if (item != null && (item.categoriaItemului == TipItem.Arma || item.categoriaItemului == TipItem.Armura))
        {
            // Deschidem panoul și trimitem arma spre el!
            upgradeManager.DeschidePanou(item);
        }
    }
}