using System.Threading.Tasks;
using Coinbot.Domain.Contracts.Models;

namespace Coinbot.Domain.Contracts
{
    public interface IBot
    {
        Task<ServiceResponse> BuyIfConditionsMet ();
        Task<ServiceResponse> SellIfConditionsMet ();

        Task<ServiceResponse> CheckIfSold ();
        Task<ServiceResponse> CheckIfBought ();
    }
}