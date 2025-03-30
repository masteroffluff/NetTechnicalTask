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
    // business logic for pricing rules
    static Item ApplyPricingRules(Item item){
        DateTime currentTime = DateTime.Now;
        
        return Pricing.ApplyRules(item, currentTime );
    }
    async public Task<List<Item>> GetAllItems(){
    var items = await _context.Items
        .Include(c => c.Variations)  
        .ToListAsync();  

    List<Item> result = items.Select(i => ApplyPricingRules(i)).ToList();


    return result;
    }
    
    async public Task<Item> GetSingleItem(Guid id){
    var item = await _context.Items
    .Where(i => i.Id == id)
    .Include(i => i.Variations)  // eager loading
    .FirstOrDefaultAsync(); 

    if(item == null){
        return null;
    }

     Item result = ApplyPricingRules(item);
    return result;
    }
    async public Task<Item> PostItem(Item newItem){
    newItem.Id = new Guid();
    _context.Items.Add(newItem);
    await _context.SaveChangesAsync();
    return ApplyPricingRules(newItem);
    }
    async public Task<bool> UpdateItem(Item newItem){
    var id = newItem.Id;
    var oldItem = await _context.Items.FindAsync(id);
    if (oldItem == null)
    {
        return false;
    }
    _context.Items.Remove(oldItem);
    // replace
    _context.Items.Add(newItem);
    await _context.SaveChangesAsync();
    return true;
    }
    async public Task<bool> DeleteItem(Guid id){
    var oldItem = await _context.Items.FindAsync(id);
    if (oldItem == null)
    {
        return false;
    }
    _context.Items.Remove(oldItem);
    await _context.SaveChangesAsync();
    return true;
    }
    
}
