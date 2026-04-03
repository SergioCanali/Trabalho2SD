using System;
using System.Net.Sockets;
using System.Text;

class BerkeleyClient
{
    static void Main()
    {
        TcpClient client = new TcpClient("127.0.0.1", 5000);
        NetworkStream stream = client.GetStream();

        // Relógio local (simulando diferença)
        DateTime localTime = DateTime.Now.AddMilliseconds(new Random().Next(-5000, 5000));

        Console.WriteLine($"Hora local inicial: {localTime}");

        // 1. Receber hora do servidor
        byte[] buffer = new byte[1024];
        int bytes = stream.Read(buffer, 0, buffer.Length);

        DateTime serverTime = DateTime.Parse(Encoding.UTF8.GetString(buffer, 0, bytes));

        // 2. Calcular diferença
        double offset = (localTime - serverTime).TotalMilliseconds;

        byte[] msg = Encoding.UTF8.GetBytes(offset.ToString());
        stream.Write(msg, 0, msg.Length);

        // 3. Receber ajuste
        bytes = stream.Read(buffer, 0, buffer.Length);
        double ajuste = double.Parse(Encoding.UTF8.GetString(buffer, 0, bytes));

        // 4. Aplicar ajuste
        localTime = localTime.AddMilliseconds(ajuste);

        Console.WriteLine($"Ajuste recebido: {ajuste} ms");
        Console.WriteLine($"Hora sincronizada: {localTime}");

        client.Close();
    }
}