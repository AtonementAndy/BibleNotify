# Instructions

Blueprint de instruções para um agente de IA criar apps.  
Preencha apenas o que for específico. Mantenha o restante genérico.

---

## Objetivo

Criar um aplicativo de bíblia que envia notificações para incentivar o usuário a ler a Bíblia.

---

## O que fazer

- Entender o objetivo do app antes de implementar
- Criar estrutura limpa, modular e fácil de manter
- Separar responsabilidades (UI, lógica, dados, serviços)
- Usar componentes reutilizáveis
- Garantir interface responsiva
- Implementar feedback visual (loading, sucesso, erro)
- Seguir boas práticas básicas de acessibilidade
- Documentar decisões importantes quando necessário
- Sempre que for solicitado uma implementação, pense em como não quebrar o código que já existe.
- Tudo que for implementado tem que ser feito pensando no código inteiro completo
- Seguir as melhores práticas de desenvolvimento de software.

---

## O que não fazer

- Não adicionar funcionalidades que não foram pedidas
- Não hardcodar valores que deveriam ser configuráveis
- Não criar código duplicado
- Não misturar lógica de negócio com a camada de apresentação
- Não ignorar estados de erro e loading
- Não usar soluções complexas quando uma simples resolve
- Não introduzir dependências desnecessárias
- Não deixar "pontas soltas" para não ser necessário refazer solicitação 2 vezes.
- Não avançar os limites das melhores práticas de desenvolvimentos de software.

---

## Stack

- Frontend: App Mobile sem Webpage.
- Backend / API: .NET MAUI.
- Banco de dados: SQLite local embarcado. A leitura da Bíblia não pode depender de API ou conexão com a internet.
- Texto bíblico inicial: Bíblia Livre (BLIVRE), edição brasileira de fevereiro de 2018, sob CC BY 4.0 BR. Preservar a atribuição no app.
- Autenticação: Você decide o que for mais usado no mundo corporativo. Faça como se fosse um Desenvolvedor Senior.
- Outros: 

### SEGUIR
- SOLID
- Clean Code

---

## Style

### Princípios
- Interface limpa e focada
- Hierarquia visual clara
- Espaçamento consistente
- Feedback imediato nas interações

### Cores
- Primária: laranja vibrante `#FF6B35`
- Secundárias: violeta `#8067FF`, âmbar `#FFC857` e menta `#44D7B6`
- Fundo escuro: `#101114`; fundo claro: `#F7F5F2`
- Texto: alto contraste, `#F7F5F2` no escuro e `#191A21` no claro
- Sucesso: `#44D7B6`
- Erro: `#FF6B6B`
- Aviso: `#FFC857`

### Tipografia
- Fonte principal: Open Sans
- Tamanhos base: 13–17 pt; títulos de tela entre 26–36 pt, com escala acessível

### Componentes
- Botões: área de toque mínima de 44 pt, cantos arredondados e contraste forte
- Inputs: estados de foco e erro visíveis
- Cards: superfícies elevadas, radius de 20–30 pt, sombras discretas
- Modais / Dialogs: curtos, acessíveis e com ações claras
- Navegação: quatro abas inferiores; a Bíblia abre diretamente a partir do início

### Layout
- Espaçamento: escala consistente baseada em 4/8 pt
- Border radius: Sim, use nos botões, nos componentes e nos cards.
- Sombras: Use se quiser.

---

## Estrutura do App

- Organize a raiz do seu projeto MAUI dividindo responsabilidades de forma clara:

```
/
MeuAppMaui/
├── Models/              # Entidades e estruturas de dados de domínio (ex: Usuario, Tarefa)
├── ViewModels/          # Lógica de apresentação, comandos (ICommand) e estados da tela [0.1.1, 0.1.4]
├── Views/               # Páginas e interfaces visuais (.xaml e .xaml.cs)
├── Services/            # Comunicação com APIs, banco de dados local e serviços de hardware [0.1.1]
├── Data/                # Contexto do banco local (SQLite / EF Core) ou repositórios
├── Helpers/ or Converters/# Conversores de XAML, extensões e utilitários globais
├── Resources/           # Imagens, fontes, ícones e splash screen centralizados [0.1.6]
└── MauiProgram.cs       # Ponto de entrada unificado e Injeção de Dependência [0.1.3]

```

