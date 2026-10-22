namespace Takwene.Api.Profiles;

public static class MappingRegistration
{
    public static IServiceCollection AddApplicationMappings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(options =>
        {
            options.ShouldUseConstructor = constructor => constructor.IsPublic;
            var licenseKey = configuration["AutoMapper:LicenseKey"];
            if (!string.IsNullOrWhiteSpace(licenseKey)) options.LicenseKey = licenseKey;
        }, typeof(ArtistProfile).Assembly);
        return services;
    }
}
