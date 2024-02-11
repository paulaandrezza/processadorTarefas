using Microsoft.Extensions.Configuration;

namespace ProcessadorTarefas.Entidades
{
    public interface IEntidade
    {
        int Id { get; }
    }
    public interface ITarefa : IEntidade
    {
        EstadoTarefa Estado { get; }
        DateTime? IniciadaEm { get; }
        DateTime? EncerradaEm { get; }
        IEnumerable<Subtarefa> SubtarefasPendentes { get; }
        IEnumerable<Subtarefa> SubtarefasExecutadas { get; }
    }

    public class Tarefa : ITarefa
    {

        private static Random random = new Random();
        public int Id { get; set; }
        public EstadoTarefa Estado { get; private set; } = EstadoTarefa.Criada;
        public DateTime? IniciadaEm { get; set; }
        public DateTime? EncerradaEm { get; set; }
        public IEnumerable<Subtarefa> SubtarefasPendentes { get; set; } = new List<Subtarefa>();
        public IEnumerable<Subtarefa> SubtarefasExecutadas { get; set; } = new List<Subtarefa>();

        private Tarefa() { }

        public static Tarefa Criar(int id, IConfiguration? configs = null)
        {
            var novaTarefa = new Tarefa
            {
                Id = id,
                SubtarefasPendentes = CriarSubtarefas(configs),
                SubtarefasExecutadas = new List<Subtarefa>()
            };

            return novaTarefa;
        }

        private static IEnumerable<Subtarefa> CriarSubtarefas(IConfiguration? configs)
        {
            var result = new List<Subtarefa>();

            var quantidadeSubtarefas = random.Next(int.Parse(configs?[Consts.MIN_SUBTASKS] ?? "1"), int.Parse(configs?[Consts.MAX_SUBTASKS] ?? "10")); ;

            for (int i = 0; i < quantidadeSubtarefas; i++)
                result.Add(
                    new Subtarefa()
                    {
                        Duracao = TimeSpan.FromSeconds(random.Next(int.Parse(configs?[Consts.MIN_DURATION_SUBTASKS] ?? "1"), int.Parse(configs?[Consts.MAX_DURATION_SUBTASKS] ?? "10")))
                    });

            return result;
        }

        public void ConcluirSubtarefa(Subtarefa subtarefa)
        {
            SubtarefasPendentes = SubtarefasPendentes.Except(new[] { subtarefa });
            SubtarefasExecutadas = SubtarefasExecutadas.Append(subtarefa);
        }

        public void MudarEstado(EstadoTarefa novoEstado)
        {
            if (Estado == novoEstado)
                throw new InvalidOperationException($"Tarefa {Id} já está com estado {Estado}.");

            else if (!TarefaHelpers.TransicoesPermitidas.TryGetValue(Estado, out var estadosPermitidos) || !estadosPermitidos.Contains(novoEstado))
                throw new InvalidOperationException($"Não é possível realizar essa ação na Tarefa {Id} pois o seu estado atual é {Estado}.");

            else
            {
                Estado = novoEstado;
                if (novoEstado == EstadoTarefa.EmExecucao && IniciadaEm == null)
                    IniciadaEm = DateTime.Now;
                else if (novoEstado == EstadoTarefa.Concluida || novoEstado == EstadoTarefa.Cancelada)
                    EncerradaEm = DateTime.Now;
            }
        }

        public void Agendar() => MudarEstado(EstadoTarefa.Agendada);

        public void Iniciar() => MudarEstado(EstadoTarefa.EmExecucao);

        public void Pausar() => MudarEstado(EstadoTarefa.EmPausa);

        public void Cancelar() => MudarEstado(EstadoTarefa.Cancelada);

        public void Concluir() => MudarEstado(EstadoTarefa.Concluida);

        public bool PodeSerExecutada()
            => Estado == EstadoTarefa.Agendada || Estado == EstadoTarefa.EmPausa;
    }

}
