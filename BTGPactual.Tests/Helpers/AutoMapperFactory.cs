using AutoMapper;
using BTGPactual.Application.Mappings;

namespace BTGPactual.Tests.Helpers;

public static class AutoMapperFactory
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        return config.CreateMapper();
    }
}