# osu! Sweep 🧹🎶

<p align="center">
  <a href="README.md"><img src="assets/flag_br.png" alt="Português" width="30"/></a>
  <a href="docs/README_en.md"><img src="assets/flag_us.png" alt="English" width="30"/></a>
  <a href="docs/README_es.md"><img src="assets/flag_es.png" alt="Español" width="30"/></a>
</p>

Uma **ferramenta de desktop segura, inteligente e amigável** para gerenciar e limpar grandes coleções de beatmaps do **osu!**, com base nos modos de jogo que você realmente usa.

<p align="center">
  <img src="https://i.imgur.com/link_para_seu_gif_aqui.gif" alt="osu! Sweep Demo" width="700"/>
</p>

---

![License](https://img.shields.io/badge/license-MIT-green.svg)
![Languages](https://img.shields.io/badge/idiomas-3-blue.svg)
![Tech](https://img.shields.io/badge/.NET-8.0-purple.svg)

## 🚦 Status do Projeto

**Em Desenvolvimento Ativo (Versão Alpha)**

Este projeto é um trabalho em progresso. A arquitetura e a lógica principal (análise de mapas, comunicação com a API, cálculo de exclusão e testes unitários) estão bem estruturadas e funcionais. A interface do usuário (UI) está sendo migrada de WPF para Avalonia UI para suporte multiplataforma.

## 📌 Sobre o Projeto

O **osu! Sweep** foi criado para lidar com bibliotecas enormes do osu!, que podem ocupar **centenas de GBs** no disco. O objetivo é oferecer uma ferramenta que ajude jogadores a **recuperar espaço e organizar seus mapas** de forma **segura e personalizável**.

---

## ✨ Funcionalidades

- **🔍 Identificação Híbrida de Beatmaps**
  - Usa a **API v2 do osu!** como método principal, com autenticação OAuth 2.0.
  - Faz **escaneamento profundo de arquivos** `.osu` como alternativa para mapas locais.

- **🧠 Lógica de Exclusão Inteligente**
  - Remove pastas inteiras ou apenas arquivos `.osu` de modos indesejados.
  - Preserva os modos de jogo que você escolheu manter.

- **🛡️ Segurança do Usuário em Primeiro Lugar**
  - Exibe uma tela de confirmação clara antes de qualquer exclusão.
  - Por padrão, move arquivos para a **Lixeira**, com opção de exclusão permanente.

- **🌐 Internacionalização (i18n)**
  - Suporte a múltiplos idiomas (Português, Inglês, Espanhol) com troca em tempo real.

---

## 🛠️ Tecnologias Utilizadas

- **Frontend (App Desktop):**
  - C# com **Avalonia UI** (.NET 8) para suporte nativo a Windows e Linux.
  - Padrão **MVVM (Model-View-ViewModel)**.

- **Backend (Serverless API):**
  - Azure Functions em C# (.NET 8 Isolated Worker).
  - Proxy seguro para a API v2 do osu! com gestão de token OAuth 2.0.

- **Testes:**
  - MSTest (para testes unitários e de integração).
  - Moq (para simulação de dependências).

---

## 📂 Estrutura do Projeto

```bash
OsuSweep.sln
├── OsuSweep/                 # Aplicação desktop (Frontend - UI)
├── OsuSweep.Backend/         # Azure Functions (Backend - API Proxy)
├── OsuSweep.Core/            # Lógica de negócio, contratos e modelos compartilhados
├── OsuSweep.Tests.Unit/      # Testes unitários rápidos
└── OsuSweep.Tests.Integration/ # Testes de integração (I/O)
```

---

## 🚀 Como Executar

Instruções para compilar e executar o projeto a partir do código-fonte.

### 📋 Pré-requisitos

- .NET 8 SDK
- Visual Studio 2022 (com os workloads ".NET desktop development" e "Azure development")
- Conta no osu! para gerar credenciais de API (OAuth)

### ▶️ Passos

1. Clone o repositório:

   ```bash
   git clone https://github.com/seu-usuario/OsuSweep.git
   cd OsuSweep
   ```

2. Configure as credenciais da API:

   - No projeto `OsuSweep.Backend`, crie um arquivo chamado `local.settings.json`.
   - Preencha com suas credenciais `Client ID` e `Client Secret`:

   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "OSU_API_CLIENT_ID": "SEU_CLIENT_ID_AQUI",
       "OSU_API_CLIENT_SECRET": "SEU_CLIENT_SECRET_AQUI",
       "Host": {
         "CORS": "*"
       }
     }
   }
   ```

3. Execute a aplicação:

   - Abra a solução `OsuSweep.sln` no Visual Studio.
   - Clique com o botão direito na Solução > *Configure Startup Projects...* e configure para iniciar ambos os projetos `OsuSweep` e `OsuSweep.Backend`.
   - Pressione F5 para iniciar.

---

## 🗺️ Roadmap

- [x] Estrutura inicial do projeto e lógica de negócio.
- [x] Implementação da API v2 do osu! e análise manual.
- [x] Arquitetura de testes (unitários e de integração).
- [x] Suporte a múltiplos idiomas (i18n).
- [ ] Migração da UI para Avalonia: Reescrever a camada de apresentação (Views) de WPF para Avalonia UI para criar uma base multiplataforma.
- [ ] Implementar suporte a Linux: Criar lógica específica para o sistema de arquivos Linux (ex.: encontrar a pasta `Songs`, mover para a Lixeira).
- [ ] Melhorias de UI/UX com tema inspirado no osu! (pós-migração).
- [ ] Configurações avançadas (ex.: excluir vídeos/storyboards).
- [ ] Publicação de uma release instalável no GitHub.

---

## 📜 Licença

Distribuído sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais informações.