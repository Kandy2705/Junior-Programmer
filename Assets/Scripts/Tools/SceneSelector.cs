using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelector : MonoBehaviour
{
    public static string MenuSceneName = "SceneSelector";
    public static KeyCode BackKey = KeyCode.Escape;

    private static SceneSelector instance;

    private readonly string[] challengeScenes =
    {
        "Challenge 1", "Challenge 2", "Challenge 3", "Challenge 4", "Challenge 5"
    };

    private readonly string[] prototypeScenes =
    {
        "Prototype 1", "Prototype 2", "Prototype 3", "Prototype 4", "Prototype 5"
    };

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != MenuSceneName &&
            Input.GetKeyDown(BackKey))
        {
            SceneManager.LoadScene(MenuSceneName);
        }
    }

    void OnGUI()
    {
        bool isMenu = SceneManager.GetActiveScene().name == MenuSceneName;

        if (!isMenu)
        {
            GUI.Box(new Rect(10, 10, 260, 36),
                "Press " + BackKey + " to return to menu");
            return;
        }

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 220, 40, 440, Screen.height - 80));

        GUIStyle title = new GUIStyle(GUI.skin.label);
        title.fontSize = 26;
        title.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("SCENE SELECTOR", title);
        GUILayout.Space(10);

        GUIStyle header = new GUIStyle(GUI.skin.label);
        header.fontSize = 18;
        header.fontStyle = FontStyle.Bold;

        GUILayout.Label("Challenge", header);
        DrawSceneRow(challengeScenes);

        GUILayout.Space(20);
        GUILayout.Label("Prototype", header);
        DrawSceneRow(prototypeScenes);

        GUILayout.FlexibleSpace();
        GUILayout.Label("Tip: Press " + BackKey + " in any scene to come back.",
            GUI.skin.label);

        GUILayout.EndArea();
    }

    void DrawSceneRow(string[] scenes)
    {
        GUILayout.BeginHorizontal();
        for (int i = 0; i < scenes.Length; i++)
        {
            if (GUILayout.Button((i + 1).ToString(),
                    GUILayout.Height(60), GUILayout.Width(80)))
            {
                LoadSceneByName(scenes[i]);
            }
        }
        GUILayout.EndHorizontal();
    }

    void LoadSceneByName(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene '" + sceneName +
                "' is not in Build Settings. Run Tools > Scene Selector > Setup.");
        }
    }
}
