// Services/TableStatusService.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Hubs;
using RestaurantQRSystem.Models.Enums;

public class TableStatusService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<OrderHub> _hubContext;

    public TableStatusService(ApplicationDbContext context, IHubContext<OrderHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task UpdateTableStatus(int tableId)
    {
        var table = await _context.Tables
            .Include(t => t.Orders.Where(o => o.Status != OrderStatus.Completed &&
                                              o.Status != OrderStatus.Cancelled &&
                                              o.Status != OrderStatus.Paid))
            .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table == null)
            return;

        // Masanın yeni durumunu belirle
        TableStatus newStatus = TableStatus.Available;

        if (table.Orders.Any())
        {
            newStatus = TableStatus.Occupied;

            // Masa dolu olarak işaretle (eski property'leri de kullanalım)
            table.IsOccupied = true;
            if (table.OccupiedSince == null)
            {
                table.OccupiedSince = DateTime.Now;
            }
        }
        else
        {
            // Masa boş olarak işaretle
            table.IsOccupied = false;
            table.OccupiedSince = null;
        }

        await _context.SaveChangesAsync();

        // SignalR ile masanın durumunun güncellendiğini bildir
        await _hubContext.Clients.All.SendAsync("TableStatusUpdated", tableId, (int)newStatus);
    }
}