using System.Threading.Tasks;
using Coinbot.Domain.Contracts;
using Coinbot.Domain.Contracts.Models;

namespace Coinbot.Core.Implementations
{
    public class AdvancedBot : IBot
    {
        public Task<ServiceResponse> BuyIfConditionsMet()
        {
            throw new System.NotImplementedException();
        }

        public Task<ServiceResponse> CheckIfBought()
        {
            throw new System.NotImplementedException();
        }

        public Task<ServiceResponse> CheckIfSold()
        {
            throw new System.NotImplementedException();
        }

        public Task<ServiceResponse> SellIfConditionsMet()
        {
            throw new System.NotImplementedException();
        }
    }
}