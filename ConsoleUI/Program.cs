using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;
using ProcessadorTarefas.Servicos;
using Repositorio;

namespace ConsoleUI
{
    internal partial class Program
    {
        static async Task Main(string[] args)
        {
            var visaoAtual = Visualizacao.NaoDefinida;

            var serviceProvider = ConfigureServiceProvider();

            var processador = serviceProvider.GetService<IProcessadorTarefas>();
            var gerenciador = serviceProvider.GetService<IGerenciadorTarefas>();


            await processador.Iniciar();

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var option = Console.ReadKey(intercept: true).KeyChar;
                    Console.WriteLine();
                    switch (option)
                    {
                        case '1':
                            visaoAtual = Visualizacao.TarefasAtivas;
                            break;
                        case '2':
                            var tarefasInativas = await gerenciador.ListarInativas();
                            ProgramHelpers.ImprimirTarefas(tarefasInativas);
                            visaoAtual = Visualizacao.TarefasInativas;
                            break;
                        case '3':
                            await gerenciador.Criar();
                            Console.WriteLine("Nova tarefa criada");
                            await Task.Delay(1000);
                            break;
                        case '4':
                            Console.WriteLine("Digite o número da tarefa:");
                            if (int.TryParse(Console.ReadLine(), out int idTarefa))
                            {
                                var tarefa = gerenciador.Consultar(idTarefa);
                                if (tarefa != null)
                                    await processador.CancelarTarefa(idTarefa);
                            }

                            break;
                        case '5':
                            Console.WriteLine("Encerrando processamento...");
                            await processador.Encerrar();
                            Console.WriteLine("Processamento encerrado.");
                            await Task.Delay(1000);
                            break;
                        case '6':
                            Console.WriteLine("Reiniciando processamento...");
                            await processador.Iniciar();
                            Console.WriteLine("Processamento reiniciado.");
                            await Task.Delay(1000);
                            break;
                        default:
                            Console.WriteLine("Opção inválida!!!");
                            visaoAtual = 0;
                            await Task.Delay(1000);
                            break;
                    }
                }

                Console.Clear();
                ProgramHelpers.ImprimirMenu();

                switch (visaoAtual)
                {
                    case Visualizacao.TarefasAtivas:
                        var tarefasAtivas = await gerenciador.ListarAtivas();
                        ProgramHelpers.ImprimirTarefas(tarefasAtivas);
                        break;
                    case Visualizacao.TarefasInativas:
                        var tarefasInativas = await gerenciador.ListarInativas();
                        ProgramHelpers.ImprimirTarefas(tarefasInativas);
                        break;
                    default:
                        break;
                }

                Console.WriteLine();
                Console.WriteLine();
                ProgramHelpers.ImprimirProgressoTarefas(gerenciador);

                await Task.Delay(100);
            }
        }

        private static IServiceProvider ConfigureServiceProvider()
        {
            string connectionString = "Data Source=database.db";

            IConfiguration configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .Build();

            IServiceCollection services = new ServiceCollection();
            services.AddScoped(_ => configuration);
            services.AddScoped<IRepository<Tarefa>, MemoryRepository>();
            //services.AddScoped<IRepository<Tarefa>>(_ => new SqliteRepository<Tarefa>(connectionString) );
            services.AddSingleton<IProcessadorTarefas, Processador>();
            services.AddScoped<IGerenciadorTarefas, Gerenciador>(serviceProvider =>
            {
                var repository = serviceProvider.GetService<IRepository<Tarefa>>();
                return new Gerenciador(serviceProvider, repository, configuration);
            });

            return services.BuildServiceProvider(); ;
        }
    }
}
