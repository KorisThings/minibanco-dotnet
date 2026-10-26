using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MiniBanco.ConsoleApp.Exceptions;
using MiniBanco.ConsoleApp.Models;
using MiniBanco.ConsoleApp.Services;

namespace MiniBanco.ConsoleApp
{
    public class Program
    {
        // Lista central com todas as contas abertas no banco.
        private static readonly List<Conta> contas = new List<Conta>();
        private static readonly MotorAnalise motor = new MotorAnalise();

        public static void Main(string[] args)
        {
            // Acentos/emojis no terminal e valores no formato brasileiro (R$ 10,50).
            Console.OutputEncoding = Encoding.UTF8;
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");

            bool executando = true;

            while (executando)
            {
                ExibirMenu();
                string? linha = Console.ReadLine();

                // Fim da entrada (Ctrl+Z / Ctrl+D): encerra em vez de repetir o menu para sempre.
                if (linha == null)
                {
                    break;
                }

                string opcao = linha.Trim();

                // RF12: nenhuma entrada inválida ou operação não permitida derruba o programa.
                try
                {
                    switch (opcao)
                    {
                        case "1":
                            AbrirNovaConta();
                            break;
                        case "2":
                            RegistrarMovimentacao();
                            break;
                        case "3":
                            AplicarFechamentoMensal();
                            break;
                        case "4":
                            ExibirRelatorioDasContas();
                            break;
                        case "5":
                            CarregarContasDemonstracao();
                            break;
                        case "0":
                            executando = false;
                            Console.WriteLine("\nEncerrando o MiniBanco. Até logo!");
                            break;
                        default:
                            Console.WriteLine("\n⚠ Opção inválida. Escolha um número do menu.");
                            break;
                    }
                }
                catch (SaldoInsuficienteException ex)
                {
                    Console.WriteLine($"\n❌ Operação não permitida: {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"\n❌ Dado inválido: {ex.Message}");
                }
                catch (FormatException)
                {
                    Console.WriteLine("\n❌ Formato inválido: era esperado um número. Tente novamente.");
                }
                catch (OverflowException)
                {
                    Console.WriteLine("\n❌ Número grande demais. Tente novamente.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Erro inesperado: {ex.Message}");
                }

                if (executando)
                {
                    Console.WriteLine("\nPressione ENTER para voltar ao menu...");
                    Console.ReadLine();
                }
            }
        }

        private static void ExibirMenu()
        {
            Console.WriteLine();
            Console.WriteLine("====================================================================");
            Console.WriteLine("        MINIBANCO .NET - MENU PRINCIPAL");
            Console.WriteLine("====================================================================");
            Console.WriteLine("  1 - Abrir Nova Conta");
            Console.WriteLine("  2 - Registrar Movimentação");
            Console.WriteLine("  3 - Aplicar Fechamento Mensal");
            Console.WriteLine("  4 - Exibir Relatório das Contas");
            Console.WriteLine("  5 - Carregar Contas de Demonstração");
            Console.WriteLine("  0 - Sair");
            Console.WriteLine("--------------------------------------------------------------------");
            Console.Write("Escolha uma opção: ");
        }

        // ---------------------------------------------------------------
        // 1 - Abrir nova conta
        // ---------------------------------------------------------------
        private static void AbrirNovaConta()
        {
            Console.WriteLine("\n--- ABRIR NOVA CONTA ---");
            Console.WriteLine("Tipos disponíveis:");
            Console.WriteLine("  1 - Conta Corrente");
            Console.WriteLine("  2 - Conta Poupança");
            Console.WriteLine("  3 - Conta Salário");
            string tipo = LerTexto("Tipo da conta: ");

            if (tipo != "1" && tipo != "2" && tipo != "3")
            {
                Console.WriteLine("⚠ Tipo de conta inválido. Operação cancelada.");
                return;
            }

            string titular = LerTexto("Titular: ");
            int numero = LerInteiro("Número da conta: ");

            if (BuscarConta(numero) != null)
            {
                Console.WriteLine($"⚠ Já existe uma conta com o número {numero}. Operação cancelada.");
                return;
            }

            decimal saldoInicial = LerDecimal("Saldo inicial (R$): ");

            Conta novaConta;
            if (tipo == "1")
            {
                decimal limite = LerDecimal("Limite do cheque especial (R$): ");
                novaConta = new ContaCorrente(titular, numero, saldoInicial, limite);
            }
            else if (tipo == "2")
            {
                double taxa = LerDouble("Taxa de rendimento mensal (% - ex.: 0,5): ");
                int diaAniversario = LerInteiro("Dia de aniversário da poupança (1 a 31): ");
                novaConta = new ContaPoupanca(titular, numero, saldoInicial, taxa, diaAniversario);
            }
            else
            {
                string empregador = LerTexto("Empregador: ");
                int limiteSaques = LerInteiro("Limite de saques gratuitos por mês: ");
                novaConta = new ContaSalario(titular, numero, saldoInicial, empregador, limiteSaques);
            }

            contas.Add(novaConta);
            Console.WriteLine($"\n✅ {novaConta.TipoConta} nº {novaConta.Numero} aberta para {novaConta.Titular}. Saldo: R$ {novaConta.Saldo:F2}");
        }

        // ---------------------------------------------------------------
        // 2 - Registrar movimentação
        // ---------------------------------------------------------------
        private static void RegistrarMovimentacao()
        {
            Console.WriteLine("\n--- REGISTRAR MOVIMENTAÇÃO ---");
            int numero = LerInteiro("Número da conta: ");

            Conta? conta = BuscarConta(numero);
            if (conta == null)
            {
                Console.WriteLine($"⚠ Conta {numero} não encontrada.");
                return;
            }

            Console.WriteLine($"Conta encontrada: {conta.TipoConta} - {conta.Titular} | Saldo: R$ {conta.Saldo:F2}");
            Console.WriteLine("  1 - Depósito");
            Console.WriteLine("  2 - Saque");
            string operacao = LerTexto("Operação: ");

            if (operacao == "1")
            {
                decimal valor = LerDecimal("Valor do depósito (R$): ");
                Console.Write("Descrição (ENTER para \"Depósito\"): ");
                string descricao = (Console.ReadLine() ?? "").Trim();
                conta.Depositar(valor, descricao);
                Console.WriteLine($"\n✅ Depósito realizado. Novo saldo: R$ {conta.Saldo:F2}");
            }
            else if (operacao == "2")
            {
                decimal valor = LerDecimal("Valor do saque (R$): ");
                conta.Sacar(valor); // Polimorfismo: cada tipo de conta aplica sua regra.
                Console.WriteLine($"\n✅ Saque realizado. Novo saldo: R$ {conta.Saldo:F2}");
            }
            else
            {
                Console.WriteLine("⚠ Operação inválida. Escolha 1 (Depósito) ou 2 (Saque).");
            }
        }

        // ---------------------------------------------------------------
        // 3 - Fechamento mensal (polimorfismo)
        // ---------------------------------------------------------------
        private static void AplicarFechamentoMensal()
        {
            if (contas.Count == 0)
            {
                Console.WriteLine("\nNenhuma conta aberta até o momento.");
                return;
            }

            Console.WriteLine("\n--- FECHAMENTO MENSAL ---");
            foreach (Conta conta in contas)
            {
                // Não verificamos o tipo: cada subclasse sabe fazer o seu fechamento.
                conta.AplicarFechamentoMensal();
                Console.WriteLine($"  ✔ {conta.TipoConta} nº {conta.Numero} ({conta.Titular}) - Saldo após fechamento: R$ {conta.Saldo:F2}");
            }
            Console.WriteLine("\n✅ Fechamento mensal aplicado a todas as contas.");
        }

        // ---------------------------------------------------------------
        // 4 - Relatório
        // ---------------------------------------------------------------
        private static void ExibirRelatorioDasContas()
        {
            if (contas.Count == 0)
            {
                Console.WriteLine("\nNenhuma conta aberta até o momento.");
                return;
            }

            motor.ExibirCabecalho();
            for (int i = 0; i < contas.Count; i++)
            {
                motor.ExibirRelatorio(contas[i], i + 1, contas.Count);
            }
            motor.ExibirRodape();
        }

        // ---------------------------------------------------------------
        // 5 - Dados de demonstração (facilita testes e a gravação do vídeo)
        // ---------------------------------------------------------------
        private static void CarregarContasDemonstracao()
        {
            if (BuscarConta(1001) != null || BuscarConta(2001) != null || BuscarConta(3001) != null)
            {
                Console.WriteLine("\n⚠ As contas de demonstração já foram carregadas.");
                return;
            }

            ContaCorrente ana = new ContaCorrente("Ana Souza", 1001, 1000m, 500m);
            ana.Depositar(2000m, "Salário");
            ana.Depositar(300m, "Transferência recebida");
            ana.Sacar(800m);
            ana.Sacar(2800m);

            ContaPoupanca bia = new ContaPoupanca("Bia Lima", 2001, 2000m, 0.5, 10);
            bia.Depositar(500m, "Depósito em dinheiro");
            bia.Depositar(300m, "Transferência recebida");

            ContaSalario carlos = new ContaSalario("Carlos Pereira", 3001, 0m, "Tech Soluções Ltda", 2);
            carlos.Depositar(3000m, "Salário - Tech Soluções");
            carlos.Sacar(500m);
            carlos.Sacar(300m);
            carlos.Sacar(200m); // 3º saque: ultrapassa o limite e gera tarifa

            contas.Add(ana);
            contas.Add(bia);
            contas.Add(carlos);

            Console.WriteLine("\n✅ 3 contas de demonstração carregadas (1001, 2001 e 3001).");
            Console.WriteLine("   Dica: use a opção 3 (fechamento mensal) e depois a 4 (relatório).");
        }

        // ---------------------------------------------------------------
        // Auxiliares
        // ---------------------------------------------------------------
        private static Conta? BuscarConta(int numero)
        {
            foreach (Conta conta in contas)
            {
                if (conta.Numero == numero)
                {
                    return conta;
                }
            }
            return null;
        }

        private static string LerTexto(string mensagem)
        {
            Console.Write(mensagem);
            return (Console.ReadLine() ?? "").Trim();
        }

        // int.Parse / decimal.Parse lançam FormatException se o texto não for número,
        // que é tratada no try/catch do menu.
        private static int LerInteiro(string mensagem)
        {
            return int.Parse(LerTexto(mensagem));
        }

        private static decimal LerDecimal(string mensagem)
        {
            return decimal.Parse(NormalizarNumero(LerTexto(mensagem)));
        }

        private static double LerDouble(string mensagem)
        {
            return double.Parse(NormalizarNumero(LerTexto(mensagem)));
        }

        // Aceita "10,50" e também "10.50" (troca o ponto por vírgula quando não há vírgula).
        private static string NormalizarNumero(string texto)
        {
            if (texto.Contains('.') && !texto.Contains(','))
            {
                return texto.Replace('.', ',');
            }
            return texto;
        }
    }
}
