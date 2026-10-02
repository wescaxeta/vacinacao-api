# ADR 0001: Arquitetura em camadas, sem MediatR

- **Status:** aceita
- **Data:** 2026-10-02

## Contexto

Projetos .NET costumam adotar Clean Architecture com MediatR (um handler por comando/consulta),
AutoMapper e repositórios genéricos. Para uma API com 3 casos de uso, esse conjunto adiciona
indireção sem resolver nenhum problema concreto.

## Decisão

- **Quatro projetos**: `Domain` (regras puras, sem dependências), `Application` (casos de uso e
  contratos), `Infrastructure` (EF Core e PostgreSQL) e `Api` (Minimal APIs). As referências apontam
  para dentro: o domínio não conhece EF Core nem HTTP.
- **Serviços de aplicação simples** (`CampanhaService`, `VacinacaoService`, `AptidaoService`), injetados
  direto nos endpoints. Sem MediatR.
- **Mapeamento manual** para DTOs com métodos `De(...)`. Sem AutoMapper: o mapeamento fica explícito,
  verificado pelo compilador e fácil de depurar.
- **Repositórios específicos** por agregado, com apenas as consultas que os casos de uso usam.
- A regra central (`AvaliadorAptidao`) é uma **função pura**: recebe campanhas, registros e a data de
  hoje, e devolve o resultado. Não depende de banco nem de relógio, então os testes cobrem todos os
  cenários sem infraestrutura.

## Consequências

- Menos código e menos "mágica"; quem chega ao projeto entende o fluxo seguindo as chamadas.
- Se os casos de uso crescerem e surgirem preocupações transversais (log, transação, autorização por
  caso de uso), MediatR ou *decorators* passam a valer a pena. A separação em camadas já permite essa troca.
