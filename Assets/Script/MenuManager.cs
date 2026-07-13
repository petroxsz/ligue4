using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;

public class MenuManager : MonoBehaviour
{
    public TCPManager tcpManager;
    public TMP_Text textoIP;
    public TMP_InputField campoIP;
    public TMP_Text textoStatus;


    void Start()
    {
        tcpManager.AoConectar += Conectou;
        textoIP.text = "Seu IP: " + ObterIPLocal();
    }


    void Conectou()
{
    Debug.Log("MENU RECEBEU CONEXÃO!");

    textoStatus.text = "Jogador conectado!";

    Invoke(nameof(CarregarJogo), 1f);
}


void CarregarJogo()
{
    SceneManager.LoadScene("SampleScene");
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


    private string ObterIPLocal()
{
    string ip = "Não encontrado";

    IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

    foreach (IPAddress endereco in host.AddressList)
    {
        if (endereco.AddressFamily == AddressFamily.InterNetwork)
        {
            ip = endereco.ToString();
            break;
        }
    }

    return ip;
}
}