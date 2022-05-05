using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NordTaskApi.Common.Data;
using NordTaskApi.Common.Repositories;
using NordTaskApi.Common.Services;
using NordTaskApi.Functions.Common.Auth;
using System.IdentityModel.Tokens.Jwt;

[assembly: FunctionsStartup(typeof(MyNamespace.Startup))]

namespace MyNamespace
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<IOIDCConfigurationManagerWrapper, OIDCConfigurationManagerWrapper>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton(new JwtSecurityTokenHandler());

            builder.Services.AddDbContext<NotesContext>(options =>
            {
                options.UseSqlServer(builder.GetContext().Configuration.GetSection("ConnectionStrings:NordTaskApi").Value);
            });

            builder.Services.AddTransient<INotesRepository, NotesRepository>();
            builder.Services.AddTransient<INotesService, NotesService>();
        }
    }
}