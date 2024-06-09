using AutoMapper;
using eCommerceProject.Data;
using eCommerceProject.ViewModels;

namespace eCommerceProject.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterVM, KhachHang>();
                
        }            

    }
}
