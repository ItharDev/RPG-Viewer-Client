using UnityEngine;
using UnityEditor;

namespace FunkyCode
{
    public class PassShader : ShaderGUI
    {
        public enum EnumPass {None, Pass1, Pass2, Pass3, Pass4, Pass5, Pass6, Pass7, Pass8}

        private bool IsPassEnabled(Material material, int passId) => material.IsKeywordEnabled("SL2D_PASS_" + passId);

        private bool IsActive(Material material)
        {
            for(int i = 0; i <= 8; i++)
                if (IsPassEnabled(material, i))               
                    return true;

            return false;
        }

        private void SetPass(Material material, int passId)
        {
            material.SetInt("_PassId", passId);

            for(int i = 0; i <= 8; i++)
            {
                string passName = "SL2D_PASS_" + i;

                if (i == passId)
                    material.EnableKeyword(passName);
                else           
                    material.DisableKeyword(passName);
            }
        }

        string[] PassPopup = new string[]{"None", "Pass 1", "Pass 2", "Pass 3", "Pass 4", "Pass 5", "Pass 6", "Pass 7", "Pass 8"};

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material material = materialEditor.target as Material;

            if (!IsActive(material))
            {
                Debug.Log("is inactive");

                SetPass(material, 1);  
            }
                else
            {
                for(int i = 1; i <= 8; i++)
                {
                    LightingSettings.LightmapPreset preset = LightmapShaders.ActivePassLightmaps[i];

                    string name = preset != null ? " (" + preset.name + ")" : "";
                    string passName = "Pass " + i + name;

                    PassPopup[i] = passName;
                }

                int passId = material.GetInt("_PassId");

                int newPassId = EditorGUILayout.Popup("Pass", passId, PassPopup);

                if (newPassId != passId)
                {
                    SetPass(material, newPassId);
                }
            }

            base.OnGUI(materialEditor, properties);
        }

    }
}