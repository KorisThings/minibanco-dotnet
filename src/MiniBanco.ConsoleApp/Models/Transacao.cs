using System;

namespace MiniBanco.ConsoleApp.Models
{
    /// <summary>
    /// Representa uma movimentação registrada no extrato de uma conta.
    /// </summary>
    public class Transacao
    {
        public const string TipoDeposito = "Deposito";
        public const string TipoSaque = "Saque";
        public const string TipoTarifa = "Tarifa";

        public string Descricao { get; }
        public string Tipo { get; }
        public decimal Valor { get; }

        /// <summary>
        /// Saldo da conta logo após esta movimentação (usado na análise de pendências).
        /// </summary>
        public decimal SaldoApos { get; }

        public Transacao(string descricao, string tipo, decimal valor, decimal saldoApos)
        {
            if (string.IsNullOrWhiteSpace(descricao))
            {
                throw new ArgumentException("A descrição da transação não pode ser vazia.");
            }

            if (tipo != TipoDeposito && tipo != TipoSaque && tipo != TipoTarifa)
            {
                throw new ArgumentException($"Tipo de transação inválido: '{tipo}'. Use Deposito, Saque ou Tarifa.");
            }

            if (valor <= 0)
            {
                throw new ArgumentException("O valor da transação deve ser maior que zero.");
            }

            this.Descricao = descricao;
            this.Tipo = tipo;
            this.Valor = valor;
            this.SaldoApos = saldoApos;
        }
    }
}
