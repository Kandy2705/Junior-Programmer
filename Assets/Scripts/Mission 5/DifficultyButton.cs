using UnityEngine;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour
{
    private Button button;
    public GameManager gameManager;

    public int difficulty;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        button.onClick.AddListener(SetDiffculty);
    }

    void SetDiffculty()
    {
        Debug.Log(gameObject.name + " was clicked. Difficulty: " + difficulty);
        gameManager.StartGame(difficulty);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
