using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public TCPManager tcpManager;

    public TMP_InputField campoIP;
    public TMP_Text textoStatus;


    void Start()
    {
        tcpManager.AoConectar += Conectou;
    }


    void Conectou()
{
    Debug.Log("MENU RECEBEU CONEXÃO!");

    textoStatus.text = "Jogador conectado!";
}


    public void BotaoHost()
    {
        tcpManager.CriarSala();

        textoStatus.text = "Sala criada. Aguardando jogador...";
    }


    public void BotaoCliente()
    {
        tcpManager.EntrarSala(campoIP.text);

        textoStatus.text = "Tentando conectar...";
    }
}