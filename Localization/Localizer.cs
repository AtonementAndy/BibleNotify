using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace BibleNotify.Localization;

[ContentProperty(nameof(Key))]
public sealed class TranslateExtension : IMarkupExtension<string>
{
    public string Key { get; set; } = string.Empty;

    public string ProvideValue(IServiceProvider serviceProvider) => Localizer.Get(Key);

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
        ProvideValue(serviceProvider);
}

public static class Localizer
{
    private static readonly IReadOnlyDictionary<string, string> EnglishBibleBookNames =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["GEN"] = "Genesis",
            ["EXO"] = "Exodus",
            ["LEV"] = "Leviticus",
            ["NUM"] = "Numbers",
            ["DEU"] = "Deuteronomy",
            ["JOS"] = "Joshua",
            ["JDG"] = "Judges",
            ["RUT"] = "Ruth",
            ["1SA"] = "1 Samuel",
            ["2SA"] = "2 Samuel",
            ["1KI"] = "1 Kings",
            ["2KI"] = "2 Kings",
            ["1CH"] = "1 Chronicles",
            ["2CH"] = "2 Chronicles",
            ["EZR"] = "Ezra",
            ["NEH"] = "Nehemiah",
            ["EST"] = "Esther",
            ["JOB"] = "Job",
            ["PSA"] = "Psalms",
            ["PRO"] = "Proverbs",
            ["ECC"] = "Ecclesiastes",
            ["SNG"] = "Song of Solomon",
            ["ISA"] = "Isaiah",
            ["JER"] = "Jeremiah",
            ["LAM"] = "Lamentations",
            ["EZK"] = "Ezekiel",
            ["DAN"] = "Daniel",
            ["HOS"] = "Hosea",
            ["JOL"] = "Joel",
            ["AMO"] = "Amos",
            ["OBA"] = "Obadiah",
            ["JON"] = "Jonah",
            ["MIC"] = "Micah",
            ["NAH"] = "Nahum",
            ["NAM"] = "Nahum",
            ["HAB"] = "Habakkuk",
            ["ZEP"] = "Zephaniah",
            ["HAG"] = "Haggai",
            ["ZEC"] = "Zechariah",
            ["MAL"] = "Malachi",
            ["MAT"] = "Matthew",
            ["MRK"] = "Mark",
            ["LUK"] = "Luke",
            ["JOH"] = "John",
            ["JHN"] = "John",
            ["ACT"] = "Acts",
            ["ROM"] = "Romans",
            ["1CO"] = "1 Corinthians",
            ["2CO"] = "2 Corinthians",
            ["GAL"] = "Galatians",
            ["EPH"] = "Ephesians",
            ["PHP"] = "Philippians",
            ["COL"] = "Colossians",
            ["1TH"] = "1 Thessalonians",
            ["2TH"] = "2 Thessalonians",
            ["1TI"] = "1 Timothy",
            ["2TI"] = "2 Timothy",
            ["TIT"] = "Titus",
            ["PHM"] = "Philemon",
            ["HEB"] = "Hebrews",
            ["JAS"] = "James",
            ["1PE"] = "1 Peter",
            ["2PE"] = "2 Peter",
            ["1JN"] = "1 John",
            ["2JN"] = "2 John",
            ["3JN"] = "3 John",
            ["JUD"] = "Jude",
            ["REV"] = "Revelation"
        };

    public static void ConfigureFromDeviceLanguage()
    {
        var deviceLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (string.IsNullOrWhiteSpace(deviceLanguage) || deviceLanguage == "iv")
        {
            deviceLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        var culture = CultureInfo.GetCultureInfo(
            deviceLanguage.Equals("pt", StringComparison.OrdinalIgnoreCase) ? "pt-BR" : "en-US");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    private static readonly IReadOnlyDictionary<string, string> English =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Símbolo do Bible Notify"] = "BibleNotify logo",
            ["Um momento com a Palavra pode mudar o seu dia."] = "A moment with the Word can change your day.",
            ["Entre ou crie sua conta para continuar."] = "Sign in or create an account to continue.",
            ["Nome"] = "First name",
            ["Sobrenome"] = "Last name",
            ["Sua função no Corpo de Cristo"] = "Your role in the Church",
            ["Telefone (opcional)"] = "Phone (optional)",
            ["E-mail"] = "Email",
            ["Senha (mín. 8 caracteres)"] = "Password (at least 8 characters)",
            ["Confirme a senha"] = "Confirm password",
            ["Senha"] = "Password",
            ["Continuar sem conta"] = "Continue as guest",
            ["Abre o aplicativo em modo sem conta e prepara a Bíblia offline"] = "Opens the app as a guest and prepares the offline Bible",
            ["Criar minha conta"] = "Create my account",
            ["Entrar"] = "Sign in",
            ["Já tem uma conta? Entrar"] = "Already have an account? Sign in",
            ["Ainda não tem conta? Criar conta"] = "Don't have an account? Create one",
            ["As senhas não conferem."] = "The passwords do not match.",
            ["E-mail ou senha incorretos."] = "Incorrect email or password.",
            ["Já existe uma conta com este e-mail."] = "An account with this email already exists.",
            ["Não foi possível abrir os dados locais. Reinstale o aplicativo."] = "Could not open local data. Reinstall the app.",
            ["Não foi possível preparar o aplicativo. Tente novamente."] = "Could not prepare the app. Try again.",
            ["Não foi possível acessar a conta local. Tente novamente."] = "Could not access the local account. Try again.",
            ["O banco da Bíblia está inválido. Reinstale o aplicativo."] = "The Bible database is invalid. Reinstall the app.",
            ["Não foi possível preparar a Bíblia offline. Tente novamente."] = "Could not prepare the offline Bible. Try again.",
            ["Não foi possível abrir a Bíblia offline. Tente novamente."] = "Could not open the offline Bible. Try again.",
            ["Leve sua jornada com você"] = "Take your journey with you",
            ["Crie sua conta para guardar seus dados e continuar depois."] = "Create an account to save your data and pick up where you left off.",
            ["Criar conta"] = "Create account",
            ["Abre o cadastro de uma nova conta"] = "Opens the new account form",
            ["{0} dias consecutivos"] = "{0} consecutive days",
            ["Sua constância na Palavra"] = "Your consistency in the Word",
            ["Sequência de leitura"] = "Reading streak",
            ["Premium"] = "Premium",
            ["SUA JORNADA DE LEITURA"] = "YOUR READING JOURNEY",
            ["UM COMEÇO PARA HOJE"] = "A START FOR TODAY",
            ["Ler capítulo"] = "Read chapter",
            ["Abre a leitura da Bíblia armazenada no aparelho"] = "Opens the Bible stored on this device",
            ["Suas anotações"] = "Your notes",
            ["Guarde reflexões e versículos importantes."] = "Save reflections and meaningful verses.",
            ["Abrir"] = "Open",
            ["Abre suas anotações bíblicas"] = "Opens your Bible notes",
            ["Abre as opções Premium"] = "Opens Premium options",
            ["Início"] = "Home",
            ["Notas"] = "Notes",
            ["Bíblia"] = "Bible",
            ["SELECIONE LIVRO E CAPÍTULO"] = "SELECT A BOOK AND CHAPTER",
            ["Livro"] = "Book",
            ["Capítulo"] = "Chapter",
            ["Selecione um livro da Bíblia"] = "Select a book of the Bible",
            ["Selecione o capítulo"] = "Select the chapter",
            ["Bíblia Livre · leitura offline"] = "World English Bible (WEB) · bible-api.com · Public Domain",
            ["Abrindo capítulo..."] = "Opening chapter...",
            ["Anterior"] = "Previous",
            ["Próximo"] = "Next",
            ["Constância, um dia de cada vez."] = "Consistency, one day at a time.",
            ["Acompanhe uma jornada guiada pela Bíblia."] = "Follow a guided journey through the Bible.",
            ["ESCOLHA UM PLANO"] = "CHOOSE A PLAN",
            ["PLANOS DE LEITURA"] = "READING PLANS",
            ["Escolha uma jornada para conhecer os livros, a duração e o foco de cada plano."] = "Choose a journey to explore the books, duration, and focus of each plan.",
            ["Toque para selecionar este plano"] = "Tap to select this plan",
            ["Toque para ver os recursos Premium"] = "Tap to view Premium options",
            ["Selecionado"] = "Selected",
            ["Nenhum plano de leitura disponível no momento."] = "No reading plans are available right now.",
            ["SUA JORNADA"] = "YOUR JOURNEY",
            ["ESCOLHA OUTRO PLANO"] = "CHOOSE ANOTHER PLAN",
            ["Seu plano ainda não começou."] = "Your plan has not started yet.",
            ["Toque em Iniciar plano para ver as leituras diárias."] = "Tap Start plan to see your daily readings.",
            ["Inicie o plano para ver as leituras diárias."] = "Start the plan to see your daily readings.",
            ["{0} dias"] = "{0} days",
            ["{0} · Bíblia Livre"] = "{0} · World English Bible (WEB)",
            ["{0} {1}:{2} · Bíblia Livre"] = "{0} {1}:{2} · World English Bible (WEB)",
            ["Dia {0}"] = "Day {0}",
            ["🔒 PLANOS E PROGRESSO PREMIUM"] = "🔒 PREMIUM PLANS AND PROGRESS",
            ["Conheça os formatos abaixo. Para iniciar um plano, acompanhar leituras e salvar seu progresso, é necessário o Premium."] = "Explore the plans below. Premium is required to start a plan, track readings, and save your progress.",
            ["LEITURAS DO PLANO"] = "PLAN READINGS",
            ["D{0}"] = "Day {0}",
            ["Concluído"] = "Completed",
            ["Ler"] = "Read",
            ["Marcar lido"] = "Mark as read",
            ["Anotações"] = "Notes",
            ["Guarde o que tocou você."] = "Keep what moved you.",
            ["Organize e releia suas reflexões bíblicas."] = "Organize and revisit your Bible reflections.",
            ["🔒 ANOTAÇÕES EXCLUSIVAS DO PREMIUM"] = "🔒 PREMIUM NOTES",
            ["NOVA ANOTAÇÃO"] = "NEW NOTE",
            ["Cap."] = "Ch.",
            ["Vers."] = "Verse",
            ["Escreva sua reflexão..."] = "Write your reflection...",
            ["Desbloqueie o Premium para escrever e salvar suas reflexões."] = "Unlock Premium to write and save your reflections.",
            ["{0:dd/MM/yyyy}"] = "{0:M/d/yyyy}",
            ["Excluir"] = "Delete",
            ["Planos"] = "Plans",
            ["Avisos"] = "Reminders",
            ["Lembretes que acolhem."] = "Gentle reminders.",
            ["Uma pausa para estar com a Palavra."] = "A moment to be with the Word.",
            ["NOTIFICAÇÕES"] = "NOTIFICATIONS",
            ["Os lembretes serão configurados na próxima etapa."] = "Reminders will be configured in a future update.",
            ["O agendamento local e as permissões do aparelho serão implementados antes de ativar os avisos."] = "Local scheduling and device permissions will be implemented before reminders are enabled.",
            ["Personalize sua experiência"] = "Make it your own",
            ["Voltar para ajustes"] = "Back to settings",
            ["Conta"] = "Account",
            ["Seus dados e perfil"] = "Your details and profile",
            ["Planos e benefícios"] = "Plans and benefits",
            ["Lembretes de leitura"] = "Reading reminders",
            ["Tema inválido."] = "Invalid theme.",
            ["Entre ou crie uma conta para acessar este recurso."] = "Sign in or create an account to access this feature.",
            ["A senha deve ter pelo menos 8 caracteres."] = "The password must be at least 8 characters long.",
            ["A senha deve ter entre 8 e 1024 caracteres."] = "The password must be between 8 and 1024 characters.",
            ["Confira o tamanho dos dados de perfil."] = "Check the lengths of the profile details.",
            ["A anotação deve ter entre 1 e 10000 caracteres."] = "The note must be between 1 and 10,000 characters.",
            ["O título deve ter até 100 caracteres e a mensagem até 1000."] = "The title must be at most 100 characters and the message at most 1,000.",
            ["É hora de um momento com a Palavra de Deus."] = "It's time for a moment with the Word of God.",
            ["Separe alguns minutos para ler a Bíblia."] = "Set aside a few minutes to read the Bible.",
            ["A Palavra de Deus espera por você. Vamos ler?"] = "The Word of God is waiting for you. Shall we read?",
            ["Faça uma pausa e fortaleça sua fé com a Palavra."] = "Take a break and strengthen your faith with the Word.",
            ["Seu momento de leitura bíblica chegou."] = "It's time for your Bible reading.",
            ["Que tal renovar o coração com a Palavra de Deus?"] = "How about renewing your heart with the Word of God?",
            ["Abra a Bíblia e reserve este momento para Deus."] = "Open the Bible and set aside this moment for God.",
            ["Um momento para a Palavra."] = "A moment for the Word.",
            ["Aparência"] = "Appearance",
            ["Premium (prévia)"] = "Premium (preview)",
            ["SEUS DADOS"] = "YOUR DETAILS",
            ["Veja e atualize as informações da sua conta."] = "View and update your account information.",
            ["Entre ou crie uma conta para cadastrar e editar seu perfil."] = "Sign in or create an account to set up and edit your profile.",
            ["Função no Corpo de Cristo"] = "Role in the Church",
            ["Telefone"] = "Phone",
            ["Salvar perfil"] = "Save profile",
            ["Entrar ou criar conta"] = "Sign in or create an account",
            ["Sair da conta"] = "Sign out",
            ["ESCOLHA SEU PLANO"] = "CHOOSE YOUR PLAN",
            ["Desbloqueie planos de leitura, anotações e personalização completa de notificações."] = "Unlock reading plans, notes, and fully customizable reminders.",
            ["Mensal"] = "Monthly",
            ["R$ 9,99 / mês"] = "$9.99 / month",
            ["Escolher"] = "Select",
            ["Semestral"] = "6 months",
            ["R$ 45,99 / 6 meses"] = "$39.99 / 6 months",
            ["Anual"] = "Yearly",
            ["R$ 79,99 / ano"] = "$69.99 / year",
            ["A contratação ainda não está ativa. Nenhuma cobrança será feita até a integração de pagamento."] = "Purchases are not available yet. You will not be charged until payment integration is complete.",
            ["Prévia de demonstração — não é uma compra nem uma assinatura."] = "Demo preview — this is not a purchase or subscription.",
            ["Ativar prévia Premium neste aparelho"] = "Enable Premium preview on this device",
            ["Desativar prévia Premium"] = "Disable Premium preview",
            ["Visitantes não podem configurar notificações. Contas gratuitas podem alterar somente os horários dos dois lembretes padrão."] = "Guests cannot configure reminders. Free accounts can only change the times of the two default reminders.",
            ["Entre ou crie uma conta para configurar os horários dos lembretes padrão."] = "Sign in or create an account to set the default reminder times.",
            ["Selecione um lembrete"] = "Select a reminder",
            ["SEUS LEMBRETES"] = "YOUR REMINDERS",
            ["Editar"] = "Edit",
            ["Ativar notificações neste aparelho"] = "Enable reminders on this device",
            ["Personalizar, criar ou excluir lembretes é exclusivo do Premium."] = "Customizing, creating, and deleting reminders is a Premium feature.",
            ["EDITANDO: {0}"] = "EDITING: {0}",
            ["Título"] = "Title",
            ["Mensagem"] = "Message",
            ["Salvar alterações do lembrete"] = "Save reminder changes",
            ["Novo título"] = "New title",
            ["Nova mensagem"] = "New message",
            ["Criar lembrete"] = "Create reminder",
            ["Conhecer planos Premium"] = "Explore Premium plans",
            ["APARÊNCIA"] = "APPEARANCE",
            ["Use o tema do aparelho ou escolha claro ou escuro. A opção Sistema acompanha a configuração atual do celular."] = "Use your device theme or choose light or dark. System follows your device's current setting.",
            ["Aplicar tema"] = "Apply theme",
            ["Sistema"] = "System",
            ["Claro"] = "Light",
            ["Escuro"] = "Dark",
            ["Membro"] = "Member",
            ["Pastor"] = "Pastor",
            ["Pastora"] = "Female pastor",
            ["Líder"] = "Leader",
            ["Missionário"] = "Missionary",
            ["Missionária"] = "Female missionary",
            ["Diácono"] = "Deacon",
            ["Diaconisa"] = "Deaconess",
            ["Presbítero"] = "Elder",
            ["Presbítera"] = "Female elder",
            ["Evangelista"] = "Evangelist",
            ["Outro"] = "Other",
            ["Não foi possível escolher um versículo para hoje."] = "Could not choose today's verse.",
            ["{0} de {1} dias concluídos"] = "{0} of {1} days completed",
            ["Nenhum plano iniciado"] = "No plan started",
            ["Parabéns! Você concluiu este plano."] = "Congratulations! You completed this plan.",
            ["Próxima leitura · Dia {0}: {1}"] = "Next reading · Day {0}: {1}",
            ["Escolha um plano para organizar sua leitura diária."] = "Choose a plan to organize your daily reading.",
            ["Comece uma jornada de leitura no seu ritmo."] = "Start a reading journey at your own pace.",
            ["Não foi possível carregar o versículo. Tente novamente."] = "Could not load the verse. Try again.",
            ["Não foi possível carregar os dados. Tente novamente."] = "Could not load the data. Try again.",
            ["Que bom ter você por aqui!"] = "It's good to have you here!",
            ["Que bom ver você novamente."] = "It's good to see you again.",
            ["Modo visitante"] = "Guest mode",
            ["Visitante"] = "Guest",
            ["Acompanhe seus dias consecutivos com uma conta Premium."] = "Track your consecutive days with a Premium account.",
            ["Acompanhe seus dias consecutivos com o Premium."] = "Track your consecutive days with Premium.",
            ["Ver plano"] = "View plan",
            ["Escolher plano"] = "Choose a plan",
            ["Não foi possível carregar os planos. Tente novamente."] = "Could not load plans. Try again.",
            ["Este plano ainda não está disponível."] = "This plan is not available yet.",
            ["Não foi possível iniciar o plano. Tente novamente."] = "Could not start the plan. Try again.",
            ["Marcar o progresso do plano é um benefício Premium."] = "Tracking plan progress is a Premium benefit.",
            ["Não foi possível atualizar o progresso. Tente novamente."] = "Could not update progress. Try again.",
            ["🔒 Disponível no Premium"] = "🔒 Available with Premium",
            ["Continuar plano"] = "Continue plan",
            ["Iniciar plano"] = "Start plan",
            ["Usar este plano"] = "Use this plan",
            ["Plano {0} selecionado. Toque em {1} para prosseguir."] = "Plan {0} selected. Tap {1} to continue.",
            ["Plano {0} iniciado! Suas leituras estão prontas abaixo."] = "Plan {0} started! Your readings are ready below.",
            ["Criar conta e conhecer o Premium"] = "Create an account and explore Premium",
            ["Ver prévia Premium"] = "View Premium preview",
            ["Este recurso é exclusivo do Premium. Veja os planos e crie uma conta para continuar."] = "This feature is exclusive to Premium. Explore the plans and create an account to continue.",
            ["Este recurso é exclusivo do Premium. Conheça os planos para criar e organizar suas anotações."] = "This feature is exclusive to Premium. Explore the plans to create and organize your notes.",
            ["Suas anotações aparecerão aqui quando o Premium estiver ativo."] = "Your notes will appear here when Premium is active.",
            ["Suas anotações aparecerão aqui."] = "Your notes will appear here.",
            ["🔒 Salvar anotação · Premium"] = "🔒 Save note · Premium",
            ["Salvar anotação"] = "Save note",
            ["Salvar alterações"] = "Save changes",
            ["EDITANDO ANOTAÇÃO"] = "EDITING NOTE",
            ["Cancelar edição"] = "Cancel editing",
            ["Você não tem permissão para editar esta anotação."] = "You do not have permission to edit this note.",
            ["Não foi possível carregar as anotações. Tente novamente."] = "Could not load notes. Try again.",
            ["Não foi possível carregar a Bíblia offline."] = "Could not load the offline Bible.",
            ["Escolha uma referência e escreva sua anotação."] = "Choose a reference and write your note.",
            ["Não foi possível salvar a anotação."] = "Could not save the note.",
            ["Anotação salva, mas não foi possível atualizar a lista."] = "Note saved, but the list could not be refreshed.",
            ["Não foi possível excluir a anotação."] = "Could not delete the note.",
            ["A referência solicitada não está disponível."] = "The requested reference is not available.",
            ["Este capítulo não está disponível."] = "This chapter is not available.",
            ["Capítulo carregado, mas não foi possível salvar seu progresso."] = "Chapter loaded, but your progress could not be saved.",
            ["Não foi possível carregar a Bíblia offline. Tente novamente."] = "Could not load the offline Bible. Try again.",
            ["Não foi possível abrir este capítulo. Tente novamente."] = "Could not open this chapter. Try again.",
            ["Ajustes"] = "Settings",
            ["Notificações"] = "Notifications",
            ["Tema"] = "Theme",
            ["Premium · prévia local"] = "Premium · local preview",
            ["Conta gratuita"] = "Free account",
            ["Visitante · Bíblia offline"] = "Guest · offline Bible",
            ["🔒 Alterar horários · precisa de conta"] = "🔒 Change times · account required",
            ["Salvar horário"] = "Save time",
            ["Recurso Premium. Entre ou crie uma conta para conhecer a prévia."] = "Premium feature. Sign in or create an account to try the preview.",
            ["Recurso exclusivo do Premium. Ative a prévia para experimentar."] = "Premium feature. Enable the preview to try it.",
            ["Não foi possível carregar as configurações."] = "Could not load settings.",
            ["Perfil atualizado."] = "Profile updated.",
            ["Não foi possível salvar o perfil."] = "Could not save the profile.",
            ["Prévia Premium ativada neste aparelho. Nenhuma compra ou cobrança foi realizada."] = "Premium preview enabled on this device. No purchase was made and no charge was made.",
            ["Prévia Premium desativada."] = "Premium preview disabled.",
            ["A prévia Premium só fica disponível em compilações de desenvolvimento."] = "The Premium preview is only available in development builds.",
            ["Dia inválido para o plano ativo."] = "Invalid day for the active plan.",
            ["Não foi possível carregar a Bíblia online. Verifique sua conexão e tente novamente."] = "Could not load the online Bible. Check your connection and try again.",
            ["O serviço da Bíblia retornou dados inválidos. Tente novamente mais tarde."] = "The Bible service returned invalid data. Please try again later.",
            ["Não foi possível atualizar o nível da conta."] = "Could not update the account level.",
            ["O nível da conta foi atualizado, mas não foi possível atualizar os lembretes."] = "The account level was updated, but reminders could not be refreshed.",
            ["Tema atualizado."] = "Theme updated.",
            ["Não foi possível salvar o tema."] = "Could not save the theme.",
            ["Selecione um lembrete."] = "Select a reminder.",
            ["Lembrete atualizado."] = "Reminder updated.",
            ["Não foi possível salvar o lembrete."] = "Could not save the reminder.",
            ["Horário salvo, mas não foi possível atualizar o agendamento no aparelho."] = "Time saved, but the schedule on this device could not be updated.",
            ["Novo lembrete criado."] = "New reminder created.",
            ["Não foi possível criar o lembrete."] = "Could not create the reminder.",
            ["Lembrete criado, mas não foi possível atualizar o agendamento no aparelho."] = "Reminder created, but the schedule on this device could not be updated.",
            ["Editando: {0}"] = "Editing: {0}",
            ["Lembrete excluído."] = "Reminder deleted.",
            ["Não foi possível excluir o lembrete."] = "Could not delete the reminder.",
            ["Lembrete excluído, mas não foi possível atualizar o agendamento no aparelho."] = "Reminder deleted, but the schedule on this device could not be updated.",
            ["Não foi possível cancelar os lembretes. Tente novamente."] = "Could not cancel reminders. Try again.",
            ["Entre ou crie uma conta para continuar. A integração de pagamento ainda será configurada."] = "Sign in or create an account to continue. Payment integration is not available yet.",
            ["A integração de pagamento ainda não está configurada. Nenhuma cobrança foi realizada."] = "Payment integration is not available yet. No charge was made.",
            ["Conceda as permissões de notificações e alarmes exatos e toque novamente em ativar."] = "Grant notification and exact alarm permissions, then tap enable again.",
            ["Lembretes diários ativados neste aparelho."] = "Daily reminders enabled on this device.",
            ["Plano {0}"] = "Plan {0}",
            ["Entendi"] = "Got it",
            ["Não foi possível carregar os lembretes."] = "Could not load reminders.",
            ["Não foi possível ativar os lembretes neste aparelho."] = "Could not enable reminders on this device.",
            ["Nome, sobrenome e função ministerial são obrigatórios."] = "First name, last name, and ministry role are required.",
            ["Plano de leitura não encontrado."] = "Reading plan not found.",
            ["A anotação não pode ficar vazia."] = "The note cannot be empty.",
            ["Somente os dois lembretes padrão podem ser alterados nesta conta."] = "Only the two default reminders can be changed on this account.",
            ["A conta local não foi encontrada."] = "The local account was not found.",
            ["Este recurso está disponível no Premium."] = "This feature is available with Premium.",
            ["Preencha nome, sobrenome e função no corpo de Cristo."] = "Enter your first name, last name, and role in the Church.",
            ["Digite um e-mail válido."] = "Enter a valid email address.",
            ["Informe um título e uma mensagem para o lembrete."] = "Enter a title and message for the reminder.",
            ["Momento da manhã"] = "Morning moment",
            ["Comece o dia com alguns minutos na Palavra."] = "Start the day with a few minutes in the Word.",
            ["Pausa para refletir"] = "Time to reflect",
            ["Reserve um instante para ler a Bíblia."] = "Set aside a moment to read the Bible.",
            ["Uma semana nos Salmos"] = "A Week in Psalms",
            ["Sete salmos para encontrar inspiração no dia a dia."] = "Seven psalms to find inspiration in everyday life.",
            ["Evangelho de João"] = "The Gospel of John",
            ["Uma jornada de 21 dias pelo evangelho de João."] = "A 21-day journey through the Gospel of John.",
            ["Plano canônico (sequencial)"] = "Canonical Plan (Sequential)",
            ["Gênesis ao Apocalipse em ordem tradicional. Simples de acompanhar; a primeira parte do ano concentra-se no Antigo Testamento."] = "Read from Genesis to Revelation in traditional order. Easy to follow; the first part of the year focuses on the Old Testament.",
            ["Plano cronológico (aproximado)"] = "Chronological Plan (Approximate)",
            ["Livros organizados por períodos históricos para conectar acontecimentos. A sequência é aproximada, não uma cronologia versículo a versículo."] = "Books are organized by historical periods to connect events. The sequence is approximate, not a verse-by-verse chronology.",
            ["Plano misto (AT + NT)"] = "Mixed Plan (OT + NT)",
            ["Leituras do Antigo e do Novo Testamento todos os dias. Estilo equilibrado, recomendado para manter variedade; alguns trechos do NT se repetem."] = "Readings from the Old and New Testaments every day. A balanced approach for variety; some New Testament passages repeat.",
            ["Plano M’Cheyne"] = "M'Cheyne Plan",
            ["Plano clássico: Antigo Testamento uma vez, Novo Testamento duas vezes e Salmos em uma segunda leitura. Cerca de quatro capítulos por dia."] = "A classic plan: read the Old Testament once, the New Testament twice, and Psalms a second time. About four chapters a day.",
            ["Bíblia + Salmos e Provérbios"] = "Bible + Psalms and Proverbs",
            ["Leia a Bíblia inteira uma vez e releia Salmos e Provérbios ao longo do ano para dedicar mais tempo à oração e sabedoria."] = "Read the entire Bible once and revisit Psalms and Proverbs throughout the year for more time in prayer and wisdom."
        };

    private static readonly string[] PortuguesePlanTitles =
    [
        "Uma semana nos Salmos",
        "Evangelho de João",
        "Plano canônico (sequencial)",
        "Plano cronológico (aproximado)",
        "Plano misto (AT + NT)",
        "Plano M’Cheyne",
        "Bíblia + Salmos e Provérbios"
    ];

    private static readonly string[] EnglishPlanTitles =
    [
        "A Week in Psalms",
        "The Gospel of John",
        "Canonical Plan (Sequential)",
        "Chronological Plan (Approximate)",
        "Mixed Plan (OT + NT)",
        "M'Cheyne Plan",
        "Bible + Psalms and Proverbs"
    ];

    private static readonly string[] PortuguesePlanDescriptions =
    [
        "Sete salmos para encontrar inspiração no dia a dia.",
        "Uma jornada de 21 dias pelo evangelho de João.",
        "Gênesis ao Apocalipse em ordem tradicional. Simples de acompanhar; a primeira parte do ano concentra-se no Antigo Testamento.",
        "Livros organizados por períodos históricos para conectar acontecimentos. A sequência é aproximada, não uma cronologia versículo a versículo.",
        "Leituras do Antigo e do Novo Testamento todos os dias. Estilo equilibrado, recomendado para manter variedade; alguns trechos do NT se repetem.",
        "Plano clássico: Antigo Testamento uma vez, Novo Testamento duas vezes e Salmos em uma segunda leitura. Cerca de quatro capítulos por dia.",
        "Leia a Bíblia inteira uma vez e releia Salmos e Provérbios ao longo do ano para dedicar mais tempo à oração e sabedoria."
    ];

    private static readonly string[] EnglishPlanDescriptions =
    [
        "Seven psalms to find inspiration in everyday life.",
        "A 21-day journey through the Gospel of John.",
        "Read from Genesis to Revelation in traditional order. Easy to follow; the first part of the year focuses on the Old Testament.",
        "Books are organized by historical periods to connect events. The sequence is approximate, not a verse-by-verse chronology.",
        "Readings from the Old and New Testaments every day. A balanced approach for variety; some New Testament passages repeat.",
        "A classic plan: read the Old Testament once, the New Testament twice, and Psalms a second time. About four chapters a day.",
        "Read the entire Bible once and revisit Psalms and Proverbs throughout the year for more time in prayer and wisdom."
    ];

    public static bool IsPortuguese =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase);

    public static string Get(string portugueseKey) =>
        IsPortuguese ? portugueseKey : English.GetValueOrDefault(portugueseKey, portugueseKey);

    public static string GetBibleBookName(string bookCode, string portugueseName) =>
        IsPortuguese ? portugueseName : EnglishBibleBookNames.GetValueOrDefault(bookCode, portugueseName);

    public static string GetMinistryRole(string role)
    {
        if (IsPortuguese)
        {
            return role switch
            {
                "Member" => "Membro",
                "Female pastor" => "Pastora",
                "Leader" => "Líder",
                "Missionary" => "Missionário",
                "Female missionary" => "Missionária",
                "Deacon" => "Diácono",
                "Deaconess" => "Diaconisa",
                "Elder" => "Presbítero",
                "Female elder" => "Presbítera",
                "Evangelist" => "Evangelista",
                "Other" => "Outro",
                _ => role
            };
        }

        return English.GetValueOrDefault(role, role);
    }

    public static string GetWelcomeGreeting(
        bool isFirstLogin,
        string fullName,
        string ministryRole,
        DateTime localTime)
    {
        var timeOfDay = localTime.Hour switch
        {
            < 10 => 0,
            < 12 => 1,
            < 18 => 2,
            _ => 3
        };
        var messages = (IsPortuguese, isFirstLogin) switch
        {
            (true, true) => PortugueseFirstWelcomeMessages[timeOfDay],
            (true, false) => PortugueseReturningWelcomeMessages[timeOfDay],
            (false, true) => EnglishFirstWelcomeMessages[timeOfDay],
            _ => EnglishReturningWelcomeMessages[timeOfDay]
        };
        var message = messages[Random.Shared.Next(messages.Length)];
        return string.Format(CultureInfo.CurrentCulture, message, fullName, ministryRole);
    }

    public static (string Title, string Message) GetReminderNotification(
        bool isDefault,
        string title,
        string message,
        DateTime localTime)
    {
        if (!isDefault)
        {
            return (title, message);
        }

        var timeOfDay = localTime.Hour switch
        {
            < 11 => 0,
            < 12 => 1,
            < 18 => 2,
            _ => 3
        };
        var options = IsPortuguese
            ? PortugueseReminderMessages[timeOfDay]
            : EnglishReminderMessages[timeOfDay];
        var selected = options[Random.Shared.Next(options.Length)];
        return (selected.Title, selected.Message);
    }

    private static readonly ReminderContent[][] PortugueseReminderMessages =
    [
        [
            new("Momento da manhã", "Comece o dia com alguns minutos na Palavra."),
            new("Uma pausa com Deus", "Que a Palavra ilumine o seu começo de dia."),
            new("Leitura da manhã", "Reserve um tempo nesta manhã para estar com Deus."),
            new("Bom dia com a Palavra", "Abra a Bíblia e leve uma boa palavra para o seu dia.")
        ],
        [
            new("Um momento com a Palavra", "Faça uma pausa e encontre inspiração na Bíblia."),
            new("Pausa para refletir", "Que tal alguns minutos de leitura e reflexão?"),
            new("Seu momento de leitura", "A Palavra de Deus tem um lugar no seu dia."),
            new("Uma pausa abençoada", "Respire fundo e separe um instante para a Palavra.")
        ],
        [
            new("Momento da tarde", "Faça uma pausa nesta tarde para estar com a Palavra."),
            new("Reflexão da tarde", "Renove suas forças com alguns minutos de leitura."),
            new("Uma pausa com Deus", "Encontre um momento de paz na Palavra nesta tarde."),
            new("Leitura para a tarde", "Abra a Bíblia e reserve este momento para Deus.")
        ],
        [
            new("Momento da noite", "Encerre o dia com alguns minutos na Palavra."),
            new("Reflexão da noite", "Acalme o coração com uma leitura bíblica."),
            new("Uma pausa com Deus", "Que a Palavra acompanhe você nesta noite."),
            new("Leitura para a noite", "Separe um momento tranquilo para estar com Deus.")
        ]
    ];

    private static readonly ReminderContent[][] EnglishReminderMessages =
    [
        [
            new("Morning with the Word", "Start your day with a few minutes in Scripture."),
            new("A moment with God", "Let the Word bring light to the start of your day."),
            new("Morning reading", "Set aside a little time to be with God this morning."),
            new("Good morning", "Open your Bible and carry a hopeful word into your day.")
        ],
        [
            new("A moment with the Word", "Take a pause and find inspiration in the Bible."),
            new("A moment to reflect", "How about a few minutes of reading and reflection?"),
            new("Your reading moment", "Make room for the Word in your day."),
            new("A peaceful pause", "Take a breath and set aside a moment for Scripture.")
        ],
        [
            new("Afternoon reading", "Take a pause to spend a moment with the Word."),
            new("Afternoon reflection", "Find renewed strength in a few minutes of reading."),
            new("A moment with God", "Find a little peace in the Word this afternoon."),
            new("A pause for Scripture", "Open your Bible and set aside this moment for God.")
        ],
        [
            new("Evening with the Word", "End your day with a few minutes in Scripture."),
            new("Evening reflection", "Quiet your heart with a little Bible reading."),
            new("A moment with God", "May the Word be with you this evening."),
            new("A peaceful evening", "Set aside a quiet moment to spend with God.")
        ]
    ];

    private sealed record ReminderContent(string Title, string Message);

    private static readonly string[][] PortugueseFirstWelcomeMessages =
    [
        [
            "É bom ter você aqui nesta manhã, {0} ({1})!",
            "Que alegria receber você nesta manhã, {0} ({1})!",
            "Que sua manhã seja abençoada, {0} ({1})!"
        ],
        [
            "É muito bom ter você aqui, {0} ({1})!",
            "Que alegria receber você por aqui, {0} ({1})!",
            "Este espaço também é seu, {0} ({1})!"
        ],
        [
            "É bom ter você aqui nesta tarde, {0} ({1})!",
            "Uma tarde abençoada para você, {0} ({1})!",
            "Que alegria receber você nesta tarde, {0} ({1})!"
        ],
        [
            "É bom ter você aqui nesta noite, {0} ({1})!",
            "Que sua noite seja abençoada, {0} ({1})!",
            "Que alegria receber você nesta noite, {0} ({1})!"
        ]
    ];

    private static readonly string[][] PortugueseReturningWelcomeMessages =
    [
        [
            "Que bom ter você de volta nesta manhã, {0} ({1})!",
            "Uma manhã abençoada para você, {0} ({1})!",
            "Que alegria começar o dia com você de volta, {0} ({1})!"
        ],
        [
            "Que bom ter você de volta, {0} ({1})!",
            "Sentimos sua falta por aqui, {0} ({1})!",
            "Que bom continuar esta jornada com você, {0} ({1})!"
        ],
        [
            "Que bom ter você de volta nesta tarde, {0} ({1})!",
            "Uma tarde abençoada para você, {0} ({1})!",
            "Que alegria ter você de volta nesta tarde, {0} ({1})!"
        ],
        [
            "Que bom ter você de volta nesta noite, {0} ({1})!",
            "Uma noite abençoada para você, {0} ({1})!",
            "Que alegria ter você de volta nesta noite, {0} ({1})!"
        ]
    ];

    private static readonly string[][] EnglishFirstWelcomeMessages =
    [
        [
            "It's good to have you here this morning, {0} ({1})!",
            "What a joy to welcome you this morning, {0} ({1})!",
            "May your morning be blessed, {0} ({1})!"
        ],
        [
            "It's great to have you here, {0} ({1})!",
            "What a joy to welcome you, {0} ({1})!",
            "Make yourself at home, {0} ({1})!"
        ],
        [
            "It's good to have you here this afternoon, {0} ({1})!",
            "Wishing you a blessed afternoon, {0} ({1})!",
            "What a joy to welcome you this afternoon, {0} ({1})!"
        ],
        [
            "It's good to have you here this evening, {0} ({1})!",
            "May your evening be blessed, {0} ({1})!",
            "What a joy to welcome you this evening, {0} ({1})!"
        ]
    ];

    private static readonly string[][] EnglishReturningWelcomeMessages =
    [
        [
            "It's good to have you back this morning, {0} ({1})!",
            "Wishing you a blessed morning, {0} ({1})!",
            "What a joy to start the day with you back, {0} ({1})!"
        ],
        [
            "It's great to have you back, {0} ({1})!",
            "We missed having you here, {0} ({1})!",
            "It's good to continue this journey with you, {0} ({1})!"
        ],
        [
            "It's good to have you back this afternoon, {0} ({1})!",
            "Wishing you a blessed afternoon, {0} ({1})!",
            "What a joy to have you back this afternoon, {0} ({1})!"
        ],
        [
            "It's good to have you back this evening, {0} ({1})!",
            "Wishing you a blessed evening, {0} ({1})!",
            "What a joy to have you back this evening, {0} ({1})!"
        ]
    ];

    public static string Format(string portugueseFormat, params object?[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Get(portugueseFormat), arguments);

    public static string GetPlanTitle(int planId, string fallback = "") =>
        GetPlanString(planId, PortuguesePlanTitles, EnglishPlanTitles, fallback);

    public static string GetPlanDescription(int planId, string fallback = "") =>
        GetPlanString(planId, PortuguesePlanDescriptions, EnglishPlanDescriptions, fallback);

    private static string GetPlanString(
        int planId,
        IReadOnlyList<string> portuguese,
        IReadOnlyList<string> english,
        string fallback)
    {
        var index = planId - 1;
        if (index < 0 || index >= portuguese.Count)
        {
            return fallback;
        }

        return IsPortuguese ? portuguese[index] : english[index];
    }
}
