using System.Collections.Concurrent;

public class SseNotificationService
{
    private readonly ConcurrentBag<StreamWriter> _clients = new();

    public void AddClient(StreamWriter client)
    {
        _clients.Add(client);
    }

    public async Task BroadcastOrderUpdateAsync(OrderUpdateMessage message)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(message);
        var activeClients = _clients.ToList();

        foreach (var client in activeClients)
        {
            try
            {
                await client.WriteAsync($"data: {json}\n\n");
                await client.FlushAsync();
            }
            catch
            {
                // Pokud klient neodpovídá (zavřel prohlížeč), v reálném produkčním kódu by se měl odstranit.
                // Pro zjednodušení školního projektu to stačí takto, případně streamy čistit.
            }
        }
    }
}

public class OrderUpdateMessage
{
    public int TotalPortions { get; set; }
    public decimal TotalPrice { get; set; }
}