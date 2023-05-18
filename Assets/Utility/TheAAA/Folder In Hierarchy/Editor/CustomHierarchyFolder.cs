using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;

namespace theaaa
{
    /// <summary>
    /// ToDo
    /// craete a IsExpended with out any issue for folder icone change
    /// </summary>
    [InitializeOnLoad]
    public class CustomHierarchyFolder
    {

        public static GameObject selectObj;
        static CustomHierarchyFolder()
        {
            //this delegate enables to put things on hierarchy
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }


        /// <summary>
        /// Where whole calculation of showing folder happend.
        /// </summary>
        /// <param name="instanceID">Object id</param>
        /// <param name="selectionRect"></param>
        static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            //Debug.Log(EditorPrefs.GetBool("MoveWindowOn", false));
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (obj == null)
            {
                selectObj = null;
                return;
            }
            //Checking Object select or not and cashing it for parent / child 
            if (obj.transform == Selection.activeTransform)
            {
                selectObj = obj;
            }
            else
            {
                //   selectObj = null;
            }


            Rect r = selectionRect;

            if (!IsTagTheir("Folder")) return;

            //selecting collection objects and decorating them
            if (obj.CompareTag("Folder"))
            //if (obj.name.StartsWith("f_",System.StringComparison.Ordinal))
            {
                //Debug.Log(obj.name + " " + obj.tag);
                EnableDisableButton(obj, selectionRect);

                //Folder always in center of the world
                TransfromReset(obj);


                //Disableing Editing option

                obj.transform.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
                obj.hideFlags = HideFlags.HideInInspector;


                //Disabling picking from scene.
                var sv = SceneVisibilityManager.instance;
                sv.DisablePicking(obj, false);


                // Over Lay Color
                //return color to normal othewise everything will be transparent!
                GUI.color = new Color(1, 1, 1, 1);
                //Changing Object icone to folder icone
                r = selectionRect;
                // Debug.Log("R " + r.ToString());
                r.xMin = selectionRect.x + 1;
                r.xMax = r.xMin + 15f;
                // Debug.Log("R " + r.ToString());


                bool isPro = EditorGUIUtility.isProSkin;
                // Debug.Log("Pro = " + isPro);


                // Theme change based on the object select or not
                if (obj.transform == Selection.activeTransform)
                {

                    //Theme Color
                    if (isPro) EditorGUI.DrawRect(r, new Color(0.17f, 0.36f, 0.52f, 1f));
                    else { EditorGUI.DrawRect(r, new Color(0.23f, 0.45f, 0.69f, 1f)); }

                }
                else
                {
                    //Theme Color
                    if (isPro) EditorGUI.DrawRect(r, new Color(0.22f, 0.22f, 0.22f, 1f));
                    else { EditorGUI.DrawRect(r, new Color(0.8f, 0.8f, 0.8f, 1f)); }
                }



                //Changing DisplayName
                r = selectionRect;
                r.x = selectionRect.xMin + 25;


                int child = obj.transform.childCount;
                if (obj.activeInHierarchy)
                {
                    // EditorGUI.LabelField(r, obj.name.ToUpperInvariant(), EditorStyles.boldLabel);
                    ChangeFolderIconeActive(obj, selectionRect);


                }
                else
                {
                    // EditorGUI.LabelField(r, obj.name.ToUpperInvariant(), EditorStyles.miniLabel);
                    ChangeFolderIconeDactive(obj, selectionRect);

                }
            }

        }



