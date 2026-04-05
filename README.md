# TCC Indaiatuba - Backend

Backend do projeto de TCC (MBA) para plataforma de zeladoria urbana, estruturado em microservicos com .NET 10.

## Sobre o projeto

Este repositorio contem o backend da aplicacao, responsavel por:

- autenticacao de usuarios
- registro e consulta de ocorrencias
- roteamento centralizado via API Gateway
- envio de notificacoes push (Firebase)

## Contexto academico

Projeto desenvolvido como parte do Trabalho de Conclusao de Curso (TCC) do MBA.

## Hospedagem

Este backend esta hospedado em servidor Oracle Cloud Infrastructure (OCI), em ambiente de producao controlado pelos autores do projeto.

## Arquitetura

Servicos principais:

- `Zeladoria.Identity.API`: autenticacao/autorizacao e usuarios
- `Zeladoria.Ocorrencias.API`: regras de negocio de ocorrencias
- `Zeladoria.Gateway.API`: gateway reverso com YARP
- `Zeladoria.Application`: camada de aplicacao (DTOs e casos de uso)
- `Zeladoria.Domain`: entidades e contratos
- `Zeladoria.Infrastructure`: persistencia, repositorios e migracoes EF Core

## Tecnologias

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker e Docker Compose
- Firebase Admin SDK
- YARP (Reverse Proxy)

## Como clonar e criar seu proprio ambiente

### 1. Pre-requisitos

Instale no seu computador:

- Git
- .NET SDK 10
- Docker Desktop (com Docker Compose)
- PostgreSQL (caso rode sem Docker para banco remoto/local)

### 2. Clonar o repositorio

```bash
git clone <URL_DO_SEU_REPOSITORIO>
cd TCC-Indaiatuba-Backend
```

### 3. Configurar variaveis de ambiente

Crie um arquivo `.env` na raiz do projeto com base no `.env.example`:

```env
DEFAULT_CONNECTION="Host=SEU_HOST;Port=5432;Database=SEU_BANCO;Username=SEU_USUARIO;Password=SUA_SENHA"
JWT_KEY="SUA_CHAVE_JWT_FORTE"
```

Observacoes:

- `DEFAULT_CONNECTION` e usada pelos servicos `Identity` e `Ocorrencias`.
- A chave JWT deve ser forte e mantida em segredo.
- Em producao, prefira secrets manager/variaveis seguras em vez de arquivo local.

### 4. Configurar credenciais Firebase

O projeto utiliza `firebase-key.json` para login Google e push notifications.

Para executar com Docker Compose, mantenha um arquivo `firebase-key.json` na raiz do repositorio (ele e montado nos containers).

Para executar via `dotnet run`, mantenha `firebase-key.json` dentro de:

- `Zeladoria.Identity.API/`
- `Zeladoria.Ocorrencias.API/`

Importante: nunca versione chaves reais em repositorios publicos.

## Executando o projeto

### Opcao A - Docker Compose (recomendado)

Na raiz do projeto:

```bash
docker compose up --build -d
```

Gateway exposto em:

- `http://localhost:9000`

Para parar:

```bash
docker compose down
```

### Opcao B - Execucao local com dotnet

Em terminais separados:

```bash
dotnet run --project Zeladoria.Identity.API
dotnet run --project Zeladoria.Ocorrencias.API
dotnet run --project Zeladoria.Gateway.API
```

Portas padrao em desenvolvimento:

- Identity API: `http://localhost:5079`
- Ocorrencias API: `http://localhost:5260`
- Gateway API: `http://localhost:9000`

## Banco de dados e migracoes

As migracoes estao em `Zeladoria.Infrastructure/Migrations`.

Para aplicar migracoes (se necessario):

```bash
dotnet ef database update --project Zeladoria.Infrastructure --startup-project Zeladoria.Identity.API
```

Se precisar, repita trocando `--startup-project` para `Zeladoria.Ocorrencias.API` conforme seu fluxo.

## Rotas via Gateway

Principais prefixos encaminhados pelo gateway:

- `api/Auth/*`
- `api/Usuarios/*`
- `api/Ocorrencias/*`

## Estrutura resumida

```text
Zeladoria.Application/
Zeladoria.Domain/
Zeladoria.Infrastructure/
Zeladoria.Identity.API/
Zeladoria.Ocorrencias.API/
Zeladoria.Gateway.API/
docker-compose.yml
```

## Boas praticas para quem for reutilizar

- altere todas as chaves e segredos antes de subir seu ambiente
- use banco separado por ambiente (dev, hml, prod)
- habilite logs e monitoramento para facilitar operacao
- configure CORS e politicas de seguranca conforme seu frontend

## Autor e finalidade

Projeto academico de TCC do MBA, disponibilizado para demonstracao tecnica e apoio a reproducao de ambiente para estudos.
