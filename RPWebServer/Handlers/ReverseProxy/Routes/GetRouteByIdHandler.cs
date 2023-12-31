using AutoMapper;
using MediatR;
using RPWebServer.Commands.ReverseProxy.Routes;
using RPWebServer.Models.RouteConfig;
using RPWebServer.Services.ReverseProxy;

namespace RPWebServer.Handlers.ReverseProxy.Routes;

public class GetRouteByIdHandler : IRequestHandler<GetRouteByIdRequest, GetRouteByIdResponse>
{
    public IRoutesConfigProvider RoutesConfigProvider { get; }
    public IMapper Mapper { get; }

    public GetRouteByIdHandler(
        IRoutesConfigProvider routesConfigProvider,
        IMapper mapper)
    {
        RoutesConfigProvider = routesConfigProvider;
        Mapper = mapper;
    }
    
    public async Task<GetRouteByIdResponse> Handle(GetRouteByIdRequest request, CancellationToken cancellationToken)
    {
        var routeConfig = RoutesConfigProvider.GetRouteById(request.Id);
        var routeConfigDto = Mapper.Map<RouteConfigDto>(routeConfig);

        return new GetRouteByIdResponse(routeConfigDto);
    }
}
