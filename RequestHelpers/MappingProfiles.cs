using AutoMapper;
using RestoreApiV2.DTOs;
using RestoreApiV2.Entities;

namespace RestoreApiV2.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();
        }
    }
}
