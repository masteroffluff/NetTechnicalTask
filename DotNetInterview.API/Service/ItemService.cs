// Routes
// - List all items
// - Get a single item
// - Create a new item
// - Update an item
// - Delete an item
// - Add a new variation to a Item
// - Update a variation
// - Delete a variation


using DotNetInterview.API.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DotNetInterview.API.Service;


public class ItemService
{
    private readonly DataContext _context;

    public ItemService(DataContext context)
    {
        _context = context;

    }
    // apply pricing rules
    static Item ApplyPricingRules(Item item)
    {
        DateTime currentTime = DateTime.Now;

        return Pricing.ApplyRules(item, currentTime);
    }
    public async Task<List<Item>> GetAllItems()
    {
        var items = await _context.Items
            .Include(c => c.Variations)
            .ToListAsync();

        List<Item> result = items.Select(i => ApplyPricingRules(i)).ToList();


        return result;
    }

    public async Task<Item?> GetSingleItem(Guid id)
    {
        var item = await _context.Items
        .Where(i => i.Id == id)
        .Include(i => i.Variations)
        .FirstOrDefaultAsync();

        if (item == null)
        {
            return null;
        }

        Item result = ApplyPricingRules(item);
        return result;
    }
    public async Task<Item> PostItem(Item newItem)
    {
        _context.Items.Add(newItem);
        await _context.SaveChangesAsync();
        return ApplyPricingRules(newItem);
    }
    public async Task<bool> UpdateItem(Item newItem)
    {
        var id = newItem.Id;
        var oldItem = await _context.Items
            .Where(i => i.Id == id)
            .Include(i => i.Variations)
            .FirstOrDefaultAsync();
        if (oldItem == null)
        {
            return false;
        }
        // using a transaction to manage the updates and ensure atomicity
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // remove old variations and items
            foreach (var v in oldItem.Variations.ToList())
            {
                _context.Variations.Remove(v);
            }

            _context.Items.Remove(oldItem);
            // replace
            _context.Items.Add(newItem);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (System.Exception)
        {

            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<bool> DeleteItem(Guid id)
    {
        var oldItem = await _context.Items
                .Where(i => i.Id == id)
                .Include(i => i.Variations)
                .FirstOrDefaultAsync();
        if (oldItem == null)
        {
            return false;
        }
        foreach (var v in oldItem.Variations.ToList())
        {
            _context.Variations.Remove(v);
        }

        _context.Items.Remove(oldItem);
        await _context.SaveChangesAsync();
        return true;
    }

}
