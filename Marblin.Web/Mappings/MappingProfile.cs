using AutoMapper;
using Marblin.Application.DTOs;
using Marblin.Web.ViewModels;

namespace Marblin.Web.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CheckoutViewModel, OrderSubmissionDto>();
        }
    }
}
