using UnityEngine;

public class Restart : MonoBehaviour
{
   public void RestartGame()
   {
      // Oyunu yeniden başlatmak için sahneyi yeniden yükle
      UnityEngine.SceneManagement.SceneManager.LoadScene(
         UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
      );
   }
}
