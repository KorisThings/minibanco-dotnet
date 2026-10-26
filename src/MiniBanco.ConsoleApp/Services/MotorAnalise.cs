using System;
using System.Collections.Generic;
using MiniBanco.ConsoleApp.Models;

namespace MiniBanco.ConsoleApp.Services
{
    /// <summary>
    /// Motor que calcula pontuação, índice de saúde e classificação das contas,
    /// e imprime o relatório no terminal.
    /// Toda varredura é feita com foreach/for + if (sem LINQ - RF09).
    /// </summary>
    public class MotorAnalise
    {
        public const int PontosDeposito = 10;
        public const int PontosSaque = 5;
        public const int PontosTarifa = 0;
        public const int PontuacaoMaximaPorTransacao = 10;

        public const string ClassificacaoSaudavel = "Conta Saudável";
        public const string ClassificacaoAtencao = "Conta em Atenção";
        public const string ClassificacaoRisco = "Conta em Risco";

        private const string LinhaDupla = "====================================================================";
        private const string LinhaSimples = "--------------------------------------------------------------------";

        // ---------------------------------------------------------------
        // RF04 - Tabela de pontuação por transação
        // ---------------------------------------------------------------
        public int ObterPontuacao(string tipo)
        {
            switch (tipo)
            {
                case Transacao.TipoDeposito:
                    return PontosDeposito;
                case Transacao.TipoSaque:
                    return PontosSaque;
                case Transacao.TipoTarifa:
                    return PontosTarifa;
                default:
                    throw new ArgumentException($"Tipo de transação desconhecido: {tipo}");
            }
        }

        public int CalcularPontuacaoObtida(Conta conta)
        {
            int total = 0;
            foreach (Transacao transacao in conta.Extrato)
            {
                total += ObterPontuacao(transacao.Tipo);
            }
            return total;
        }

        public int CalcularPontuacaoMaxima(Conta conta)
        {
            return conta.Extrato.Count * PontuacaoMaximaPorTransacao;
        }

        // ---------------------------------------------------------------
        // RF05 - Índice de saúde (%)
        // ---------------------------------------------------------------
        public double CalcularIndiceSaude(int pontuacaoObtida, int pontuacaoMaxima)
        {
            // Conta sem transações: evita divisão por zero.
            if (pontuacaoMaxima == 0)
            {
                return 0.0;
            }

            // Cast para double: sem ele, 30 / 50 em inteiros daria 0.
            return ((double)pontuacaoObtida / pontuacaoMaxima) * 100.0;
        }

        // ---------------------------------------------------------------
        // RF06 - Classificação e ação corporativa
        // ---------------------------------------------------------------
        public string Classificar(double percentual)
        {
            if (percentual >= 90.0)
            {
                return ClassificacaoSaudavel;
            }
            else if (percentual >= 60.0)
            {
                return ClassificacaoAtencao;
            }
            else
            {
                return ClassificacaoRisco;
            }
        }

        public string ObterAcaoCorporativa(string classificacao)
        {
            switch (classificacao)
            {
                case ClassificacaoSaudavel:
                    return "Oferecer investimentos e avaliar aumento de limite.";
                case ClassificacaoAtencao:
                    return "Contato da gerência para orientação financeira e revisão de tarifas.";
                default:
                    return "Bloquear aumento de limite e abrir análise de crédito.";
            }
        }

        // ---------------------------------------------------------------
        // RF07 / RF09 - Filtragem por tipo com laço tradicional
        // ---------------------------------------------------------------
        public List<Transacao> FiltrarPorTipo(Conta conta, string tipo)
        {
            List<Transacao> resultado = new List<Transacao>();
            foreach (Transacao transacao in conta.Extrato)
            {
                if (transacao.Tipo == tipo)
                {
                    resultado.Add(transacao);
                }
            }
            return resultado;
        }

        public decimal SomarValores(List<Transacao> transacoes)
        {
            decimal soma = 0m;
            for (int i = 0; i < transacoes.Count; i++)
            {
                soma += transacoes[i].Valor;
            }
            return soma;
        }

        // ---------------------------------------------------------------
        // RF08 - Recomendações para a gerência
        // ---------------------------------------------------------------
        public List<string> GerarRecomendacoes(Conta conta, List<Transacao> depositos, List<Transacao> saques, List<Transacao> tarifas)
        {
            List<string> recomendacoes = new List<string>();

            if (conta.Saldo < 0)
            {
                recomendacoes.Add($"PRIORIDADE ALTA: contatar o cliente. Saldo negativo de R$ {conta.Saldo:F2}.");
            }

            if (tarifas.Count > 0)
            {
                recomendacoes.Add($"Revisar o pacote de tarifas: {tarifas.Count} tarifa(s) somando R$ {SomarValores(tarifas):F2}.");
            }

            decimal totalSaques = SomarValores(saques);
            decimal totalDepositos = SomarValores(depositos);
            if (saques.Count > 0 && totalSaques > totalDepositos)
            {
                recomendacoes.Add($"Saques (R$ {totalSaques:F2}) superam os depósitos (R$ {totalDepositos:F2}): oferecer orientação de planejamento financeiro.");
            }

            // Polimorfismo: cada tipo de conta sugere sua própria ação.
            string? acaoEspecifica = conta.SugerirAcaoEspecifica(saques.Count, tarifas.Count);
            if (acaoEspecifica != null)
            {
                recomendacoes.Add(acaoEspecifica);
            }

            return recomendacoes;
        }

