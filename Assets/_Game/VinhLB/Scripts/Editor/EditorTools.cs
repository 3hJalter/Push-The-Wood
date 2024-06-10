using UnityEditor;
using UnityEditor.SceneManagement;

namespace VinhLB
{
    public static class EditorTools
    {
        [MenuItem("Tools/VinhLB/Scenes/GameDemo")]
        private static void OpenGameDemoScene()
        {
            OpenGlobalScene("GameDemo");
        }

        [MenuItem("Tools/VinhLB/Scenes/ChooseLevel")]
        private static void OpenChooseLevelScene()
        {
            OpenGlobalScene("ChooseLevel");
        }

        [MenuItem("Tools/VinhLB/Scenes/MapEditor")]
        private static void OpenMapEditorScene()
        {
            OpenGlobalScene("MapEditor");
        }
        
        [MenuItem("Tools/VinhLB/Scenes/SecretMapEditor")]
        private static void OpenSecretMapEditorScene()
        {
            OpenVinhLBScene("SecretMapEditor");
        }

        [MenuItem("Tools/VinhLB/Scenes/BuildingTest")]
        private static void OpenBuildingTestScene()
        {
            OpenVinhLBScene("BuildingTest");
        }

        [MenuItem("Tools/VinhLB/Scenes/Test")]
        private static void OpenTestScene()
        {
            OpenVinhLBScene("Test");
        }

        private static void OpenVinhLBScene(string sceneName)
        {
            OpenScene($"Assets/_Game/VinhLB/Scenes/{sceneName}.unity", OpenSceneMode.Single);
        }

        private static void OpenGlobalScene(string sceneName)
        {
            OpenScene($"Assets/_Game/_Scenes/{sceneName}.unity", OpenSceneMode.Single);
        }

        private static void OpenScene(string scenePath, OpenSceneMode openSceneMode)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath, openSceneMode);
            }
        }
    }
}