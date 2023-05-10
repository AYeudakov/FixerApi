using AutoMapper;
using MeDirect.CurrencyExchange.Application.Entities;
using MeDirect.CurrencyExchange.Domain.Models;

namespace MeDirect.CurrencyExchange.Profiles;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<TransactionInfo, Transaction>()
            .ForMember(dest => dest.Amount, options => options.Ignore())
            .ForMember(dest => dest.TransactionTime, options => options.MapFrom(src => new DateTime(src.Info.Timestamp)))
            .ForMember(dest => dest.From, options => options.MapFrom(src => src.Query.From))
            .ForMember(dest => dest.To, options => options.MapFrom(src => src.Query.To))
            .ForMember(dest => dest.Amount, options => options.MapFrom(src => src.Query.Amount))
            .ForMember(dest => dest.ExchangeRate, options => options.MapFrom(src => src.Info.Rate));
    }
}