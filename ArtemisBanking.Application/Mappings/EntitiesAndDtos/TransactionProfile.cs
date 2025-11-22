using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.EntitiesAndDtos
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction, TransactionDTO>();
        }
    }
}
