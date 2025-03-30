using DotNetInterview.API.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DotNetInterview.API.Service;
// Extra Rules
// - When the quantity of stock for an item is greater than 5, the price should be discounted by 10%
// - When the quantity of stock for an item is greater than 10, the price should be discounted by 20%
// - Every Monday between 12pm and 5pm, all items are discounted by 50%
// - Only a single discount should be applied to an item at any time, the highest discount percentage

public class Pricing
{
    public Pricing()
    {

    }
    // - When the quantity of stock for an item is greater than 5, the price should be discounted by 10%
    // - When the quantity of stock for an item is greater than 10, the price should be discounted by 20%
    private static decimal QuantityRule(decimal price, int quantity)
    {
        decimal result = price;
        if (quantity > 10)
        {
            result -= price * 0.2m;

        }
        else if (quantity > 5)
        {
            result -= price * 0.1m;
        }
        return result;
    }
    // - Every Monday between 12pm and 5pm, all items are discounted by 50%
    private static decimal TimeRule(decimal price, DateTime datetime)
    {
        decimal result = price;
        if (datetime.DayOfWeek == DayOfWeek.Monday)
        {
            if (datetime.Hour >= 12 && datetime.Hour < 17)
            {
                result -= price * 0.5m;
            }
        }
        return result;
    }
    public static Item ApplyRules(Item item, DateTime time)
    {

        int totalQuantity = item.Variations.Sum(v => v.Quantity);
        

        decimal quantityDiscountPrice = QuantityRule(item.Price, totalQuantity);
        decimal timeDiscountPrice = TimeRule(item.Price, time);

        item.Price =  Math.Min(quantityDiscountPrice,timeDiscountPrice );
        return item;
    }
}