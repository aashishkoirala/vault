using Microsoft.Extensions.DependencyInjection;

namespace AK.Vault
{
    public static class DependencyRegistrar
    {
        public static IServiceCollection AddVaultServices(this IServiceCollection services) => services
            .AddSingleton<EncryptionKeyGenerator>()
            .AddSingleton<FileEncryptorFactory>()
            .AddSingleton<FileNameManager>()
            .AddSingleton<ListGenerator>()
            .AddSingleton<SymmetricEncryptor>();
    }
}