        // ---------------------------------------------------------------
        // RF10 - Exibição no terminal
        // ---------------------------------------------------------------
        public void ExibirCabecalho()
        {
            Console.WriteLine();
            Console.WriteLine(LinhaDupla);
            Console.WriteLine("        MINIBANCO .NET - MOTOR DE GESTÃO DE CONTAS");
            Console.WriteLine(LinhaDupla);
        }

        public void ExibirRodape()
        {
            Console.WriteLine();
            Console.WriteLine(LinhaDupla);
            Console.WriteLine("        FIM DO PROCESSAMENTO DE CONTAS");
            Console.WriteLine(LinhaDupla);
        }

        public void ExibirRelatorio(Conta conta, int posicao, int totalContas)
        {
            int pontuacaoObtida = CalcularPontuacaoObtida(conta);
            int pontuacaoMaxima = CalcularPontuacaoMaxima(conta);
            double indice = CalcularIndiceSaude(pontuacaoObtida, pontuacaoMaxima);
            string classificacao = Classificar(indice);

            List<Transacao> depositos = FiltrarPorTipo(conta, Transacao.TipoDeposito);
            List<Transacao> saques = FiltrarPorTipo(conta, Transacao.TipoSaque);
            List<Transacao> tarifas = FiltrarPorTipo(conta, Transacao.TipoTarifa);

            Console.WriteLine();
            Console.WriteLine($"[{posicao}/{totalContas}] PROCESSANDO CONTA");
            Console.WriteLine(LinhaSimples);

            // Dados da conta (TipoConta e DescreverAtributosEspecificos são polimórficos)
            Console.WriteLine("> DADOS DA CONTA:");
            Console.WriteLine($"  - Tipo: {conta.TipoConta}");
            Console.WriteLine($"  - Titular: {conta.Titular} | Conta: {conta.Numero}");
            Console.WriteLine($"  - Atributo Específico: {conta.DescreverAtributosEspecificos()}");
            Console.WriteLine($"  - Saldo Atual: R$ {conta.Saldo:F2}");
            Console.WriteLine();

            // Movimentações
            Console.WriteLine($"> MOVIMENTAÇÕES REGISTRADAS ({conta.Extrato.Count} TRANSAÇÕES):");
            if (conta.Extrato.Count == 0)
            {
                Console.WriteLine("  Nenhuma movimentação registrada.");
            }
            foreach (Transacao transacao in conta.Extrato)
            {
                Console.WriteLine("  " + FormatarLinhaTransacao(transacao));
            }
            Console.WriteLine();

            // Resumo da pontuação
            Console.WriteLine("> RESUMO DA PONTUAÇÃO:");
            Console.WriteLine($"  - Pontuação Atingida: {pontuacaoObtida} de {pontuacaoMaxima} pontos possíveis");
            Console.WriteLine($"  - Índice de Saúde: {indice:F1}%");
            if (pontuacaoMaxima == 0)
            {
                Console.WriteLine("    (conta sem movimentações: índice considerado 0%)");
            }
            Console.WriteLine($"  - Classificação Final: [ {classificacao.ToUpper()} ]");
            Console.WriteLine($"  - Ação Corporativa: {ObterAcaoCorporativa(classificacao)}");
            Console.WriteLine();

            // Pendências e recomendações
            Console.WriteLine("> RELATÓRIO DE PENDÊNCIAS E RECOMENDAÇÕES DA GERÊNCIA:");

            if (tarifas.Count == 0 && saques.Count == 0)
            {
                Console.WriteLine("  🟢 Nenhuma pendência identificada. Oferecer produtos de investimento ao cliente.");
            }
            else
            {
                if (tarifas.Count > 0)
                {
                    Console.WriteLine("  🔴 ITENS CRÍTICOS (TARIFAS):");
                    foreach (Transacao tarifa in tarifas)
                    {
                        Console.WriteLine($"    - {conta.AnalisarTarifa(tarifa)}");
                    }
                    Console.WriteLine();
                }

                if (saques.Count > 0)
                {
                    Console.WriteLine("  🟡 ITENS DE ATENÇÃO (SAQUES):");
                    foreach (Transacao saque in saques)
                    {
                        Console.WriteLine($"    - {conta.AnalisarSaque(saque)}");
                    }
                    Console.WriteLine();
                }

                List<string> recomendacoes = GerarRecomendacoes(conta, depositos, saques, tarifas);
                if (recomendacoes.Count > 0)
                {
                    Console.WriteLine("  📋 AÇÕES PRIORITÁRIAS PARA A EQUIPE DE RELACIONAMENTO:");
                    for (int i = 0; i < recomendacoes.Count; i++)
                    {
                        Console.WriteLine($"    {i + 1}. {recomendacoes[i]}");
                    }
                }
            }

            Console.WriteLine(LinhaSimples);
        }

        private string FormatarLinhaTransacao(Transacao transacao)
        {
            string marcador;
            if (transacao.Tipo == Transacao.TipoDeposito)
            {
                marcador = "[OK] ";
            }
            else if (transacao.Tipo == Transacao.TipoSaque)
            {
                marcador = "[ ! ]";
            }
            else
            {
                marcador = "[ X ]";
            }

            // Preenche com traços para alinhar as colunas.
            int quantidadeTracos = 32 - transacao.Descricao.Length;
            if (quantidadeTracos < 3)
            {
                quantidadeTracos = 3;
            }
            string tracos = new string('-', quantidadeTracos);

            int pontos = ObterPontuacao(transacao.Tipo);
            return $"{marcador} {transacao.Descricao} {tracos} {transacao.Tipo}: R$ {transacao.Valor:F2} ({pontos} pts)";
        }
    }
}
