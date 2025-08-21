[<img src="./assets/us.svg" width="35" title="Ver em Inglês">](./README.md)
[<img src="./assets/es.svg" width="35" title="Ver em Espanhol">](./README.es.md)

# osu! Sweep

> Um utilitário de desktop seguro, inteligente e fácil de usar para gerenciar e limpar grandes coleções de beatmaps de osu! com base nos modos de jogo.

O osu! Sweep é um utilitário de desktop para Windows, construído em C# e WPF, projetado para jogadores do game de ritmo osu!. 
O objetivo é fornecer uma ferramenta para liberar espaço em disco e organizar a biblioteca do jogador, identificando e apagando beatmaps de modos de jogo que ele não joga.

## ✨ Funcionalidades Chave

-   **Identificação de Mapas Híbrida**: Usa a API do osu! como método principal e um escaneamento profundo de arquivos como fallback.
-   **Lógica de Deleção Inteligente**: Apaga pastas inteiras ou apenas arquivos `.osu` específicos, preservando os modos que você quer manter.
-   **Segurança do Usuário**: Possui uma tela de confirmação clara e, por padrão, move os arquivos para a Lixeira.

## 🛠️ Tecnologias Utilizadas

-   **Frontend (Aplicativo Desktop)**: C# com WPF (.NET 8) utilizando o padrão MVVM.
-   **Backend (API Serverless)**: C# em Azure Functions para proteger a chave da API e cachear as requisições.

## 🚀 Como Executar

*(Esta seção será preenchida mais tarde, com as instruções de como compilar e executar o projeto a partir do código-fonte).*

## 🤝 Contribuições

*(Podemos adicionar diretrizes aqui mais tarde, caso você deseje aceitar contribuições da comunidade).*