using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Configurações de UI")]
    public GameObject painelGameOver;
    public CanvasGroup grupoCanvas; 

    [Header("Configurações de Tempo")]
    public float tempoAnimacao = 2f; 

    public void AtivarTelaGameOver()
    {
        StartCoroutine(SequenciaFimDeFita());
    }

    private IEnumerator SequenciaFimDeFita()
    {
        // Espera a animação
        yield return new WaitForSeconds(tempoAnimacao);

        // Ativa a tela de uma vez (Corte seco)
        painelGameOver.SetActive(true);
        grupoCanvas.alpha = 1f;

        // Pausa o jogo e corta o som
        Time.timeScale = 0f; 
        AudioListener.pause = true; 

        // Libera o mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Recomecar()
    {
        Time.timeScale = 1f; 
        AudioListener.pause = false; // Devolve o som
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false; // Devolve o som
        SceneManager.LoadScene(0); // Usa o número zero (do Build Settings)
    }
}