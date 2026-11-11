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
    private static readonly IReadOnlyDictionary<string, string> SpanishBibleBookNames =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["GEN"] = "Génesis",
            ["EXO"] = "Éxodo",
            ["LEV"] = "Levítico",
            ["NUM"] = "Números",
            ["DEU"] = "Deuteronomio",
            ["JOS"] = "Josué",
            ["JDG"] = "Jueces",
            ["RUT"] = "Rut",
            ["1SA"] = "1 Samuel",
            ["2SA"] = "2 Samuel",
            ["1KI"] = "1 Reyes",
            ["2KI"] = "2 Reyes",
            ["1CH"] = "1 Crónicas",
            ["2CH"] = "2 Crónicas",
            ["EZR"] = "Esdras",
            ["NEH"] = "Nehemías",
            ["EST"] = "Ester",
            ["JOB"] = "Job",
            ["PSA"] = "Salmos",
            ["PRO"] = "Proverbios",
            ["ECC"] = "Eclesiastés",
            ["SNG"] = "Cantares",
            ["ISA"] = "Isaías",
            ["JER"] = "Jeremías",
            ["LAM"] = "Lamentaciones",
            ["EZK"] = "Ezequiel",
            ["DAN"] = "Daniel",
            ["HOS"] = "Oseas",
            ["JOL"] = "Joel",
            ["AMO"] = "Amós",
            ["OBA"] = "Abdías",
            ["JON"] = "Jonás",
            ["MIC"] = "Miqueas",
            ["NAH"] = "Nahúm",
            ["NAM"] = "Nahúm",
            ["HAB"] = "Habacuc",
            ["ZEP"] = "Sofonías",
            ["HAG"] = "Hageo",
            ["ZEC"] = "Zacarías",
            ["MAL"] = "Malaquías",
            ["MAT"] = "Mateo",
            ["MRK"] = "Marcos",
            ["LUK"] = "Lucas",
            ["JOH"] = "Juan",
            ["JHN"] = "Juan",
            ["ACT"] = "Hechos",
            ["ROM"] = "Romanos",
            ["1CO"] = "1 Corintios",
            ["2CO"] = "2 Corintios",
            ["GAL"] = "Gálatas",
            ["EPH"] = "Efesios",
            ["PHP"] = "Filipenses",
            ["COL"] = "Colosenses",
            ["1TH"] = "1 Tesalonicenses",
            ["2TH"] = "2 Tesalonicenses",
            ["1TI"] = "1 Timoteo",
            ["2TI"] = "2 Timoteo",
            ["TIT"] = "Tito",
            ["PHM"] = "Filemón",
            ["HEB"] = "Hebreos",
            ["JAS"] = "Santiago",
            ["1PE"] = "1 Pedro",
            ["2PE"] = "2 Pedro",
            ["1JN"] = "1 Juan",
            ["2JN"] = "2 Juan",
            ["3JN"] = "3 Juan",
            ["JUD"] = "Judas",
            ["REV"] = "Apocalipsis"
        };

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
        var deviceCulture = CultureInfo.CurrentUICulture;
        if (string.IsNullOrWhiteSpace(deviceCulture.Name) || deviceCulture.Name == "iv")
        {
            deviceCulture = CultureInfo.CurrentCulture;
        }

        var language = deviceCulture.TwoLetterISOLanguageName;
        var cultureName = language switch
        {
            "pt" => "pt-BR",
            "es" => deviceCulture.Name.Contains("-", StringComparison.Ordinal) ? deviceCulture.Name : "es-ES",
            _ => "en-US"
        };
        var culture = CultureInfo.GetCultureInfo(cultureName);
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
            ["Seja Premium"] = "Become a Premium member",
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
            ["Bíblia Livre · leitura offline"] = "World English Bible (WEB) · Offline · Public Domain",
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
            ["Os planos de leitura, as leituras diárias e o acompanhamento do progresso são exclusivos do Premium."] = "Reading plans, daily readings, and progress tracking are exclusive to Premium.",
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
            ["A referência escolhida não está disponível na Bíblia."] = "The selected reference is not available in the Bible.",
            ["Escolha um capítulo e versículo disponíveis."] = "Choose an available chapter and verse.",
            ["Não foi possível carregar as opções de referência. Tente novamente."] = "Could not load the reference options. Try again.",
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
            ["Conceda a permissão de notificações e toque novamente em ativar."] = "Grant notification permission, then tap enable again.",
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

    private static readonly IReadOnlyDictionary<string, string> Spanish =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Símbolo do Bible Notify"] = "Logotipo de BibleNotify",
            ["Um momento com a Palavra pode mudar o seu dia."] = "Un momento con la Palabra puede cambiar tu día.",
            ["Entre ou crie sua conta para continuar."] = "Inicia sesión o crea una cuenta para continuar.",
            ["Nome"] = "Nombre",
            ["Sobrenome"] = "Apellido",
            ["Sua função no Corpo de Cristo"] = "Tu función en el cuerpo de Cristo",
            ["Telefone (opcional)"] = "Teléfono (opcional)",
            ["E-mail"] = "Correo electrónico",
            ["Senha (mín. 8 caracteres)"] = "Contraseña (mín. 8 caracteres)",
            ["Confirme a senha"] = "Confirma la contraseña",
            ["Senha"] = "Contraseña",
            ["Continuar sem conta"] = "Continuar sin cuenta",
            ["Abre o aplicativo em modo sem conta e prepara a Bíblia offline"] = "Abre la aplicación como invitado y prepárala para leer la Biblia",
            ["Criar minha conta"] = "Crear mi cuenta",
            ["Entrar"] = "Iniciar sesión",
            ["Já tem uma conta? Entrar"] = "¿Ya tienes una cuenta? Inicia sesión",
            ["Ainda não tem conta? Criar conta"] = "¿Aún no tienes una cuenta? Crea una",
            ["As senhas não conferem."] = "Las contraseñas no coinciden.",
            ["E-mail ou senha incorretos."] = "Correo electrónico o contraseña incorrectos.",
            ["Já existe uma conta com este e-mail."] = "Ya existe una cuenta con este correo electrónico.",
            ["Não foi possível abrir os dados locais. Reinstale o aplicativo."] = "No se pudieron abrir los datos locales. Reinstala la aplicación.",
            ["Não foi possível preparar o aplicativo. Tente novamente."] = "No se pudo preparar la aplicación. Inténtalo de nuevo.",
            ["Não foi possível acessar a conta local. Tente novamente."] = "No se pudo acceder a la cuenta local. Inténtalo de nuevo.",
            ["O banco da Bíblia está inválido. Reinstale o aplicativo."] = "La base de datos de la Biblia no es válida. Reinstala la aplicación.",
            ["Não foi possível preparar a Bíblia offline. Tente novamente."] = "No se pudo preparar la Biblia sin conexión. Inténtalo de nuevo.",
            ["Não foi possível abrir a Bíblia offline. Tente novamente."] = "No se pudo abrir la Biblia sin conexión. Inténtalo de nuevo.",
            ["Leve sua jornada com você"] = "Lleva tu camino contigo",
            ["Crie sua conta para guardar seus dados e continuar depois."] = "Crea una cuenta para guardar tus datos y continuar más tarde.",
            ["Criar conta"] = "Crear cuenta",
            ["Seja Premium"] = "Hazte miembro Premium",
            ["Abre o cadastro de uma nova conta"] = "Abre el formulario para crear una cuenta",
            ["{0} dias consecutivos"] = "{0} días consecutivos",
            ["Sua constância na Palavra"] = "Tu constancia en la Palabra",
            ["Sequência de leitura"] = "Racha de lectura",
            ["Premium"] = "Premium",
            ["SUA JORNADA DE LEITURA"] = "TU CAMINO DE LECTURA",
            ["UM COMEÇO PARA HOJE"] = "UN COMIENZO PARA HOY",
            ["Ler capítulo"] = "Leer capítulo",
            ["Abre a leitura da Bíblia armazenada no aparelho"] = "Abre la Biblia en este dispositivo",
            ["Suas anotações"] = "Tus notas",
            ["Guarde reflexões e versículos importantes."] = "Guarda reflexiones y versículos importantes.",
            ["Abrir"] = "Abrir",
            ["Abre suas anotações bíblicas"] = "Abre tus notas bíblicas",
            ["Abre as opções Premium"] = "Abre las opciones Premium",
            ["Início"] = "Inicio",
            ["Notas"] = "Notas",
            ["Bíblia"] = "Biblia",
            ["SELECIONE LIVRO E CAPÍTULO"] = "SELECCIONA EL LIBRO Y EL CAPÍTULO",
            ["Livro"] = "Libro",
            ["Capítulo"] = "Capítulo",
            ["Selecione um livro da Bíblia"] = "Selecciona un libro de la Biblia",
            ["Selecione o capítulo"] = "Selecciona el capítulo",
            ["Bíblia Livre · leitura offline"] = "Reina-Valera 1909 · sin conexión · Dominio público",
            ["Abrindo capítulo..."] = "Abriendo capítulo...",
            ["Anterior"] = "Anterior",
            ["Próximo"] = "Siguiente",
            ["Constância, um dia de cada vez."] = "Constancia, un día a la vez.",
            ["Acompanhe uma jornada guiada pela Bíblia."] = "Sigue un camino guiado por la Biblia.",
            ["ESCOLHA UM PLANO"] = "ELIGE UN PLAN",
            ["PLANOS DE LEITURA"] = "PLANES DE LECTURA",
            ["Escolha uma jornada para conhecer os livros, a duração e o foco de cada plano."] = "Elige un camino para conocer los libros, la duración y el enfoque de cada plan.",
            ["Toque para selecionar este plano"] = "Toca para seleccionar este plan",
            ["Toque para ver os recursos Premium"] = "Toca para ver las opciones Premium",
            ["Selecionado"] = "Seleccionado",
            ["Nenhum plano de leitura disponível no momento."] = "No hay planes de lectura disponibles en este momento.",
            ["SUA JORNADA"] = "TU CAMINO",
            ["ESCOLHA OUTRO PLANO"] = "ELIGE OTRO PLAN",
            ["Seu plano ainda não começou."] = "Tu plan aún no ha comenzado.",
            ["Toque em Iniciar plano para ver as leituras diárias."] = "Toca en Iniciar plan para ver las lecturas diarias.",
            ["Inicie o plano para ver as leituras diárias."] = "Inicia el plan para ver las lecturas diarias.",
            ["{0} dias"] = "{0} días",
            ["{0} · Bíblia Livre"] = "{0} · Reina-Valera 1909",
            ["{0} {1}:{2} · Bíblia Livre"] = "{0} {1}:{2} · Reina-Valera 1909",
            ["Dia {0}"] = "Día {0}",
            ["🔒 PLANOS E PROGRESSO PREMIUM"] = "🔒 PLANES Y PROGRESO PREMIUM",
            ["Conheça os formatos abaixo. Para iniciar um plano, acompanhar leituras e salvar seu progresso, é necessário o Premium."] = "Descubre los planes disponibles. Necesitas Premium para iniciar un plan, seguir tus lecturas y guardar tu progreso.",
            ["LEITURAS DO PLANO"] = "LECTURAS DEL PLAN",
            ["D{0}"] = "Día {0}",
            ["Concluído"] = "Completado",
            ["Ler"] = "Leer",
            ["Marcar lido"] = "Marcar como leído",
            ["Anotações"] = "Notas",
            ["Guarde o que tocou você."] = "Guarda lo que te ha llegado al corazón.",
            ["Organize e releia suas reflexões bíblicas."] = "Organiza y vuelve a leer tus reflexiones bíblicas.",
            ["🔒 ANOTAÇÕES EXCLUSIVAS DO PREMIUM"] = "🔒 NOTAS EXCLUSIVAS DE PREMIUM",
            ["NOVA ANOTAÇÃO"] = "NUEVA NOTA",
            ["Cap."] = "Cap.",
            ["Vers."] = "Vers.",
            ["Escreva sua reflexão..."] = "Escribe tu reflexión...",
            ["Desbloqueie o Premium para escrever e salvar suas reflexões."] = "Activa Premium para escribir y guardar tus reflexiones.",
            ["{0:dd/MM/yyyy}"] = "{0:dd/MM/yyyy}",
            ["Excluir"] = "Eliminar",
            ["Planos"] = "Planes",
            ["Avisos"] = "Recordatorios",
            ["Lembretes que acolhem."] = "Recordatorios que te acompañan.",
            ["Uma pausa para estar com a Palavra."] = "Una pausa para estar con la Palabra.",
            ["NOTIFICAÇÕES"] = "NOTIFICACIONES",
            ["Os lembretes serão configurados na próxima etapa."] = "Los recordatorios se configurarán en una próxima actualización.",
            ["O agendamento local e as permissões do aparelho serão implementados antes de ativar os avisos."] = "La programación local y los permisos del dispositivo se implementarán antes de activar los recordatorios.",
            ["Personalize sua experiência"] = "Personaliza tu experiencia",
            ["Voltar para ajustes"] = "Volver a ajustes",
            ["Conta"] = "Cuenta",
            ["Seus dados e perfil"] = "Tus datos y perfil",
            ["Planos e benefícios"] = "Planes y beneficios",
            ["Os planos de leitura, as leituras diárias e o acompanhamento do progresso são exclusivos do Premium."] = "Los planes de lectura, las lecturas diarias y el seguimiento del progreso son exclusivos de Premium.",
            ["Lembretes de leitura"] = "Recordatorios de lectura",
            ["Tema inválido."] = "El tema no es válido.",
            ["Entre ou crie uma conta para acessar este recurso."] = "Inicia sesión o crea una cuenta para acceder a esta función.",
            ["A senha deve ter pelo menos 8 caracteres."] = "La contraseña debe tener al menos 8 caracteres.",
            ["A senha deve ter entre 8 e 1024 caracteres."] = "La contraseña debe tener entre 8 y 1024 caracteres.",
            ["Confira o tamanho dos dados de perfil."] = "Revisa la longitud de los datos del perfil.",
            ["A anotação deve ter entre 1 e 10000 caracteres."] = "La nota debe tener entre 1 y 10.000 caracteres.",
            ["O título deve ter até 100 caracteres e a mensagem até 1000."] = "El título puede tener hasta 100 caracteres y el mensaje hasta 1.000.",
            ["É hora de um momento com a Palavra de Deus."] = "Es hora de dedicar un momento a la Palabra de Dios.",
            ["Separe alguns minutos para ler a Bíblia."] = "Reserva unos minutos para leer la Biblia.",
            ["A Palavra de Deus espera por você. Vamos ler?"] = "La Palabra de Dios te espera. ¿Leemos?",
            ["Faça uma pausa e fortaleça sua fé com a Palavra."] = "Haz una pausa y fortalece tu fe con la Palabra.",
            ["Seu momento de leitura bíblica chegou."] = "Ha llegado tu momento de leer la Biblia.",
            ["Que tal renovar o coração com a Palavra de Deus?"] = "¿Qué te parece renovar el corazón con la Palabra de Dios?",
            ["Abra a Bíblia e reserve este momento para Deus."] = "Abre la Biblia y dedica este momento a Dios.",
            ["Um momento para a Palavra."] = "Un momento para la Palabra.",
            ["Aparência"] = "Apariencia",
            ["Premium (prévia)"] = "Premium (vista previa)",
            ["SEUS DADOS"] = "TUS DATOS",
            ["Veja e atualize as informações da sua conta."] = "Consulta y actualiza la información de tu cuenta.",
            ["Entre ou crie uma conta para cadastrar e editar seu perfil."] = "Inicia sesión o crea una cuenta para completar y editar tu perfil.",
            ["Função no Corpo de Cristo"] = "Función en el cuerpo de Cristo",
            ["Telefone"] = "Teléfono",
            ["Salvar perfil"] = "Guardar perfil",
            ["Entrar ou criar conta"] = "Iniciar sesión o crear una cuenta",
            ["Sair da conta"] = "Cerrar sesión",
            ["ESCOLHA SEU PLANO"] = "ELIGE TU PLAN",
            ["Desbloqueie planos de leitura, anotações e personalização completa de notificações."] = "Desbloquea planes de lectura, notas y personalización completa de los recordatorios.",
            ["Mensal"] = "Mensual",
            ["R$ 9,99 / mês"] = "R$ 9,99 / mes",
            ["Escolher"] = "Elegir",
            ["Semestral"] = "Semestral",
            ["R$ 45,99 / 6 meses"] = "R$ 45,99 / 6 meses",
            ["Anual"] = "Anual",
            ["R$ 79,99 / ano"] = "R$ 79,99 / año",
            ["A contratação ainda não está ativa. Nenhuma cobrança será feita até a integração de pagamento."] = "Las compras aún no están disponibles. No se realizará ningún cobro hasta que se integre el sistema de pagos.",
            ["Prévia de demonstração — não é uma compra nem uma assinatura."] = "Vista previa de demostración: no es una compra ni una suscripción.",
            ["Ativar prévia Premium neste aparelho"] = "Activar la vista previa de Premium en este dispositivo",
            ["Desativar prévia Premium"] = "Desactivar la vista previa de Premium",
            ["Visitantes não podem configurar notificações. Contas gratuitas podem alterar somente os horários dos dois lembretes padrão."] = "Los invitados no pueden configurar recordatorios. Las cuentas gratuitas solo pueden cambiar la hora de los dos recordatorios predeterminados.",
            ["Entre ou crie uma conta para configurar os horários dos lembretes padrão."] = "Inicia sesión o crea una cuenta para cambiar la hora de los recordatorios predeterminados.",
            ["Selecione um lembrete"] = "Selecciona un recordatorio",
            ["SEUS LEMBRETES"] = "TUS RECORDATORIOS",
            ["Editar"] = "Editar",
            ["Ativar notificações neste aparelho"] = "Activar recordatorios en este dispositivo",
            ["Personalizar, criar ou excluir lembretes é exclusivo do Premium."] = "Personalizar, crear o eliminar recordatorios es una función exclusiva de Premium.",
            ["EDITANDO: {0}"] = "EDITANDO: {0}",
            ["Título"] = "Título",
            ["Mensagem"] = "Mensaje",
            ["Salvar alterações do lembrete"] = "Guardar cambios del recordatorio",
            ["Novo título"] = "Nuevo título",
            ["Nova mensagem"] = "Nuevo mensaje",
            ["Criar lembrete"] = "Crear recordatorio",
            ["Conhecer planos Premium"] = "Descubrir los planes Premium",
            ["APARÊNCIA"] = "APARIENCIA",
            ["Use o tema do aparelho ou escolha claro ou escuro. A opção Sistema acompanha a configuração atual do celular."] = "Usa el tema del dispositivo o elige el modo claro u oscuro. La opción Sistema sigue la configuración actual del teléfono.",
            ["Aplicar tema"] = "Aplicar tema",
            ["Sistema"] = "Sistema",
            ["Claro"] = "Claro",
            ["Escuro"] = "Oscuro",
            ["Membro"] = "Miembro",
            ["Pastor"] = "Pastor",
            ["Pastora"] = "Pastora",
            ["Líder"] = "Líder",
            ["Missionário"] = "Misionero",
            ["Missionária"] = "Misionera",
            ["Diácono"] = "Diácono",
            ["Diaconisa"] = "Diaconisa",
            ["Presbítero"] = "Anciano",
            ["Presbítera"] = "Anciana",
            ["Evangelista"] = "Evangelista",
            ["Outro"] = "Otro",
            ["Não foi possível escolher um versículo para hoje."] = "No se pudo elegir el versículo de hoy.",
            ["{0} de {1} dias concluídos"] = "{0} de {1} días completados",
            ["Nenhum plano iniciado"] = "No has iniciado ningún plan",
            ["Parabéns! Você concluiu este plano."] = "¡Felicidades! Has completado este plan.",
            ["Próxima leitura · Dia {0}: {1}"] = "Próxima lectura · Día {0}: {1}",
            ["Escolha um plano para organizar sua leitura diária."] = "Elige un plan para organizar tu lectura diaria.",
            ["Comece uma jornada de leitura no seu ritmo."] = "Comienza un camino de lectura a tu propio ritmo.",
            ["Não foi possível carregar o versículo. Tente novamente."] = "No se pudo cargar el versículo. Inténtalo de nuevo.",
            ["Não foi possível carregar os dados. Tente novamente."] = "No se pudieron cargar los datos. Inténtalo de nuevo.",
            ["Que bom ter você por aqui!"] = "¡Qué bueno tenerte aquí!",
            ["Que bom ver você novamente."] = "Qué bueno verte de nuevo.",
            ["Modo visitante"] = "Modo invitado",
            ["Visitante"] = "Invitado",
            ["Acompanhe seus dias consecutivos com uma conta Premium."] = "Sigue tus días consecutivos con una cuenta Premium.",
            ["Acompanhe seus dias consecutivos com o Premium."] = "Sigue tus días consecutivos con Premium.",
            ["Ver plano"] = "Ver plan",
            ["Escolher plano"] = "Elegir plan",
            ["Não foi possível carregar os planos. Tente novamente."] = "No se pudieron cargar los planes. Inténtalo de nuevo.",
            ["Este plano ainda não está disponível."] = "Este plan aún no está disponible.",
            ["Não foi possível iniciar o plano. Tente novamente."] = "No se pudo iniciar el plan. Inténtalo de nuevo.",
            ["Marcar o progresso do plano é um benefício Premium."] = "Registrar el progreso del plan es un beneficio de Premium.",
            ["Não foi possível atualizar o progresso. Tente novamente."] = "No se pudo actualizar el progreso. Inténtalo de nuevo.",
            ["🔒 Disponível no Premium"] = "🔒 Disponible con Premium",
            ["Continuar plano"] = "Continuar plan",
            ["Iniciar plano"] = "Iniciar plan",
            ["Usar este plano"] = "Usar este plan",
            ["Plano {0} selecionado. Toque em {1} para prosseguir."] = "Plan {0} seleccionado. Toca en {1} para continuar.",
            ["Plano {0} iniciado! Suas leituras estão prontas abaixo."] = "¡Plan {0} iniciado! Tus lecturas están listas abajo.",
            ["Criar conta e conhecer o Premium"] = "Crear una cuenta y descubrir Premium",
            ["Ver prévia Premium"] = "Ver vista previa de Premium",
            ["Este recurso é exclusivo do Premium. Veja os planos e crie uma conta para continuar."] = "Esta función es exclusiva de Premium. Consulta los planes y crea una cuenta para continuar.",
            ["Este recurso é exclusivo do Premium. Conheça os planos para criar e organizar suas anotações."] = "Esta función es exclusiva de Premium. Descubre los planes para crear y organizar tus notas.",
            ["Suas anotações aparecerão aqui quando o Premium estiver ativo."] = "Tus notas aparecerán aquí cuando Premium esté activo.",
            ["Suas anotações aparecerão aqui."] = "Tus notas aparecerán aquí.",
            ["🔒 Salvar anotação · Premium"] = "🔒 Guardar nota · Premium",
            ["Salvar anotação"] = "Guardar nota",
            ["Salvar alterações"] = "Guardar cambios",
            ["EDITANDO ANOTAÇÃO"] = "EDITANDO NOTA",
            ["Cancelar edição"] = "Cancelar edición",
            ["Você não tem permissão para editar esta anotação."] = "No tienes permiso para editar esta nota.",
            ["Não foi possível carregar as anotações. Tente novamente."] = "No se pudieron cargar las notas. Inténtalo de nuevo.",
            ["Não foi possível carregar a Bíblia offline."] = "No se pudo cargar la Biblia.",
            ["Escolha uma referência e escreva sua anotação."] = "Elige una referencia y escribe tu nota.",
            ["Não foi possível salvar a anotação."] = "No se pudo guardar la nota.",
            ["Anotação salva, mas não foi possível atualizar a lista."] = "La nota se guardó, pero no se pudo actualizar la lista.",
            ["Não foi possível excluir a anotação."] = "No se pudo eliminar la nota.",
            ["A referência solicitada não está disponível."] = "La referencia solicitada no está disponible.",
            ["A referência escolhida não está disponível na Bíblia."] = "La referencia seleccionada no está disponible en la Biblia.",
            ["Escolha um capítulo e versículo disponíveis."] = "Elige un capítulo y un versículo disponibles.",
            ["Não foi possível carregar as opções de referência. Tente novamente."] = "No se pudieron cargar las opciones de referencia. Inténtalo de nuevo.",
            ["Este capítulo não está disponível."] = "Este capítulo no está disponible.",
            ["Capítulo carregado, mas não foi possível salvar seu progresso."] = "El capítulo se cargó, pero no se pudo guardar tu progreso.",
            ["Não foi possível carregar a Bíblia offline. Tente novamente."] = "No se pudo cargar la Biblia. Inténtalo de nuevo.",
            ["Não foi possível abrir este capítulo. Tente novamente."] = "No se pudo abrir este capítulo. Inténtalo de nuevo.",
            ["Ajustes"] = "Ajustes",
            ["Notificações"] = "Notificaciones",
            ["Tema"] = "Tema",
            ["Premium · prévia local"] = "Premium · vista previa local",
            ["Conta gratuita"] = "Cuenta gratuita",
            ["Visitante · Bíblia offline"] = "Invitado · Biblia",
            ["🔒 Alterar horários · precisa de conta"] = "🔒 Cambiar horarios · requiere una cuenta",
            ["Salvar horário"] = "Guardar horario",
            ["Recurso Premium. Entre ou crie uma conta para conhecer a prévia."] = "Función Premium. Inicia sesión o crea una cuenta para probar la vista previa.",
            ["Recurso exclusivo do Premium. Ative a prévia para experimentar."] = "Función exclusiva de Premium. Activa la vista previa para probarla.",
            ["Não foi possível carregar as configurações."] = "No se pudo cargar la configuración.",
            ["Perfil atualizado."] = "Perfil actualizado.",
            ["Não foi possível salvar o perfil."] = "No se pudo guardar el perfil.",
            ["Prévia Premium ativada neste aparelho. Nenhuma compra ou cobrança foi realizada."] = "Vista previa de Premium activada en este dispositivo. No se realizó ninguna compra ni cobro.",
            ["Prévia Premium desativada."] = "Vista previa de Premium desactivada.",
            ["A prévia Premium só fica disponível em compilações de desenvolvimento."] = "La vista previa de Premium solo está disponible en versiones de desarrollo.",
            ["Dia inválido para o plano ativo."] = "El día no es válido para el plan activo.",
            ["Não foi possível atualizar o nível da conta."] = "No se pudo actualizar el nivel de la cuenta.",
            ["O nível da conta foi atualizado, mas não foi possível atualizar os lembretes."] = "Se actualizó el nivel de la cuenta, pero no se pudieron actualizar los recordatorios.",
            ["Tema atualizado."] = "Tema actualizado.",
            ["Não foi possível salvar o tema."] = "No se pudo guardar el tema.",
            ["Selecione um lembrete."] = "Selecciona un recordatorio.",
            ["Lembrete atualizado."] = "Recordatorio actualizado.",
            ["Não foi possível salvar o lembrete."] = "No se pudo guardar el recordatorio.",
            ["Horário salvo, mas não foi possível atualizar o agendamento no aparelho."] = "Se guardó el horario, pero no se pudo actualizar la programación en el dispositivo.",
            ["Novo lembrete criado."] = "Nuevo recordatorio creado.",
            ["Não foi possível criar o lembrete."] = "No se pudo crear el recordatorio.",
            ["Lembrete criado, mas não foi possível atualizar o agendamento no aparelho."] = "Se creó el recordatorio, pero no se pudo actualizar la programación en el dispositivo.",
            ["Editando: {0}"] = "Editando: {0}",
            ["Lembrete excluído."] = "Recordatorio eliminado.",
            ["Não foi possível excluir o lembrete."] = "No se pudo eliminar el recordatorio.",
            ["Lembrete excluído, mas não foi possível atualizar o agendamento no aparelho."] = "Se eliminó el recordatorio, pero no se pudo actualizar la programación en el dispositivo.",
            ["Não foi possível cancelar os lembretes. Tente novamente."] = "No se pudieron cancelar los recordatorios. Inténtalo de nuevo.",
            ["Entre ou crie uma conta para continuar. A integração de pagamento ainda será configurada."] = "Inicia sesión o crea una cuenta para continuar. El sistema de pagos aún no está disponible.",
            ["A integração de pagamento ainda não está configurada. Nenhuma cobrança foi realizada."] = "El sistema de pagos aún no está configurado. No se realizó ningún cobro.",
            ["Conceda a permissão de notificações e toque novamente em ativar."] = "Concede el permiso de notificaciones y vuelve a tocar Activar.",
            ["Lembretes diários ativados neste aparelho."] = "Recordatorios diarios activados en este dispositivo.",
            ["Plano {0}"] = "Plan {0}",
            ["Entendi"] = "Entendido",
            ["Não foi possível carregar os lembretes."] = "No se pudieron cargar los recordatorios.",
            ["Não foi possível ativar os lembretes neste aparelho."] = "No se pudieron activar los recordatorios en este dispositivo.",
            ["Nome, sobrenome e função ministerial são obrigatórios."] = "El nombre, el apellido y la función ministerial son obligatorios.",
            ["Plano de leitura não encontrado."] = "No se encontró el plan de lectura.",
            ["A anotação não pode ficar vazia."] = "La nota no puede estar vacía.",
            ["Somente os dois lembretes padrão podem ser alterados nesta conta."] = "Esta cuenta solo puede cambiar los dos recordatorios predeterminados.",
            ["A conta local não foi encontrada."] = "No se encontró la cuenta local.",
            ["Este recurso está disponível no Premium."] = "Esta función está disponible con Premium.",
            ["Preencha nome, sobrenome e função no corpo de Cristo."] = "Completa el nombre, el apellido y la función en el cuerpo de Cristo.",
            ["Digite um e-mail válido."] = "Introduce una dirección de correo electrónico válida.",
            ["Informe um título e uma mensagem para o lembrete."] = "Escribe un título y un mensaje para el recordatorio.",
            ["Momento da manhã"] = "Momento de la mañana",
            ["Comece o dia com alguns minutos na Palavra."] = "Comienza el día dedicando unos minutos a la Palabra.",
            ["Pausa para refletir"] = "Pausa para reflexionar",
            ["Reserve um instante para ler a Bíblia."] = "Reserva un momento para leer la Biblia.",
            ["Uma semana nos Salmos"] = "Una semana en los Salmos",
            ["Sete salmos para encontrar inspiração no dia a dia."] = "Siete salmos para encontrar inspiración cada día.",
            ["Evangelho de João"] = "Evangelio de Juan",
            ["Uma jornada de 21 dias pelo evangelho de João."] = "Un recorrido de 21 días por el Evangelio de Juan.",
            ["Plano canônico (sequencial)"] = "Plan canónico (secuencial)",
            ["Gênesis ao Apocalipse em ordem tradicional. Simples de acompanhar; a primeira parte do ano concentra-se no Antigo Testamento."] = "De Génesis a Apocalipsis en el orden tradicional. Fácil de seguir; la primera parte del año se centra en el Antiguo Testamento.",
            ["Plano cronológico (aproximado)"] = "Plan cronológico (aproximado)",
            ["Livros organizados por períodos históricos para conectar acontecimentos. A sequência é aproximada, não uma cronologia versículo a versículo."] = "Libros organizados por períodos históricos para relacionar los acontecimientos. El orden es aproximado, no una cronología versículo por versículo.",
            ["Plano misto (AT + NT)"] = "Plan mixto (AT + NT)",
            ["Leituras do Antigo e do Novo Testamento todos os dias. Estilo equilibrado, recomendado para manter variedade; alguns trechos do NT se repetem."] = "Lecturas diarias del Antiguo y del Nuevo Testamento. Un enfoque equilibrado para mantener la variedad; algunos pasajes del NT se repiten.",
            ["Plano M’Cheyne"] = "Plan de M'Cheyne",
            ["Plano clássico: Antigo Testamento uma vez, Novo Testamento duas vezes e Salmos em uma segunda leitura. Cerca de quatro capítulos por dia."] = "Un plan clásico: leer el Antiguo Testamento una vez, el Nuevo Testamento dos veces y los Salmos por segunda vez. Unos cuatro capítulos al día.",
            ["Bíblia + Salmos e Provérbios"] = "Biblia + Salmos y Proverbios",
            ["Leia a Bíblia inteira uma vez e releia Salmos e Provérbios ao longo do ano para dedicar mais tempo à oração e sabedoria."] = "Lee toda la Biblia una vez y vuelve a leer Salmos y Proverbios durante el año para dedicar más tiempo a la oración y la sabiduría."
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

    private static readonly string[] SpanishPlanTitles =
    [
        "Una semana en los Salmos",
        "Evangelio de Juan",
        "Plan canónico (secuencial)",
        "Plan cronológico (aproximado)",
        "Plan mixto (AT + NT)",
        "Plan de M'Cheyne",
        "Biblia + Salmos y Proverbios"
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

    private static readonly string[] SpanishPlanDescriptions =
    [
        "Siete salmos para encontrar inspiración cada día.",
        "Un recorrido de 21 días por el Evangelio de Juan.",
        "De Génesis a Apocalipsis en el orden tradicional. Fácil de seguir; la primera parte del año se centra en el Antiguo Testamento.",
        "Libros organizados por períodos históricos para relacionar los acontecimientos. El orden es aproximado, no una cronología versículo por versículo.",
        "Lecturas diarias del Antiguo y del Nuevo Testamento. Un enfoque equilibrado para mantener la variedad; algunos pasajes del NT se repiten.",
        "Un plan clásico: leer el Antiguo Testamento una vez, el Nuevo Testamento dos veces y los Salmos por segunda vez. Unos cuatro capítulos al día.",
        "Lee toda la Biblia una vez y vuelve a leer Salmos y Proverbios durante el año para dedicar más tiempo a la oración y la sabiduría."
    ];

    public static bool IsSpanish =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("es", StringComparison.OrdinalIgnoreCase);

    public static bool IsPortuguese =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase);

    public static string BibleLanguageCode =>
        IsPortuguese ? "pt-BR" : IsSpanish ? "es" : "en";

    public static string Get(string portugueseKey)
    {
        if (IsPortuguese)
        {
            return portugueseKey;
        }

        return IsSpanish
            ? Spanish.GetValueOrDefault(portugueseKey, portugueseKey)
            : English.GetValueOrDefault(portugueseKey, portugueseKey);
    }

    public static string GetBibleBookName(string bookCode, string portugueseName)
    {
        if (IsPortuguese)
        {
            return portugueseName;
        }

        return IsSpanish
            ? SpanishBibleBookNames.GetValueOrDefault(bookCode, portugueseName)
            : EnglishBibleBookNames.GetValueOrDefault(bookCode, portugueseName);
    }

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

        if (IsSpanish)
        {
            return role switch
            {
                "Member" => "Miembro",
                "Female pastor" => "Pastora",
                "Leader" => "Líder",
                "Missionary" => "Misionero",
                "Female missionary" => "Misionera",
                "Deacon" => "Diácono",
                "Deaconess" => "Diaconisa",
                "Elder" => "Anciano",
                "Female elder" => "Anciana",
                "Other" => "Otro",
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
        var messages = (IsPortuguese, IsSpanish, isFirstLogin) switch
        {
            (true, _, true) => PortugueseFirstWelcomeMessages[timeOfDay],
            (true, _, false) => PortugueseReturningWelcomeMessages[timeOfDay],
            (false, true, true) => SpanishFirstWelcomeMessages[timeOfDay],
            (false, true, false) => SpanishReturningWelcomeMessages[timeOfDay],
            (false, false, true) => EnglishFirstWelcomeMessages[timeOfDay],
            _ => EnglishReturningWelcomeMessages[timeOfDay]
        };
        var message = messages[Random.Shared.Next(messages.Length)];
        message = message.Replace("{0} ({1})", "{1} {0}", StringComparison.Ordinal);
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
            : IsSpanish
                ? SpanishReminderMessages[timeOfDay]
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

    private static readonly ReminderContent[][] SpanishReminderMessages =
    [
        [
            new("Un momento por la mañana", "Comienza el día dedicando unos minutos a la Palabra."),
            new("Un momento con Dios", "Deja que la Palabra ilumine el comienzo de tu día."),
            new("Lectura de la mañana", "Reserva un momento esta mañana para estar con Dios."),
            new("Buenos días con la Palabra", "Abre la Biblia y lleva un mensaje de esperanza contigo.")
        ],
        [
            new("Un momento con la Palabra", "Haz una pausa y encuentra inspiración en la Biblia."),
            new("Pausa para reflexionar", "¿Qué te parece dedicar unos minutos a leer y reflexionar?"),
            new("Tu momento de lectura", "La Palabra de Dios también tiene un lugar en tu día."),
            new("Una pausa bendecida", "Respira hondo y reserva un momento para la Palabra.")
        ],
        [
            new("Un momento por la tarde", "Haz una pausa esta tarde para estar con la Palabra."),
            new("Reflexión de la tarde", "Renueva tus fuerzas con unos minutos de lectura."),
            new("Un momento con Dios", "Encuentra un momento de paz en la Palabra esta tarde."),
            new("Lectura para la tarde", "Abre la Biblia y dedica este momento a Dios.")
        ],
        [
            new("Un momento por la noche", "Termina el día dedicando unos minutos a la Palabra."),
            new("Reflexión de la noche", "Serena el corazón con una lectura bíblica."),
            new("Un momento con Dios", "Que la Palabra te acompañe esta noche."),
            new("Lectura para la noche", "Reserva un momento tranquilo para estar con Dios.")
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

    private static readonly string[][] SpanishFirstWelcomeMessages =
    [
        [
            "Qué bueno tenerte aquí esta mañana, {0} ({1}).",
            "¡Qué alegría darte la bienvenida esta mañana, {0} ({1})!",
            "Que tengas una mañana bendecida, {0} ({1})."
        ],
        [
            "¡Qué bueno tenerte aquí, {0} ({1})!",
            "¡Qué alegría recibirte, {0} ({1})!",
            "¡Esta también es tu casa, {0} ({1})!"
        ],
        [
            "Qué bueno tenerte aquí esta tarde, {0} ({1}).",
            "Que tengas una tarde bendecida, {0} ({1}).",
            "¡Qué alegría darte la bienvenida esta tarde, {0} ({1})!"
        ],
        [
            "Qué bueno tenerte aquí esta noche, {0} ({1}).",
            "Que tengas una noche bendecida, {0} ({1}).",
            "¡Qué alegría darte la bienvenida esta noche, {0} ({1})!"
        ]
    ];

    private static readonly string[][] SpanishReturningWelcomeMessages =
    [
        [
            "Qué bueno tenerte de vuelta esta mañana, {0} ({1}).",
            "Que tengas una mañana bendecida, {0} ({1}).",
            "¡Qué alegría empezar el día contigo de vuelta, {0} ({1})!"
        ],
        [
            "¡Qué bueno tenerte de vuelta, {0} ({1})!",
            "Te echábamos de menos, {0} ({1}).",
            "Qué bueno continuar este camino contigo, {0} ({1})."
        ],
        [
            "Qué bueno tenerte de vuelta esta tarde, {0} ({1}).",
            "Que tengas una tarde bendecida, {0} ({1}).",
            "¡Qué alegría tenerte de vuelta esta tarde, {0} ({1})!"
        ],
        [
            "Qué bueno tenerte de vuelta esta noche, {0} ({1}).",
            "Que tengas una noche bendecida, {0} ({1}).",
            "¡Qué alegría tenerte de vuelta esta noche, {0} ({1})!"
        ]
    ];

    public static string Format(string portugueseFormat, params object?[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Get(portugueseFormat), arguments);

    public static string GetPlanTitle(int planId, string fallback = "") =>
        GetPlanString(planId, PortuguesePlanTitles, EnglishPlanTitles, SpanishPlanTitles, fallback);

    public static string GetPlanDescription(int planId, string fallback = "") =>
        GetPlanString(planId, PortuguesePlanDescriptions, EnglishPlanDescriptions, SpanishPlanDescriptions, fallback);

    private static string GetPlanString(
        int planId,
        IReadOnlyList<string> portuguese,
        IReadOnlyList<string> english,
        IReadOnlyList<string> spanish,
        string fallback)
    {
        var index = planId - 1;
        if (index < 0 || index >= portuguese.Count)
        {
            return fallback;
        }

        return IsPortuguese
            ? portuguese[index]
            : IsSpanish
                ? spanish[index]
                : english[index];
    }
}