        /// <summary>
        /// Changing Folder Icon On Active mode
        /// </summary>
        /// <param name="childcout"></param>
        /// <param name="selectionRect"></param>
        static void ChangeFolderIconeActive(GameObject obj, Rect selectionRect)
        {
            int childcout = obj.transform.childCount;
            bool isExtended = true;
            // SerializedProperty sp = new SerializedObject.
            // if (EditorPrefs.GetBool("MoveWindowOn", true) == false)
            {
                //  isExtended = SceneHierarchyUtility.IsExpanded(obj);
            }
            string iconeName = "";
            bool hasChild = childcout > 0;
            if (hasChild)
            {
                //Theme Color
                if (EditorGUIUtility.isProSkin)
                {
                    if (isExtended)
                    {
                        iconeName = "FolderOpened On Icon";
                    }
                    else
                    {
                        iconeName = "Folder On Icon";
                    }
                }
                else
                {
                    if (isExtended)
                    {
                        iconeName = "FolderOpened Icon";
                    }
                    else
                    {
                        iconeName = "Folder Icon";
                    }

                }

            }
            else
            {

                //Theme Color
                if (EditorGUIUtility.isProSkin)
                {


                    iconeName = "FolderEmpty On Icon";

                }
                else
                {

                    iconeName = "FolderEmpty Icon";


                }

            }

            EditorGUI.LabelField(selectionRect, EditorGUIUtility.IconContent(iconeName));
        }

        /// <summary>
        /// Changing Folder Icon On Dactive mode
        /// </summary>
        /// <param name="childcout"></param>
        /// <param name="selectionRect"></param>
        static void ChangeFolderIconeDactive(GameObject obj, Rect selectionRect)
        {
            int childcout = obj.transform.childCount;
            bool isExtended = true;
            // if(EditorPrefs.GetBool("MoveWindowOn", true) == false)
            {
                //  isExtended = SceneHierarchyUtility.IsExpanded(obj);
            }

            string iconeName = "";
            bool hasChild = childcout > 0;
            if (hasChild)
            {
                //Theme Color
                if (EditorGUIUtility.isProSkin)
                {
                    if (isExtended)
                    {
                        iconeName = "FolderOpened Icon";
                    }
                    else
                    {
                        iconeName = "Folder Icon";
                    }
                }
                else
                {
                    if (isExtended)
                    {
                        iconeName = "FolderOpened On Icon";
                    }
                    else
                    {
                        iconeName = "Folder On Icon";
                    }


                }
            }
            else
            {
                //Theme Color
                if (EditorGUIUtility.isProSkin)
                {


                    iconeName = "FolderEmpty Icon";

                }
                else
                {

                    iconeName = "FolderEmpty On Icon";


                }


            }

            EditorGUI.LabelField(selectionRect, EditorGUIUtility.IconContent(iconeName));
        }


        /// <summary>
        /// Folder Enable Disable Button
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="selectionRect"></param>
        static void EnableDisableButton(GameObject obj, Rect selectionRect)
        {
            //doing some calc to place the button to leftmost side
            Rect r = selectionRect;
            //doing some calc to place the button to leftmost side
            r.xMax = r.width;
            r.x = selectionRect.xMax;




            //make the button transparent
            GUI.color = new Color(0, 0, 0, 0f);

            //if active>deactivate
            //if not active>activate
            if (GUI.Button(r, ""))
            {
                obj.SetActive(!obj.activeInHierarchy);
            }

            //return color to normal othewise everything will be transparent!
            GUI.color = new Color(1, 1, 1, 1);


            //label placement calc
            r.x = selectionRect.xMax;

            //show label based on active state
            GUI.Label(r, obj.activeInHierarchy ? EditorGUIUtility.IconContent("d_toggle_on_focus") : EditorGUIUtility.IconContent("d_toggle_bg"));

        }

        /// <summary>
        /// Transfrom Reset when crated
        /// </summary>
        /// <param name="obj"></param>
        static void TransfromReset(GameObject obj)
        {
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
        }
        /// <summary>
        /// Check is the tag in the project
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool IsTagTheir(string tag)
        {
            Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");
                bool isTag = false;
                for (int i = 0; i < tags.arraySize; ++i)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                    {
                        //Debug.Log("Already Here");
                        isTag = true;     // Tag already present, nothing to do.
                        break;
                    }
                    else
                    {
                        isTag = false;
                    }
                }
                if (isTag) return true;
                else { return false; }
            }
            else
            {
                return false;
            }
        }


    }
}

