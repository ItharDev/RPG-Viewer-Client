using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace theaaa
{
    public class MenuItemFolder
    {
        // Add a menu item to create custom GameObjects.
        // Priority 10 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarchy context menus.
        [MenuItem("GameObject/Folder/Create Folder", false, -1)]
        static void CreateEmptyFolder()
        {
            GameObject obj = new GameObject();
            Undo.RegisterCreatedObjectUndo(obj, obj.name);
            if (Selection.activeTransform != null)
            {
                Object[] selectedObjects = Selection.objects;
                GameObject selectedGameObject = selectedObjects[0] as GameObject;


                obj.transform.SetParent(selectedGameObject.transform);

                //UnCollaps();
                //UnCollapseHierarchy();
            }
            obj.name = "New Folder";
            EditorUtility.SetDirty(obj);
            Selection.activeGameObject = obj;
            AddTag(obj, "Folder");

        }
        [MenuItem("GameObject/Folder/Create Folder", true, -1)]
        private static bool ValidateCreateEmpty()
        {
            // Return true to enable the menu item, false to disable it
            return Selection.objects.Length <= 1;
        }



        /// <summary>
        /// Adding Tag first in system then in Game object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="tag"></param>
        public static void AddTag(GameObject obj, string tag)
        {
            Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");

                for (int i = 0; i < tags.arraySize; ++i)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                    {
                        // Debug.Log("Already Here");
                        obj.tag = tag;
                        return;     // Tag already present, nothing to do.
                    }
                }

                tags.InsertArrayElementAtIndex(0);
                tags.GetArrayElementAtIndex(0).stringValue = tag;
                so.ApplyModifiedProperties();
                so.Update();

                obj.tag = tag;
            }
        }

        /// <summary>
        /// Checking Selected object parent has in the list 
        /// if it has then no need to setparent the child because if parent goes child also goes with it.
        /// </summary>
        /// <param name="selectedObjects"></param>
        /// <returns></returns>
        public static bool CheckSelectObjectParentInList(Object[] selectedObjects)
        {
            foreach (Object o in selectedObjects)
            {
                GameObject obj = o as GameObject;
                Transform parent = obj.transform.parent;
                foreach (Object ob in selectedObjects)
                {
                    GameObject obj2 = ob as GameObject;
                    if (obj2.transform == parent) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Zombi for now
        /// UnCollaps for new object parent
        /// </summary>
        public static void UnCollaps()
        {
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var window = EditorWindow.GetWindow(type);
            var exprec = type.GetMethod("SetExpandedRecursive");
            exprec?.Invoke(window, new object[] { Selection.activeGameObject.transform.GetInstanceID(), true });

        }

        /// <summary>
        /// Zombi for now
        /// </summary>
        public static void UnCollapseHierarchy()
        {
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var hierarchyWindow = EditorWindow.GetWindow(type);
            var expandMethodInfo = hierarchyWindow.GetType().GetMethod("SetExpandedRecursive");
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Debug.Log(root.name);
                if (root != Selection.activeGameObject)
                {
                    expandMethodInfo.Invoke(hierarchyWindow, new object[] { root.GetInstanceID(), true });
                    //return;
                }

            }
        }

    }
}