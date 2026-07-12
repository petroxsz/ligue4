using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class TCPManager : MonoBehaviour
{
    public static TCPManager Instance;
    private Queue<string> mensagensRecebidas = new Queue<string>();
private Queue<Action> filaPrincipal = new Queue<Action>();

    [Header("Configuração")]
    public int porta = 7777;

    private TcpListener servidor;
    private TcpClient cliente;
    private NetworkStream stream;

    private Thread threadReceber;

    public bool conectado = false;

    public Action<string> AoReceberMensagem;
    public Action AoConectar;


    public bool souHost;


    private void Awake()
{
    Debug.Log("TCPManager criado em: " + gameObject.scene.name);

    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Debug.Log("DESTRUINDO DUPLICADO: " + gameObject.name);
        Destroy(gameObject);
    }
}


    // ==========================
    // HOST
    // ==========================

    public void CriarSala()
    {
        souHost = true;
        servidor = new TcpListener(IPAddress.Any, porta);
        servidor.Start();

        Debug.Log("Servidor criado. Aguardando jogador...");

        servidor.BeginAcceptTcpClient(AceitarCliente, null);
    }


    private void AceitarCliente(IAsyncResult resultado)
{
    cliente = servidor.EndAcceptTcpClient(resultado);

    stream = cliente.GetStream();

    conectado = true;

    Debug.Log("Cliente conectado!");

    lock (filaPrincipal)
    {
        filaPrincipal.Enqueue(() =>
        {
            AoConectar?.Invoke();
        });
    }

    IniciarRecebimento();
}



    // ==========================
    // CLIENTE
    // ==========================

    public void EntrarSala(string ip)
    {
        souHost = false;

        try
        {
            cliente = new TcpClient();

            cliente.Connect(ip, porta);

            stream = cliente.GetStream();

            conectado = true;

            Debug.Log("Conectado ao servidor!");
            lock (filaPrincipal)
{
    filaPrincipal.Enqueue(() =>
    {
        AoConectar?.Invoke();
    });
}

            IniciarRecebimento();
        }
        catch(Exception erro)
        {
            Debug.LogError("Erro ao conectar: " + erro.Message);
        }
    }



    // ==========================
    // ENVIO
    // ==========================

    public void EnviarMensagem(string mensagem)
    {

        Debug.Log("TENTANDO ENVIAR: " + mensagem);

        if (!conectado)
            return;


        byte[] dados = Encoding.UTF8.GetBytes(mensagem);

        stream.Write(dados, 0, dados.Length);
    }



    // ==========================
    // RECEBIMENTO
    // ==========================

    private void IniciarRecebimento()
    {
        threadReceber = new Thread(Receber);
        threadReceber.Start();
    }


    private void Receber()
    {

        

        byte[] buffer = new byte[1024];


        while(conectado)
        {
            try
            {
                int tamanho = stream.Read(buffer,0,buffer.Length);

                if(tamanho > 0)
                {
                    string mensagem = Encoding.UTF8.GetString(buffer,0,tamanho);

                    Debug.Log("Recebido: " + mensagem);
                    Debug.Log("RECEBEU REDE: " + mensagem);

lock(mensagensRecebidas)
{
    mensagensRecebidas.Enqueue(mensagem);
}
                }
            }
            catch(Exception e)
{
    Debug.LogError("ERRO RECEBIMENTO TCP: " + e);

    conectado = false;
}
        }
    }


    private void Update()
{
    // Eventos da conexão
    lock (filaPrincipal)
    {
        while (filaPrincipal.Count > 0)
        {
            filaPrincipal.Dequeue()?.Invoke();
        }
    }


    // Mensagens do jogo
    lock (mensagensRecebidas)
    {
        while (mensagensRecebidas.Count > 0)
        {
            string mensagem = mensagensRecebidas.Dequeue();

            AoReceberMensagem?.Invoke(mensagem);
        }
    }
}




    private void OnApplicationQuit()
    {
        conectado = false;

        if(stream != null)
            stream.Close();

        if(cliente != null)
            cliente.Close();

        if(servidor != null)
            servidor.Stop();
    }

    
}