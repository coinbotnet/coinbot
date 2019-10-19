using AutoMapper;
using Coinbot.SQLite.Models;
using DbOrder = Coinbot.Domain.Contracts.Models.DatabaseService.Order;

namespace Coinbot.SQLite.Mappings
{
    public class DatabaseProfile : Profile
    {
        public DatabaseProfile()
        {
            CreateMap<Order,DbOrder>().ReverseMap();
        }
    }
}