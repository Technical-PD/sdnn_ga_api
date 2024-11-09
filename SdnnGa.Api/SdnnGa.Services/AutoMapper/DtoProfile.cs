using AutoMapper;
using SdnnGa.Database.Models;
using SdnnGa.Model.Models;

namespace SdnnGa.Services.AutoMapper;

public class DtoProfile : Profile
{
    public DtoProfile()
    {
        CreateMap<DbNeuralNetworkModel, NeuralNetworkModel>().ReverseMap();
        CreateMap<DbGeneticConfig, GeneticConfig>().ReverseMap();
        CreateMap<DbFitConfig, FitConfig>().ReverseMap();
        CreateMap<DbSession, Session>().ReverseMap();
        CreateMap<DbEpoch, Epoch>().ReverseMap();
    }
}
