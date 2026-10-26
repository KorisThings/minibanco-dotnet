# MiniBanco.ConsoleApp — Motor de Gestão de Contas Bancárias

Mini-Projeto Avaliativo — **Desenvolvedor Back-End [.NET] [T2] — SENAI — Módulo 2, Semana 07**

| | |
|---|---|
| **Aluno** | Ruan da Rosa Correia |
| **Turma** | QA DBEN 2026/1 5 |

---

## 🎯 Objetivo

Aplicação de console em C# que simula o motor de processamento de um banco digital. O sistema:

- abre contas de três tipos (**Corrente**, **Poupança** e **Salário**), cada uma com regras próprias;
- registra movimentações (**Depósito**, **Saque** e **Tarifa**) no extrato de cada conta;
- aplica o **fechamento mensal** de cada tipo de conta (tarifa, rendimento, reinício da contagem de saques);
- calcula a **pontuação** e o **índice de saúde financeira** de cada conta;
- **classifica** a conta (Saudável, Atenção ou Risco);
- emite um **relatório** com pendências e recomendações para a gerência.

## ⚙️ Funcionalidades (menu principal)

| Opção | Ação |
|---|---|
| **1** | Abrir nova conta (Corrente, Poupança ou Salário) |
| **2** | Registrar movimentação (depósito ou saque) em uma conta existente |
| **3** | Aplicar fechamento mensal em todas as contas (polimorfismo) |
| **4** | Exibir relatório detalhado de todas as contas |
| **5** | Carregar contas de demonstração (atalho para testes) |
| **0** | Sair |

O menu roda dentro de um laço `while` e trata todas as exceções, então o programa **não fecha** por causa de entrada inválida.

## 📁 Estrutura do projeto

```
minibanco-dotnet/
├── src/
│   └── MiniBanco.ConsoleApp/
│       ├── Program.cs                     # Menu principal, leitura de dados e try/catch
│       ├── Models/
│       │   ├── Transacao.cs               # Modelo da movimentação (com validação)
│       │   ├── Conta.cs                   # Classe base abstrata
│       │   ├── ContaCorrente.cs           # Cheque especial + tarifa de manutenção
│       │   ├── ContaPoupanca.cs           # Rendimento mensal
│       │   └── ContaSalario.cs            # Limite de saques mensais
│       ├── Exceptions/
│       │   └── SaldoInsuficienteException.cs
│       ├── Services/
│       │   └── MotorAnalise.cs            # Pontuação, índice, classificação e relatório
│       └── MiniBanco.ConsoleApp.csproj
└── README.md
```

## ▶️ Como executar

**Pré-requisito:** [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior.

```bash
git clone https://github.com/KorisThings/minibanco-dotnet.git
cd minibanco-dotnet/src/MiniBanco.ConsoleApp
dotnet run
```

**Roteiro rápido para testar:** escolha `5` (carrega 3 contas de exemplo) → `3` (fechamento mensal) → `4` (relatório).

> Valores podem ser digitados com vírgula (`10,50`) ou ponto (`10.50`). Não use separador de milhar.

## 📏 Regras de negócio

### Transação
- Tipos aceitos: `Deposito`, `Saque` ou `Tarifa`.
- Valor sempre maior que zero. Tipo ou valor inválido lança `ArgumentException`.

### Conta (classe base)
- `Saldo` pode ser lido por qualquer classe, mas só é alterado pela própria conta e por suas subclasses (`protected set`).
- O saldo inicial informado na abertura **não** entra no extrato como transação (é o saldo de partida da conta).
- Saque padrão: só é permitido até o valor do saldo; caso contrário lança `SaldoInsuficienteException`.

### Regras por tipo de conta

| Tipo | Atributos próprios | Regra de saque | Fechamento mensal |
|---|---|---|---|
| **Conta Corrente** | `LimiteChequeEspecial` | Pode sacar até **saldo + limite** | Cobra **tarifa de manutenção de R$ 10,00** |
| **Conta Poupança** | `TaxaRendimentoMensal` (%), `DiaAniversario` | Regra padrão (até o saldo) | Deposita **rendimento = saldo × taxa / 100** |
| **Conta Salário** | `Empregador`, `LimiteSaquesMensais` | A partir do saque que **ultrapassa o limite mensal**, cobra **tarifa de R$ 5,00** | **Zera a contagem de saques** do mês (sem tarifa) |

### Pontuação e índice de saúde

| Tipo da transação | Pontos |
|---|---|
| Deposito | 10 |
| Saque | 5 |
| Tarifa | 0 |

```
Pontuação máxima = Total de transações × 10
Índice de saúde (%) = (Pontuação obtida / Pontuação máxima) × 100
```

- A divisão usa cast para `double` (sem ele, a divisão de inteiros truncaria para 0).
- Conta **sem transações**: o índice é considerado 0% para evitar divisão por zero.

### Classificação

| Índice | Classificação | Ação corporativa |
|---|---|---|
| 90% a 100% | **Conta Saudável** | Oferecer investimentos e avaliar aumento de limite. |
| 60% a 89% | **Conta em Atenção** | Contato da gerência para orientação financeira e revisão de tarifas. |
| 0% a 59% | **Conta em Risco** | Bloquear aumento de limite e abrir análise de crédito. |

### Relatório de pendências e recomendações
- 🔴 **Itens críticos:** todas as transações do tipo `Tarifa`.
- 🟡 **Itens de atenção:** todas as transações do tipo `Saque` (na conta corrente, indica quando o saque usou o cheque especial).
- 📋 **Ações prioritárias:** saldo negativo, revisão do pacote de tarifas, saques maiores que depósitos e uma recomendação específica de cada tipo de conta (ex.: sugerir migração da Conta Salário para Conta Corrente).

## 🧱 Conceitos de C# e POO aplicados

| Conceito | Onde aparece |
|---|---|
| Tipos primitivos | `string` (Titular), `int` (Numero), `decimal` (Saldo/Valor), `double` (taxa e índice), `bool` (controle do menu) |
| `List<T>` | `List<Transacao> Extrato` em cada conta e `List<Conta>` central no `Program` |
| Laços `foreach` / `for` / `while` | Cálculo de pontos, filtros, busca de contas e menu |
| `if/else` e `switch` | Pontuação por tipo, faixas de classificação e opções do menu |
| Construtores com `this` | Todas as classes de modelo |
| Herança (`:`) | `ContaCorrente`, `ContaPoupanca` e `ContaSalario` herdam de `Conta` |
| `virtual` / `override` | `Sacar()`, `AplicarFechamentoMensal()` e os métodos de análise do relatório |
| Encapsulamento | `{ get; }`, `{ get; protected set; }`, `{ get; private set; }` e `RegistrarTransacao` protegido |
| Exceções | `SaldoInsuficienteException` (própria), `ArgumentException` e `FormatException` tratadas no menu |
| Sem LINQ | Toda busca/filtragem usa laços tradicionais; o `.csproj` desativa os *implicit usings* para não importar `System.Linq` |

## ⚠️ Limitações e melhorias futuras

- Os dados ficam apenas em memória: ao fechar o programa, as contas são perdidas (não há banco de dados nem arquivo).
- Não há controle de datas: o "mês" é definido por quando o usuário escolhe a opção de fechamento mensal.
- O fechamento mensal pode ser aplicado várias vezes seguidas (cada execução gera nova tarifa/rendimento).
- Não há operação de transferência entre contas nem encerramento de conta.
- Não há testes automatizados (uma melhoria seria criar um projeto xUnit para as regras de saque e pontuação).
