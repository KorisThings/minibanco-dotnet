using System;

namespace MiniBanco.ConsoleApp.Models
{
    public class ContaPoupanca : Conta
    {
        /// <summary>Taxa de rendimento mensal, em porcentagem (ex.: 0.5 = 0,5% ao mês).</summary>
        public double TaxaRendimentoMensal { get; private set; }

        public int DiaAniversario { get; private set; }

        public ContaPoupanca(string titular, int numero, decimal saldoInicial, double taxaRendimentoMensal, int diaAniversario)
            : base(titular, numero, saldoInicial)
        {
            if (taxaRendimentoMensal < 0)
            {
                throw new ArgumentException("A taxa de rendimento não pode ser negativa.");
            }

            if (diaAniversario < 1 || diaAniversario > 31)
            {
                throw new ArgumentException("O dia de aniversário deve estar entre 1 e 31.");
            }

            this.TaxaRendimentoMensal = taxaRendimentoMensal;
            this.DiaAniversario = diaAniversario;
        }

        public override string TipoConta
        {
            get { return "Conta Poupança"; }
        }

        public override string DescreverAtributosEspecificos()
        {
            return $"Rendimento: {this.TaxaRendimentoMensal:0.##}% ao mês | Aniversário: dia {this.DiaAniversario}";
        }

        /// <summary>
        /// Fechamento da poupança: deposita o rendimento (saldo × taxa / 100).
        /// O saque usa a regra padrão da classe base (não pode ficar negativo).
        /// </summary>
        public override void AplicarFechamentoMensal()
        {
            decimal rendimento = Math.Round(this.Saldo * (decimal)this.TaxaRendimentoMensal / 100m, 2);

            // Sem saldo positivo (ou taxa zero) não há rendimento a registrar.
            if (rendimento > 0)
            {
                Depositar(rendimento, "Rendimento mensal");
            }
        }

        public override string AnalisarSaque(Transacao saque)
        {
            return $"Saque de R$ {saque.Valor:F2}: Retirada da poupança reduz o rendimento. Acompanhar o cliente.";
        }

        public override string? SugerirAcaoEspecifica(int quantidadeSaques, int quantidadeTarifas)
        {
            if (quantidadeSaques > 0)
            {
                return $"Orientar o cliente a concentrar retiradas após o aniversário da conta (dia {this.DiaAniversario}) para não perder rendimento.";
            }

            return null;
        }
    }
}
