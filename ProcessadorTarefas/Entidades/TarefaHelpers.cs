using ProcessadorTarefas.Entidades;

internal static class TarefaHelpers
{
    public static readonly Dictionary<EstadoTarefa, EstadoTarefa[]> TransicoesPermitidas = new Dictionary<EstadoTarefa, EstadoTarefa[]>
    {
        { EstadoTarefa.Criada, new[] { EstadoTarefa.Agendada, EstadoTarefa.Cancelada } },
        { EstadoTarefa.Agendada, new[] { EstadoTarefa.EmExecucao, EstadoTarefa.Cancelada } },
        { EstadoTarefa.EmExecucao, new[] { EstadoTarefa.EmPausa, EstadoTarefa.Concluida, EstadoTarefa.Cancelada } },
        { EstadoTarefa.EmPausa, new[] { EstadoTarefa.EmExecucao } }
    };
}