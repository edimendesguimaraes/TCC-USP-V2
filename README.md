# TCC — Plataforma de Zeladoria Urbana de Indaiatuba

Projeto de Trabalho de Conclusão de Curso (TCC / MBA) que implementa uma plataforma de zeladoria urbana com aplicativo mobile Flutter e backend em microserviços .NET hospedado na Oracle Cloud Infrastructure (OCI).

---

## Índice

1. [Visão geral da arquitetura](#1-visão-geral-da-arquitetura)
2. [Pré-requisitos](#2-pré-requisitos)
3. [Clonar os repositórios](#3-clonar-os-repositórios)
4. [Configurar o projeto Firebase](#4-configurar-o-projeto-firebase)
5. [Provisionar a VM na Oracle Cloud](#5-provisionar-a-vm-na-oracle-cloud)
6. [Instalar dependências na VM](#6-instalar-dependências-na-vm)
7. [Configurar e subir o Backend](#7-configurar-e-subir-o-backend)
8. [Configurar e rodar o Frontend Flutter](#8-configurar-e-rodar-o-frontend-flutter)
9. [Verificação final](#9-verificação-final)
10. [Estrutura de diretórios](#10-estrutura-de-diretórios)

---

## 1. Visão geral da arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│                     Oracle Cloud VM (Ubuntu)                    │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    Docker Compose                         │  │
│  │                                                           │  │
│  │  ┌─────────────────┐     ┌──────────────────────────┐   │  │
│  │  │  Gateway API     │────▶│  Identity API            │   │  │
│  │  │  (YARP)          │     │  (Auth + Usuários)       │   │  │
│  │  │  porta 9000      │     │  porta interna 8080      │   │  │
│  │  │                  │     └──────────────────────────┘   │  │
│  │  │                  │     ┌──────────────────────────┐   │  │
│  │  │                  │────▶│  Ocorrencias API         │   │  │
│  │  └─────────────────┘     │  porta interna 8080      │   │  │
│  │                           └───────────┬──────────────┘   │  │
│  └───────────────────────────────────────┼──────────────────┘  │
│                                          │                      │
│  ┌───────────────────────────────────────▼──────────────────┐  │
│  │  PostgreSQL (porta 5432 ou externa via .env)              │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │ HTTPS / porta 9000
                              │
          ┌───────────────────┴───────────────────┐
          │          Aplicativo Flutter            │
          │   (Android / iOS — autenticação        │
          │   Google, mapa, push notifications)    │
          └───────────────────────────────────────┘
                              ▲
                              │
          ┌───────────────────┴───────────────────┐
          │              Firebase                  │
          │  Auth · Storage · Firestore            │
          │  Messaging (FCM) · Analytics           │
          └───────────────────────────────────────┘
```

**Componentes:**

| Projeto                     | Responsabilidade                                                  |
| --------------------------- | ----------------------------------------------------------------- |
| `Zeladoria.Gateway.API`     | Roteamento centralizado via YARP (Reverse Proxy) — porta 9000     |
| `Zeladoria.Identity.API`    | Autenticação JWT, login com Google (Firebase), gestão de usuários |
| `Zeladoria.Ocorrencias.API` | CRUD de ocorrências urbanas, envio de push via FCM                |
| `Zeladoria.Application`     | DTOs e casos de uso (camada de aplicação)                         |
| `Zeladoria.Domain`          | Entidades, enums e interfaces de domínio                          |
| `Zeladoria.Infrastructure`  | EF Core, repositórios, migrações, PostgreSQL                      |
| `tcc_app`                   | App Flutter — login Google, mapa, câmera, push notifications      |

---

## 2. Pré-requisitos

### Na sua máquina local (desenvolvimento)

| Ferramenta     | Versão mínima      | Instalação                                                                                  |
| -------------- | ------------------ | ------------------------------------------------------------------------------------------- |
| Git            | qualquer           | https://git-scm.com                                                                         |
| .NET SDK       | 10.0               | https://dotnet.microsoft.com/download                                                       |
| Flutter SDK    | 3.11+ (Dart ^3.11) | https://docs.flutter.dev/get-started/install                                                |
| Android Studio | qualquer           | https://developer.android.com/studio (para emulador ou build Android)                       |
| Docker Desktop | qualquer           | https://www.docker.com/products/docker-desktop (opcional — para rodar o backend localmente) |

### Na VM Oracle Cloud (servidor de produção)

| Ferramenta        | Como instalar                       |
| ----------------- | ----------------------------------- |
| Docker Engine     | `apt install docker.io`             |
| Docker Compose v2 | `apt install docker-compose-plugin` |
| Git               | `apt install git`                   |

---

## 3. Clonar os repositórios

O projeto é dividido em dois repositórios separados:

````bash
# Backend
git clone https://github.com/edimendesguimaraes/TCC-USP-V2.git

# Frontend
git clone https://github.com/edimendesguimaraes/TCC-USP-FRONT.git

## 4. Configurar o projeto Firebase

Ambos os projetos (backend e frontend) dependem de um projeto Firebase próprio.
Siga estes passos **uma única vez** para criar e exportar as credenciais:

### 4.1 Criar projeto no Firebase Console

1. Acesse https://console.firebase.google.com e clique em **Adicionar projeto**.
2. Dê um nome (ex: `zeladoria-tcc`) e finalize o assistente.

### 4.2 Ativar os serviços necessários

No painel do projeto Firebase, habilite:

- **Authentication** → provedor **Google**
- **Cloud Firestore** → modo produção ou teste
- **Firebase Storage**
- **Cloud Messaging (FCM)**
- **Analytics** (opcional)
- **Crashlytics** (opcional)

### 4.3 Gerar a chave de serviço (backend)

1. No Firebase Console, vá em **Configurações do projeto → Contas de serviço**.
2. Clique em **Gerar nova chave privada** → baixe o arquivo JSON.
3. Renomeie para `firebase-key.json`.

Você precisará deste arquivo em dois lugares:

- `TCC-Indaiatuba-Backend/firebase-key.json` (usado pelo Docker Compose)
- `TCC-Indaiatuba-Backend/Zeladoria.Identity.API/firebase-key.json` (execução local sem Docker)
- `TCC-Indaiatuba-Backend/Zeladoria.Ocorrencias.API/firebase-key.json` (execução local sem Docker)

> **Nunca versione este arquivo.** Ele já está no `.gitignore`.

### 4.4 Gerar o `firebase_options.dart` (frontend Flutter)

Com o Flutter e o FlutterFire CLI instalados:

```bash
# Instalar o FlutterFire CLI (apenas uma vez)
dart pub global activate flutterfire_cli

# Dentro da pasta do app
cd TCC-FRONT/tcc_app
flutterfire configure
````

O comando vai guiá-lo para selecionar o projeto Firebase e gerar automaticamente o arquivo `lib/firebase_options.dart`.

Alternativamente, copie o exemplo e preencha manualmente:

```bash
cp lib/firebase_options.dart.example lib/firebase_options.dart
# Edite o arquivo com os dados do seu projeto Firebase
```

---

## 5. Provisionar a VM na Oracle Cloud

### 5.1 Criar conta e acessar a OCI

1. Acesse https://cloud.oracle.com e crie uma conta gratuita (Free Tier).
2. Faça login e vá em **Compute → Instâncias → Criar Instância**.

### 5.2 Configurar a instância

| Campo               | Valor recomendado                                                                                     |
| ------------------- | ----------------------------------------------------------------------------------------------------- |
| Nome                | `vm-zeladoria-tcc`                                                                                    |
| Imagem              | **Ubuntu 22.04 Minimal**                                                                              |
| Shape               | `VM.Standard.A1.Flex` — 1 OCPU / 6 GB RAM (Free Tier ARM) ou `VM.Standard.E2.1.Micro` (x86 Free Tier) |
| VCN                 | Criar nova VCN ou usar existente                                                                      |
| Sub-rede            | Pública                                                                                               |
| Endereço IP público | **Atribuir automaticamente**                                                                          |

### 5.3 Configurar chave SSH

1. Na criação da instância, em **Adicionar chaves SSH**, selecione **Gerar um par de chaves para mim** e baixe a chave privada (`.key`).
2. Salve a chave em local seguro (ex: `~/.ssh/oracle-tcc.key`).

```bash
# Ajustar permissão da chave (Linux/Mac)
chmod 400 ~/.ssh/oracle-tcc.key

# Para Windows (PowerShell)
icacls "C:\Users\SEU_USUARIO\.ssh\oracle-tcc.key" /inheritance:r /grant:r "$env:USERNAME:(R)"
```

### 5.4 Abrir portas no Security List (Firewall OCI)

Na OCI, vá em **Rede → VCN → Sub-redes → Security Lists → Regras de entrada** e adicione:

| Protocolo | Porta | Origem    | Descrição   |
| --------- | ----- | --------- | ----------- |
| TCP       | 22    | 0.0.0.0/0 | SSH         |
| TCP       | 9000  | 0.0.0.0/0 | API Gateway |

> Em produção, restrinja a origem do SSH ao seu IP fixo.

### 5.5 Também liberar no firewall do Ubuntu (dentro da VM)

Após conectar via SSH (próxima etapa), execute:

```bash
sudo iptables -I INPUT -p tcp --dport 9000 -j ACCEPT
sudo iptables-save | sudo tee /etc/iptables/rules.v4
# Ou usando ufw:
sudo ufw allow 9000/tcp
sudo ufw allow 22/tcp
sudo ufw enable
```

### 5.6 Conectar na VM via SSH

```bash
ssh -i ~/.ssh/oracle-tcc.key ubuntu@IP_PUBLICO_DA_VM
```

> Substitua `IP_PUBLICO_DA_VM` pelo IP exibido no painel OCI.

---

## 6. Instalar dependências na VM

Conectado via SSH na VM Ubuntu, execute:

```bash
# Atualizar pacotes
sudo apt update && sudo apt upgrade -y

# Instalar Git, Docker e Docker Compose
sudo apt install -y git docker.io docker-compose-plugin

# Adicionar usuário atual ao grupo docker (evita precisar de sudo)
sudo usermod -aG docker $USER
newgrp docker

# Verificar instalações
docker --version
docker compose version
git --version
```

---

## 7. Configurar e subir o Backend

### 7.1 Clonar o repositório na VM

```bash
cd ~
git clone https://github.com/SEU_USUARIO/TCC-Indaiatuba-Backend.git
cd TCC-Indaiatuba-Backend
```

### 7.2 Criar o arquivo `.env`

```bash
cp .env.example .env
nano .env
```

Preencha com seus dados reais:

```env
DEFAULT_CONNECTION="Host=SEU_HOST_POSTGRES;Port=5432;Database=ZeladoriaDB;Username=postgres;Password=SUA_SENHA_FORTE"
JWT_KEY="UmaChaveJwtMuitoLongaESegura_MinimoDe32Caracteres!"
```

> `SEU_HOST_POSTGRES` pode ser:
>
> - `host.docker.internal` (banco rodando fora do Docker no mesmo host)
> - IP privado da VM se o PostgreSQL estiver instalado diretamente
> - Endereço de um banco gerenciado na nuvem

### 7.3 Transferir o `firebase-key.json` para a VM

Da sua máquina local, copie o arquivo de credenciais Firebase gerado no passo 4.3:

```bash
# Execute na sua máquina LOCAL
scp -i ~/.ssh/oracle-tcc.key firebase-key.json ubuntu@IP_PUBLICO_DA_VM:~/TCC-Indaiatuba-Backend/firebase-key.json
```

### 7.4 Banco de dados PostgreSQL

**Opção A — PostgreSQL instalado diretamente na VM (recomendado para TCC):**

```bash
sudo apt install -y postgresql
sudo systemctl enable postgresql
sudo systemctl start postgresql

# Criar banco e usuário
sudo -u postgres psql <<EOF
CREATE USER zeladoriauser WITH PASSWORD 'SUA_SENHA_FORTE';
CREATE DATABASE ZeladoriaDB OWNER zeladoriauser;
GRANT ALL PRIVILEGES ON DATABASE ZeladoriaDB TO zeladoriauser;
EOF
```

Atualize o `DEFAULT_CONNECTION` no `.env`:

```env
DEFAULT_CONNECTION="Host=host.docker.internal;Port=5432;Database=ZeladoriaDB;Username=zeladoriauser;Password=SUA_SENHA_FORTE"
```

> `host.docker.internal` permite que os containers Docker acessem o PostgreSQL instalado diretamente no host. No Linux, pode ser necessário usar o IP do `docker0` (normalmente `172.17.0.1`) ou `--network host`.

**Opção B — PostgreSQL via Docker (tudo containerizado):**

Adicione ao `docker-compose.yml` um serviço `postgres` e use `Host=postgres` na connection string.

### 7.5 Aplicar as migrações do banco

Execute **uma vez** para criar as tabelas:

```bash
# Instale o EF Core tools na VM se for rodar fora do Docker
dotnet tool install --global dotnet-ef

# Aplicar migrations
dotnet ef database update \
  --project Zeladoria.Infrastructure \
  --startup-project Zeladoria.Identity.API \
  -- --environment Production
```

> Se o banco já estiver criado via Docker, as migrações podem ser aplicadas via `dotnet run` com a string de conexão correta configurada no `.env`.

### 7.6 Subir os containers com Docker Compose

```bash
cd ~/TCC-Indaiatuba-Backend
docker compose up --build -d
```

Verificar se os containers estão rodando:

```bash
docker compose ps
docker compose logs -f
```

**Serviço disponível após subir:**

| Endpoint         | URL                                        |
| ---------------- | ------------------------------------------ |
| API Gateway      | `http://IP_DA_VM:9000`                     |
| Rota Auth        | `http://IP_DA_VM:9000/api/Auth/...`        |
| Rota Usuários    | `http://IP_DA_VM:9000/api/Usuarios/...`    |
| Rota Ocorrências | `http://IP_DA_VM:9000/api/Ocorrencias/...` |

Teste rápido:

```bash
curl http://IP_DA_VM:9000/
# Esperado: "Zeladoria API Gateway rodando na porta 9000!"
```

### 7.7 Parar / reiniciar o backend

```bash
# Parar
docker compose down

# Reiniciar após uma atualização de código
git pull
docker compose up --build -d
```

---

## 8. Configurar e rodar o Frontend Flutter

Estes passos são executados na **sua máquina local de desenvolvimento**.

### 8.1 Instalar o Flutter SDK

Siga o guia oficial: https://docs.flutter.dev/get-started/install

Verifique a instalação:

```bash
flutter doctor
```

Corrija qualquer item marcado com `[X]` antes de continuar.

### 8.2 Entrar na pasta do app

```bash
cd TCC-FRONT/tcc_app
```

### 8.3 Configurar o arquivo `.env`

```bash
cp .env.example .env
```

Edite o `.env` e aponte para o IP da sua VM OCI:

```env
API_BASE_URL=http://IP_PUBLICO_DA_VM:9000/api
```

> Para rodar 100% local (sem a VM), aponte para `http://localhost:9000/api` e suba o backend localmente com Docker Compose.

### 8.4 Configurar o Firebase para o app (`firebase_options.dart`)

Se ainda não fez no passo 4.4:

```bash
dart pub global activate flutterfire_cli
flutterfire configure
```

Ou copie e edite manualmente:

```bash
cp lib/firebase_options.dart.example lib/firebase_options.dart
```

### 8.5 Configurar o `google-services.json` (Android)

1. No Firebase Console: **Configurações do projeto → Seus aplicativos → Android**.
2. Registre o app com o `applicationId` do projeto (verifique em `android/app/build.gradle.kts`).
3. Baixe o `google-services.json` e coloque em `android/app/google-services.json`.

### 8.6 Instalar as dependências Flutter

```bash
flutter pub get
```

### 8.7 Rodar o app

```bash
# Listar dispositivos disponíveis
flutter devices

# Rodar no emulador ou dispositivo conectado
flutter run

# Gerar APK de debug
flutter build apk --debug

# Gerar APK de release
flutter build apk --release
```

---

## 9. Verificação final

Após todos os passos, valide o ambiente com este checklist:

- [ ] VM OCI criada com IP público e portas 22 e 9000 abertas
- [ ] SSH funcionando: `ssh -i chave.key ubuntu@IP_VM`
- [ ] PostgreSQL rodando e banco `ZeladoriaDB` criado
- [ ] Migrações aplicadas (tabelas existem no banco)
- [ ] `firebase-key.json` na raiz do backend na VM
- [ ] `docker compose ps` mostrando 3 containers `Up`
- [ ] `curl http://IP_VM:9000/` retornando `"Zeladoria API Gateway rodando na porta 9000!"`
- [ ] `.env` no app Flutter com `API_BASE_URL` apontando para a VM
- [ ] `firebase_options.dart` gerado no projeto Flutter
- [ ] `google-services.json` em `android/app/`
- [ ] `flutter doctor` sem erros críticos
- [ ] App rodando no emulador/dispositivo e conseguindo fazer login com Google

---

## 10. Estrutura de diretórios

```
TCC/
├── TCC-Indaiatuba-Backend/          # Backend .NET 10
│   ├── docker-compose.yml           # Orquestração dos containers
│   ├── .env.example                 # Variáveis de ambiente (template)
│   ├── firebase-key.json            # ⚠ NÃO versionar — credencial Firebase
│   ├── Zeladoria.Gateway.API/       # API Gateway YARP (porta 9000)
│   ├── Zeladoria.Identity.API/      # Auth JWT + login Google
│   ├── Zeladoria.Ocorrencias.API/   # CRUD ocorrências + push FCM
│   ├── Zeladoria.Application/       # DTOs e casos de uso
│   ├── Zeladoria.Domain/            # Entidades e contratos
│   └── Zeladoria.Infrastructure/    # EF Core, repositórios, migrações
│
├── TCC-FRONT/
│   └── tcc_app/                     # App Flutter
│       ├── .env.example             # URL da API (template)
│       ├── lib/
│       │   ├── main.dart
│       │   ├── firebase_options.dart.example  # ⚠ Gerar com FlutterFire CLI
│       │   ├── telas/               # Telas do app
│       │   └── utils/               # Helpers (auth, sessão)
│       ├── assets/
│       │   └── categorias_tags.json
│       └── android/
│           └── app/
│               └── google-services.json  # ⚠ Baixar do Firebase Console
│
└── OracleCloud/                     # Chave SSH da VM OCI (não versionar)
```

---

## Tecnologias utilizadas

**Backend**

- .NET 10 / ASP.NET Core
- Entity Framework Core + PostgreSQL (Npgsql)
- YARP — Yet Another Reverse Proxy
- Firebase Admin SDK (autenticação Google + FCM)
- Docker + Docker Compose

**Frontend**

- Flutter 3 / Dart
- Firebase Auth, Storage, Firestore, Messaging, Analytics, Crashlytics
- Google Sign-In
- `flutter_map` + `geolocator` + `geocoding`
- `google_mlkit_image_labeling`
- `flutter_dotenv` para configuração de ambiente

**Infraestrutura**

- Oracle Cloud Infrastructure (OCI) — VM Ubuntu Free Tier
- PostgreSQL

---

## Segurança — Arquivos que nunca devem ser versionados

| Arquivo                     | Motivo                                                 |
| --------------------------- | ------------------------------------------------------ |
| `firebase-key.json`         | Chave de serviço do Firebase (acesso total ao projeto) |
| `.env` (backend e frontend) | Senha do banco, chave JWT, URL da API                  |
| `firebase_options.dart`     | API keys do Firebase                                   |
| `google-services.json`      | Configuração Firebase para Android                     |
| `OracleCloud/*.key`         | Chave privada SSH da VM                                |

Todos já estão listados nos respectivos `.gitignore` dos projetos.
