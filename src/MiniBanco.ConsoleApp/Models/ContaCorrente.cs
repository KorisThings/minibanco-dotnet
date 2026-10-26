using System;
using MiniBanco.ConsoleApp.Exceptions;

namespace MiniBanco.ConsoleApp.Models
{
    public class ContaCorrente : Conta
    {
        public const decimal TarifaManutencao = 10.00m;

        public decimal LimiteChequeEspecial { get; private set; }

        public ContaCorrente(string titular, int numero, decimal saldoInicial, decimal limiteChequeEspecial)
            : base(titular, numero, saldoInicial)
        {
            if (limiteChequeEspecial < 0)
            {
                throw new ArgumentException("O limite do cheque especial não pode ser negativo.");
            }

            this.LimiteChequeEspecial = limiteChequeEspecial;
        }

        public override string TipoConta
        {
            get { return "Conta Corrente"; }
        }

        public override string DescreverAtributosEspecificos()
        {
            return $"Limite Cheque Especial: R$ {this.LimiteChequeEspecial:F2}";
        }

        /// <summary>
        /// Permite sacar até o saldo + limite do cheque especial.
        /// </summary>
        public override void Sacar(decimal valor)
        {
            ValidarValorPositivo(valor);

            decimal disponivel = this.Saldo + this.LimiteChequeEspecial;
            if (valor > disponivel)
            {
                throw new SaldoInsuficienteException(valor, disponivel);
            }

            this.Saldo -= valor;
            RegistrarTransacao("Saque", Transacao.TipoSaque, valor);
        }

        /// <summary>
        /// Fechamento da conta corrente: cobra a tarifa de manutenção mensal.
        /// </summary>
        public override void AplicarFechamentoMensal()
        {
            this.Saldo -= TarifaManutencao;
            RegistrarTransacao("Tarifa de manutenção", Transacao.TipoTarifa, TarifaManutencao);
        }

        public override string AnalisarSaque(Transacao saque)
        {
            if (saque.SaldoApos < 0)
            {
                return $"Saque de R$ {saque.Valor:F2}: Conta usando limite do cheque especial. Entrar em contato com o cliente.";
            }

            return base.AnalisarSaque(saque);
        }

        public override string AnalisarTarifa(Transacao tarifa)
        {
            return $"{tarifa.Descricao}: Avaliar isenção ou migração para pacote de serviços.";
        }

        public override string? SugerirAcaoEspecifica(int quantidadeSaques, int quantidadeTarifas)
        {
            if (this.Saldo < 0)
            {
                return "Cliente está no cheque especial: oferecer troca da dívida por crédito pessoal com juros menores.";
            }

            return null;
        }
    }
}
