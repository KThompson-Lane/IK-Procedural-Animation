using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor((typeof(SpiderController)))]
    public class InitialiseSpiderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var myScript = (SpiderController) target;
            if(GUILayout.Button("Setup spider"))
                myScript.InitialiseSpider();
        }
    }
}
