using System;
using MiniBanco.ConsoleApp.Exceptions;

namespace MiniBanco.ConsoleApp.Models
{
    public class ContaSalario : Conta
    {
        public const decimal TarifaSaqueExcedente = 5.00m;

        public string Empregador { get; private set; }
        public int LimiteSaquesMensais { get; private set; }

        // Contagem interna: só a própria classe altera.
        public int SaquesNoMes { get; private set; }

        public ContaSalario(string titular, int numero, decimal saldoInicial, string empregador, int limiteSaquesMensais)
            : base(titular, numero, saldoInicial)
        {
            if (string.IsNullOrWhiteSpace(empregador))
            {
                throw new ArgumentException("O nome do empregador é obrigatório.");
            }

            if (limiteSaquesMensais < 0)
            {
                throw new ArgumentException("O limite de saques mensais não pode ser negativo.");
            }

            this.Empregador = empregador.Trim();
            this.LimiteSaquesMensais = limiteSaquesMensais;
            this.SaquesNoMes = 0;
        }

        public override string TipoConta
        {
            get { return "Conta Salário"; }
        }

        public override string DescreverAtributosEspecificos()
        {
            return $"Empregador: {this.Empregador} | Saques gratuitos: {this.LimiteSaquesMensais}/mês (usados: {this.SaquesNoMes})";
        }

        /// <summary>
        /// A partir do saque que ultrapassa o limite mensal, cobra também uma tarifa de R$ 5,00.
        /// </summary>
        public override void Sacar(decimal valor)
        {
            ValidarValorPositivo(valor);

            bool ultrapassaLimite = this.SaquesNoMes + 1 > this.LimiteSaquesMensais;
            decimal tarifa = ultrapassaLimite ? TarifaSaqueExcedente : 0m;

            // Verifica antes se há saldo para o saque + tarifa, para não sacar pela metade.
            if (valor + tarifa > this.Saldo)
            {
                throw new SaldoInsuficienteException(valor + tarifa, this.Saldo);
            }

            base.Sacar(valor);
            this.SaquesNoMes++;

            if (ultrapassaLimite)
            {
                this.Saldo -= tarifa;
                RegistrarTransacao("Tarifa por saque excedente", Transacao.TipoTarifa, tarifa);
            }
        }

        /// <summary>
        /// Fechamento da conta salário: zera a contagem de saques do mês (sem tarifa de manutenção).
        /// </summary>
        public override void AplicarFechamentoMensal()
        {
            this.SaquesNoMes = 0;
        }

        public override string AnalisarTarifa(Transacao tarifa)
        {
            return $"{tarifa.Descricao}: Orientar o cliente sobre o limite de {this.LimiteSaquesMensais} saque(s) gratuito(s) por mês.";
        }

        public override string? SugerirAcaoEspecifica(int quantidadeSaques, int quantidadeTarifas)
        {
            if (quantidadeTarifas > 0)
            {
                return "Cliente ultrapassa o limite de saques da conta salário: sugerir migração para Conta Corrente com pacote de serviços.";
            }

            return null;
        }
    }
}
