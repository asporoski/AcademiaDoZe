
// Amanda Soares Poroski

using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // ============================================
        // SERVICOS DA APLICACAO
        // ============================================

        services.AddTransient<
            ILogradouroService,
            LogradouroService>();

        services.AddTransient<
            IColaboradorService,
            ColaboradorService>();

        services.AddTransient<
            IAlunoService,
            AlunoService>();

        services.AddTransient<
            IMatriculaService,
            MatriculaService>();

        // Avaliacao 03 - Acesso dos alunos
        services.AddTransient<AcessoAlunoService>();

        // Avaliacao 03 - Acesso dos colaboradores
        services.AddTransient<AcessoColaboradorService>();

        // ============================================
        // REPOSITORIO DE LOGRADOUROS
        // ============================================

        services.AddTransient(provider =>
        {
            var config =
                provider.GetRequiredService<RepositoryConfig>();

            return (Func<ILogradouroRepository>)(() =>
                new LogradouroRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        // ============================================
        // REPOSITORIO DE COLABORADORES
        // ============================================

        services.AddTransient(provider =>
        {
            var config =
                provider.GetRequiredService<RepositoryConfig>();

            return (Func<IColaboradorRepository>)(() =>
                new ColaboradorRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        // ============================================
        // REPOSITORIO DE ALUNOS
        // ============================================

        services.AddTransient(provider =>
        {
            var config =
                provider.GetRequiredService<RepositoryConfig>();

            return (Func<IAlunoRepository>)(() =>
                new AlunoRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        // ============================================
        // REPOSITORIO DE MATRICULAS
        // ============================================

        services.AddTransient(provider =>
        {
            var config =
                provider.GetRequiredService<RepositoryConfig>();

            return (Func<IMatriculaRepository>)(() =>
                new MatriculaRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        // ============================================
        // REPOSITORIO DE ACESSOS DOS ALUNOS
        // ============================================

        services.AddTransient(provider =>
        {
            var config =
                provider.GetRequiredService<RepositoryConfig>();

            return (Func<IAcessoAlunoRepository>)(() =>
                new AcessoAlunoRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        // ============================================
        // REPOSITORIO DE ACESSOS DOS COLABORADORES
        // ============================================

        services.AddTransient(provider =>
        {
            var config =
                provider.GetRequiredService<RepositoryConfig>();

            return (Func<IAcessoColaboradorRepository>)(() =>
                new AcessoColaboradorRepository(
                    config.ConnectionString,
                    config.DatabaseType));
        });

        return services;
    }
}
