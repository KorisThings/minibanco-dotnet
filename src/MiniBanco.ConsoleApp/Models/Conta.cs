using System;
using System.Collections.Generic;
using MiniBanco.ConsoleApp.Exceptions;

namespace MiniBanco.ConsoleApp.Models
{
    /// <summary>
    /// Classe base (abstrata) de todas as contas do banco.
    /// Não pode ser instanciada diretamente: é preciso escolher um tipo de conta.
    /// </summary>
    public abstract class Conta
    {
        public string Titular { get; }
        public int Numero { get; }

        // Encapsulamento: qualquer um pode ler o saldo,
        // mas só a própria Conta e suas subclasses podem alterá-lo.
        public decimal Saldo { get; protected set; }

        public List<Transacao> Extrato { get; }

        protected Conta(string titular, int numero, decimal saldoInicial)
        {
            if (string.IsNullOrWhiteSpace(titular))
            {
                throw new ArgumentException("O nome do titular é obrigatório.");
            }

            if (numero <= 0)
            {
                throw new ArgumentException("O número da conta deve ser maior que zero.");
            }

            if (saldoInicial < 0)
            {
                throw new ArgumentException("O saldo inicial não pode ser negativo.");
            }

            this.Titular = titular.Trim();
            this.Numero = numero;
            this.Saldo = saldoInicial;
            this.Extrato = new List<Transacao>();
        }

        /// <summary>Nome amigável do tipo de conta, exibido no relatório.</summary>
        public abstract string TipoConta { get; }

        /// <summary>Texto com os atributos próprios de cada subclasse.</summary>
        public abstract string DescreverAtributosEspecificos();

        /// <summary>
        /// Cria a transação (que valida tipo e valor) e a adiciona ao extrato.
        /// </summary>
        protected void RegistrarTransacao(string descricao, string tipo, decimal valor)
        {
            Transacao transacao = new Transacao(descricao, tipo, valor, this.Saldo);
            this.Extrato.Add(transacao);
        }

        protected void ValidarValorPositivo(decimal valor)
        {
            if (valor <= 0)
            {
                throw new ArgumentException("O valor da operação deve ser maior que zero.");
            }
        }

        public void Depositar(decimal valor, string descricao)
        {
            ValidarValorPositivo(valor);

            if (string.IsNullOrWhiteSpace(descricao))
            {
                descricao = "Depósito";
            }

            this.Saldo += valor;
            RegistrarTransacao(descricao, Transacao.TipoDeposito, valor);
        }

        /// <summary>
        /// Regra padrão: só permite sacar até o saldo disponível.
        /// As subclasses podem sobrescrever (override) com regras próprias.
        /// </summary>
        public virtual void Sacar(decimal valor)
        {
            ValidarValorPositivo(valor);

            if (valor > this.Saldo)
            {
                throw new SaldoInsuficienteException(valor, this.Saldo);
            }

            this.Saldo -= valor;
            RegistrarTransacao("Saque", Transacao.TipoSaque, valor);
        }

        /// <summary>
        /// Cada tipo de conta executa seu fechamento à sua maneira (tarifa, rendimento etc.).
        /// Por padrão, não faz nada.
        /// </summary>
        public virtual void AplicarFechamentoMensal()
        {
        }

        /// <summary>Apontamento do relatório para uma transação do tipo Saque.</summary>
        public virtual string AnalisarSaque(Transacao saque)
        {
            return $"Saque de R$ {saque.Valor:F2}: Acompanhar a frequência das retiradas.";
        }

        /// <summary>Apontamento do relatório para uma transação do tipo Tarifa.</summary>
        public virtual string AnalisarTarifa(Transacao tarifa)
        {
            return $"{tarifa.Descricao}: Revisar a cobrança com o cliente.";
        }

        /// <summary>
        /// Recomendação extra para a gerência, específica de cada tipo de conta.
        /// Retorna null quando não há nada específico a recomendar.
        /// </summary>
        public virtual string? SugerirAcaoEspecifica(int quantidadeSaques, int quantidadeTarifas)
        {
            return null;
        }
    }
}
