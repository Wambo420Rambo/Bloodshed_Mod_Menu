using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace mod_menu
{
    public partial class ModMenuBehaviour
    {
        private static List<GameObject> CollectAliveEnemies()
        {
            var result     = new List<GameObject>();
            int enemyLayer = LayerMask.NameToLayer("Enemy");

            foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (obj.layer == enemyLayer && obj.CompareTag("Enemy") && obj.activeInHierarchy)
                    result.Add(obj);
            }

            return result;
        }
    }
}
