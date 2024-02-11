using ConsoleUI;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Servicos;
using System.Text;

public static class ProgramHelpers
{

    public static void ImprimirMenu()
    {
        Console.WriteLine("ESCOLHA UMA OPÇÃO: ");
        Console.WriteLine("---------------------------------------------");
        Console.WriteLine("    1. Listar tarefas ativas;");
        Console.WriteLine("    2. Listar tarefas inativas;");
        Console.WriteLine("    3. Criar nova tarefa;");
        Console.WriteLine("    4. Cancelar tarefa;");
        Console.WriteLine("    5. Parar de processar;");
        Console.WriteLine("    6. Reiniciar procesamento;");
        Console.WriteLine("---------------------------------------------");
        Console.WriteLine();
    }
    public static void ImprimirProgressoTarefas(IGerenciadorTarefas gerenciadorTarefas)
    {
        const int BAR_SIZE = 50;
        var sb = new StringBuilder();
        var tarefas = gerenciadorTarefas.ListarAtivas().GetAwaiter().GetResult();

        foreach (var tarefa in tarefas.Where(t => t.Estado == EstadoTarefa.EmExecucao))
            if (tarefa != null)
            {
                decimal competed = tarefa.SubtarefasExecutadas.Count();
                var total = tarefa.SubtarefasPendentes.Count() + tarefa.SubtarefasExecutadas.Count();
                var completion = competed / total;

                sb.AppendLine(
                    string.Concat($"TAREFA {tarefa.Id}".PadRight(20, ' '),
                    $": [{"█".Repeat(Convert.ToInt32(completion * BAR_SIZE)).PadRight(BAR_SIZE, '_')}] {Convert.ToInt32(completion * 100)}%"
                    ));
            }

        Console.WriteLine(sb.ToString());
    }
    public static void ImprimirTarefas(IEnumerable<Tarefa> tarefas)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            string.Join('|',
                "DESCRICÃO".PadRight(12, ' '),
                "ESTADO".PadRight(15, ' '),
                "INÍCIO".PadRight(30, ' '),
                "TÉRMINO".PadRight(30, ' '),
                "SUBTAREFAS".PadRight(10, ' '),
                "TEMPO TOTAL".PadRight(10, ' ')
                )
        );
        foreach (var tarefa in tarefas)
            if (tarefa != null)
            {
                sb.AppendLine($"{string.Join(
                        $"{Extensions.CodigoResetCor}|{Extensions.CodigoCor[tarefa.Estado]}",
                        $"{Extensions.CodigoCor[tarefa.Estado]} Tarefa {tarefa.Id}".PadRight(17, ' '),
                        $"{tarefa.Estado}".PadRight(15, ' '),
                        $"{tarefa.IniciadaEm}".PadRight(30, ' '),
                        $"{tarefa.EncerradaEm}".PadRight(30, ' '),
                        $"{tarefa.SubtarefasExecutadas.Count() + tarefa.SubtarefasPendentes.Count()}".PadRight(10, ' '),
                        $"{tarefa.SubtarefasExecutadas.Union(tarefa.SubtarefasPendentes).Sum(x => x.Duracao.TotalSeconds)}".PadRight(10, ' ')
                    )}{Extensions.CodigoResetCor}");
            }

        Console.WriteLine(sb.ToString());
    }
}