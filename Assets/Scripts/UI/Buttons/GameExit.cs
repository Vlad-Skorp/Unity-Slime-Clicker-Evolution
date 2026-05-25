using UnityEngine;

public class GameExit : MonoBehaviour
{
    public void QuitGame()
    {
        // Для ПК-билдов
        Application.Quit();

        // Чтобы проверить работу прямо в Unity Editor (в консоли появится надпись)
        Debug.Log("Игра закрылась!");
    }
}
