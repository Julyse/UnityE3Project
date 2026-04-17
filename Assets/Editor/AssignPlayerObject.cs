using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AssignPlayerObject
{
    static AssignPlayerObject()
    {
        EditorApplication.delayCall += Assign;
    }

    [MenuItem("Tools/Assign PlayerObject to PlayerMovementAdvanced")]
    static void Assign()
    {
        // Use known instanceIds: Player=188618, PlayerObject=189338
        var playerGO = EditorUtility.InstanceIDToObject(188618) as GameObject;
        var playerObjGO = EditorUtility.InstanceIDToObject(189338) as GameObject;

        if (playerGO == null || playerObjGO == null)
        {
            // Fallback: search by name
            playerGO = GameObject.Find("Player");
            if (playerGO != null)
            {
                var t = playerGO.transform.Find("PlayerObject");
                if (t != null) playerObjGO = t.gameObject;
            }
        }

        if (playerGO == null) { Debug.LogError("AssignPlayerObject: Player not found"); return; }
        if (playerObjGO == null) { Debug.LogError("AssignPlayerObject: PlayerObject not found"); return; }

        var pma = playerGO.GetComponent<PlayerMovementAdvanced>();
        if (pma == null) { Debug.LogError("AssignPlayerObject: PlayerMovementAdvanced not found"); return; }

        if (pma.playerObject != null) { Debug.Log("AssignPlayerObject: already assigned to " + pma.playerObject.name); return; }

        pma.playerObject = playerObjGO.transform;
        EditorUtility.SetDirty(pma);
        EditorSceneManager.MarkSceneDirty(pma.gameObject.scene);
        Debug.Log("AssignPlayerObject: SUCCESS — playerObject assigned to " + playerObjGO.name);
    }
}
