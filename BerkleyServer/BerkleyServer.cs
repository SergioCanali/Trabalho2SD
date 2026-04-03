using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

class BerkeleyServer
{
    static List<TcpClient> clients = new List<TcpClient>();

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();

        Console.WriteLine("Servidor aguardando conexões...");

        // Aceitar clientes (ex: 3 clientes)
        for (int i = 0; i < 3; i++)
        {
            var client = server.AcceptTcpClient();
            clients.Add(client);
            Console.WriteLine("Cliente conectado!");
        }

        SincronizarRelogios();
    }

    static void SincronizarRelogios()
    {
        List<double> offsets = new List<double>();

        // Hora do servidor
        DateTime serverTime = DateTime.Now;

        // 1. Solicitar diferenças
        foreach (var client in clients)
        {
            NetworkStream stream = client.GetStream();

            byte[] msg = Encoding.UTF8.GetBytes(serverTime.ToString("O"));
            stream.Write(msg, 0, msg.Length);

            byte[] buffer = new byte[1024];
            int bytes = stream.Read(buffer, 0, buffer.Length);

            double offset = double.Parse(Encoding.UTF8.GetString(buffer, 0, bytes));
            offsets.Add(offset);
        }

        // Inclui o próprio servidor (offset 0)
        offsets.Add(0);

        // 2. Calcular média
        double media = 0;
        foreach (var o in offsets)
            media += o;

        media /= offsets.Count;

        Console.WriteLine($"Média calculada: {media} ms");

        // 3. Enviar ajuste
        foreach (var client in clients)
        {
            NetworkStream stream = client.GetStream();

            // ajuste = média - offset_cliente
            double clientOffset = offsets[clients.IndexOf(client)];
            double ajuste = media - clientOffset;

            byte[] msg = Encoding.UTF8.GetBytes(ajuste.ToString());
            stream.Write(msg, 0, msg.Length);
        }

        Console.WriteLine("Sincronização concluída!");
    }
}