---

## Regras de negócio

- O app tem: Biblia, Plano de leitura, aba de anotações
- Assim que baixar o app, tem a tela de login. Se o usuario não quiser logar ou criar conta, terá um botão de sem conta ou algo parecido.
- Assim que o user logar/criar conta/não criar conta ele vai pro dashboard inicial. Ali tem contagem de dias pra quem é premium somente. Terá abas na parte inferior do app com: Dashboard ou inicio, Plano de leitura, Aba de notificalções e configurações.
- Dashboard ou inicio: aparece o nome do usuario e sua função no corpo de Cristo que vai pegar isso na criação da conta. Botão que leva direto pra Biblia. Bloco onde mostra os dias consecutivos de leitura. E voce pode decidir o que mais dentro do contexto do app.
- Aba de Plano de Leitura: Só isso mesmo, o que ja foi lido dentro do plano escolhido e clicando ele vai direto pra Biblia pra ler.
- Biblia: sem mais informações só Biblia
- Aba de anotação da biblia e vai salvar isso, pode ser no proprio app de inicio...Mais pra frente salvarei no banco de dados.
- Configurações: Parte onde está o cadastro do usuario, que tem nome, sobrenome, funçao do corpo de Cristo, telefone e email algo assim. Aprte de Premium: Plano mensal para BR: 9.99, semestral: 45.99 e anual: 79.99 BRL; Usuario de lingua Inglesa: $9.99/month, $39.99/Six Month e $69.99/yearly Parte das notificações: Aqui somente o Premium pode tudo, criar, deletar e modificar as notificações, modificando o horario tudo mesmo. Usuario com conta: POde mudar somente o horario das unicas 2 notificações. Usuario sem conta nao pode nada.
Parte de tema: Tema claro e escuro.
E se quiser add mais alguma coisa importante pra um app profissional me avise antes. 
- Vão existir 3 tipos de usuários: Sem conta, com conta e o Premium.
- Sem conta: pode ler a Bíblia offline; não pode acessar planos/anotações nem alterar notificações ou preferências de conta.
- Com conta mas não premium: Tambem não tem direito a nada. Mas pode mudar o horario das 2 notificações default.
- Usuário premium: Pode tudo. Pode escolher plano de leitura da Bíblia, aba de anotações.
- O app tem que ser criado em português e inglês. E identificar segundo as configs do usuario qual versão mostrar
- O app tem que ser multiplatforma.

Menu de Ajustes:
- Conta
- Premium
- Notificações
- Tema

O cara clica e muda o que der...
- Conta: Mostra os informações do usuario = nome, sobrenome, função no corpo de Cristo, telefone e email...essas coisa..
- Premium: É assim que tem que ser...em Premium mostra os plano de pagamento...
E leva ele pra um meio de pagar.
Eu ainda vou implementar isso ou usar algum meio ja existente como Stripe.
- Notificações:  ao clicar entra na parte de notificações onde o premium pode criar, deletar, mudar tudo. Usuario com conta somente o horario das 2 notificações default e user sem conta nada.
- Tema claro e escuro e inicia o app de acordo com o que esta estabelecido no cel do usuario
O que estiver bloqueado dentro do app que é premium...deixa um botão que leva o user pra aba de premium dentro do ajustes pra ele comprar.

Notificações:
- o app tem que enviar as notificações no celular do usuario o chamando pra ler a biblia de forma que esta configurada no app. Se a norificação default 1 esta configurada para 8:00 da manhã, ele recebe uma notificação: "É hora de ler a palavra do seu Deus" algo assim, e pode ser algo aleatorio desse jeito, podemos criar varias frases assim.

---

## Critérios de qualidade

- Código legível e organizado
- Componentes com responsabilidade única
- Tratamento adequado de erros
- Experiência do usuário fluida
- Fácil de evoluir e manter
