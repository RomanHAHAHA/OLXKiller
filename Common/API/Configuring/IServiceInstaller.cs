using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.API.Configuring;

public interface IServiceInstaller
{
    void Install(IServiceCollection services, IConfiguration configuration);
}