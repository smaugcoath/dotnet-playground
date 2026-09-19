using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pocs.Aspire.Business.Common;
using Pocs.Aspire.Business.Users.Create;
using Pocs.Aspire.Business.Users.Delete;
using Pocs.Aspire.Business.Users.GetById;
using Pocs.Aspire.Business.Users.List;
using Pocs.Aspire.Business.Users.Update;

namespace Pocs.Aspire.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateService, CreateService>();
        services.AddScoped<IGetByIdService, GetByIdService>();
        services.AddScoped<IUpdateService, UpdateService>();
        services.AddScoped<IDeleteService, DeleteService>();
        services.AddScoped<IListService, ListService>();

        services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>();

        return services;
    }
}
