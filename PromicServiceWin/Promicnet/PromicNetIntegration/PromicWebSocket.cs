using Functions;
using PromicnetIntegration.Events;
using PromicnetIntegration.Types;
using PromicnetWebsocket;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PromicnetWebsocket.Messages;
using static PromicnetIntegration.TypesDTO;

namespace PromicnetIntegration
{
    /// <summary>
    /// Classe com os métodos de comunicação via WebSocket com o Promicnet
    /// </summary>
    public partial class PromicWebSocket
    {
        private readonly Util utils;
        private readonly Map map;
        private readonly string session_sk;
        private readonly string orgao;
        private readonly int id_sala;
        /// <summary>
        /// Objeto de controle de uma instância do websocket.
        /// </summary>
        public readonly WebSocketSharp.WebSocket webSocket;


        /// <summary>Cosntrutor da classe PromicWebSocket.</summary>
        /// <param name="wsUrl">URL do WebSocket</param>
        /// <param name="token">Token de autenticação (Token.Access_token).</param>
        /// <param name="id_promicnet">SK da sessão.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <param name="sistema">Identificação do sistema que está criando está conexão. Por exemplo, o Promic View deve utilizar o nome "PROMIC".</param>
        /// <param name="id_sala">ID da sala que está criando está conexão.</param>
        public PromicWebSocket(string wsUrl, string token, string id_promicnet, string orgao, int id_sala, string sistema)
        {
            this.session_sk = id_promicnet;
            this.orgao = orgao;
            this.id_sala = id_sala;

            string wsUrlPromicnet = $"{wsUrl}?token={token}&session_id={id_promicnet}&client_id={orgao}";
            if (sistema != null) wsUrlPromicnet += $"&service={sistema}";

            webSocket = new WebSocketSharp.WebSocket(wsUrlPromicnet);
            utils = new Util();
            map = new Map();
        }
        /// <summary>
        /// Evento de mensagens de log.
        /// </summary>
        public event EventHandler<LogData> LogEvent;
        /// <summary>
        /// Evento disparado quando o Promic ingressar na sessão.
        /// </summary>
        public event EventHandler<ConectadoEvent> ConectadoEventHandler;
        /// <summary>
        /// Evento disparado quando o Promic ingressar na sessão.
        /// </summary>
        public event EventHandler<ParticipanteDesconectadoEvent> ParticipanteDesconectadoEventHandler;
        /// <summary>
        /// Evento disparado ao receber um ManterLogado.
        /// </summary>
        public event EventHandler<KeepAliveEvent> KeepAliveEventHandler;
        /// <summary>
        /// Evento disparado quando um vídeo for ativado.
        /// </summary>
        public event EventHandler<VideoAtivadoEvent> VideoAtivadoEventHandler;
        /// <summary>
        /// Evento disparado quando um áudio for ativado.
        /// </summary>
        public event EventHandler<MicrofoneAtivadoEvent> MicrofoneAtivadoEventHandler;
        /// <summary>
        /// Evento disparado quando um áudio for desativado.
        /// </summary>
        public event EventHandler<MicrofoneDesativadoEvent> MicrofoneDesativadoEventHandler;
        /// <summary>
        /// Evento disparado quando um participante solicitar a palavra.
        /// </summary>
        public event EventHandler<PedidoPalavraEvent> PedidoPalavraEventHandler;
        /// <summary>
        /// Evento disparado quando uma palavra solicitada for negada.
        /// </summary>
        public event EventHandler<PermissaoFalarRecusadaEvent> PermissaoFalarRecusadaEventHandler;
        /// <summary>
        /// Evento disparado quando o situacao de um microfone for alterado.
        /// </summary>
        public event EventHandler<SitaucaoMicrofoneAlteradaEvent> SitaucaoMicrofoneAlteradaEventHandler;
        /// <summary>
        /// Evento disparado quando for detectado que um participante, que está com o microfone desativado, começar a falar.
        /// </summary>
        public event EventHandler<PeakDetectEvent> PeakDetectEventHandler;
        /// <summary>
        /// Evento disparado quando o status da reunião for alterado.
        /// </summary>
        public event EventHandler<SituacaoReuniaoAlteradaEvent> SituacaoReuniaoAlteradaEventHandler;
        /// <summary>
        /// Evento disparado quando um participante ingressar na sessão.
        /// </summary>
        public event EventHandler<ParticipanteIngressouEvent> ParticipanteIngressouEventHandler;
        /// <summary>
        /// Evento disparado quando uma message não específicada for recebida.
        /// </summary>
        public event EventHandler<Event> EventHandler;
        /// <summary>
        /// Evento disparado quando uma message de erro for disparada pelo WebSocket.
        /// </summary>
        public event EventHandler<WebSocketSharp.ErrorEventArgs> ErrorEventHandler;
        /// <summary>
        /// Evento disparado quando a conexão com o WebSocket for encerrada.
        /// </summary>
        public event EventHandler<ConexaoFechadaEvent> ClosedEventHandler;
        /// <summary>
        /// Evento disparado quando a conexão do websocket for estabelecida.
        /// </summary>
        public event EventHandler<EventArgs> ConexaoAbertaEventHandler;
        /// <summary>
        /// Evento disparado quando a situação de um microfone ou câmera for alterada.
        /// </summary>
        public event EventHandler<VideoDesativadoEvent> VideoDesativadoEventHandler;
        /// <summary>
        /// Evento disparado quando um participante conectado ao Promicnet registrar um voto.
        /// </summary>
        public event EventHandler<ParticipanteVotouEvent> ParticipanteVotouEventHandler;
        /// <summary>
        /// Evento disparado quando uma votação for finalizada para os participantes conectados ao Promicnet.
        /// </summary>
        public event EventHandler<VotacaoFinalizadaEvent> VotacaoFinalizadaEventHandler;
        /// <summary>
        /// Evento disparado quando uma votação for iniciada para os participantes conectados ao Promicnet.
        /// </summary>
        public event EventHandler<VotacaoIniciadaEvent> VotacaoIniciadaEventHandler;
        /// <summary>
        /// Evento disparado quando uma votação for cancelada para os participantes conectados ao Promicnet.
        /// </summary>
        public event EventHandler<VotacaoCanceladaEvent> VotacaoCanceladaEventHandler;
        /// <summary>
        /// Evento disparado quando um participante conectado ao Promicnet se inscrever como orador.
        /// </summary>
        public event EventHandler<OradorInscritoEvent> OradorInscritoEventHandler;
        /// <summary>
        /// Evento disparado quando um participante conectado ao Promicnet se desinscreve como orador.
        /// </summary>
        public event EventHandler<OradorDesinscritoEvent> OradorDesincritoEventHandler;
        /// <summary>
        /// Evento disparado quando um participante remoto finaliza o seu tempo como orador.
        /// </summary>
        public event EventHandler<TempoOradorEncerradoEvent> TempoOradorEncerradoEventHandler;

        #region LOG
        /// <summary>Responsável por disparar o evento de message</summary>
        /// <param name="function">Nome da função que está disparando o evento</param>
        /// <param name="message">Mensagem a ser disparada</param>
        /// <param name="status">Mensagem a ser disparada</param>
        public virtual void Log(string status, string function, string message) => LogEvent?.Invoke(this, new LogData(@class: "WebSocket", status: status, function: function, message: message));
        #endregion
        #region FUNÇÕES
        /// <summary>Estabelece uma conexão com o WebSocket do Promicnet.</summary>
        /// <returns>Retorna um objeto  Result contendo as informações da conexão com o servidor dentro do parametro ` Result.Item`.</returns>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public HReturn<bool> Conectar()
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                Log("1", "Conectar", $"Conectando à sessão {session_sk} do órgão {orgao}.");
                webSocket.OnMessage += OnMessage;
                webSocket.OnClose += OnClose;
                webSocket.OnError += OnError;
                webSocket.OnOpen += OnOpen;

                webSocket.Connect();

                Log("200", "Conectar", "Conectando com sucesso!");
            }
            catch (Exception e)
            {
                Log("502", "Conectar", e.Message);
            }
            return result;
        }
        /// <summary>Encerra a conexão com o WebSocket do Promicnet.</summary>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public HReturn<bool> Desconectar()
        {
            Log("1", "Desconectar", $"Desconectando da sessão {session_sk} do órgão {orgao}.");
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                webSocket.CloseAsync();
                result.MENSAGEM = $"Desconectado com sucesso!";
                result.SUCESSO = true;
                result.OBJETO = true;
                Log("200", "Desconectar", $"Desconectado com sucesso!");
            }
            catch (Exception e)
            {
                result.MENSAGEM = e.Message;
                result.SUCESSO = false;
                result.OBJETO = false;
                Log("502", "Desconectar", e.Message);
            }
            return result;
        }
        /// <summary>Reseta o tempo da conexão com o WebSocket do Promicnet. <br/>
        /// As conexões com o WebSocket duram 10 minutos, para que você não seja desconectado automaticamente, poderá chamar este método a qualquer momento para reiniciar a contagem deste tempo.</summary>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public HReturn<bool> ManterLogado()
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                Message message = new Message("keep_alive");
                string mensagem = JsonSerializer.Serialize(message);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "ManterLogado", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    ManterLogado();
                }

            }
            catch (Exception e)
            {
                Log("502", "ManterLogado", e.Message);
            }
            return result;
        }
        /// <summary>Ativa o vídeo de um participante da sessão.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public HReturn<bool> ChavearVideo(string email)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "ChavearVideo", "Email inválido!");
            }
            Log("1", "ChavearVideo", $"Ligando o vídeo do usuário {email}");
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                ChaveaVideo chavearVideo = new ChaveaVideo(session_id: session_sk, user_email: email);
                string mensagem = JsonSerializer.Serialize(chavearVideo);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "ChavearVideo", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    ChavearVideo(email);
                }
            }
            catch (Exception e)
            {
                Log("502", "ChavearVideo", e.Message);
            }
            return result;
        }
        /// <summary>Ativa o vídeo de um participante da sessão.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public HReturn<bool> DesativarCamera(string email)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "DesativarCamera", "Email inválido!");
            }

            Log("1", "DesativarCamera", $"Desativando a câmera do participante {email}");
            var result = new HReturn<bool>();
            try
            {
                // ALTERAR
                webSocket.Send(@"{ ""action"":""change_media_state"", ""state"":""OFF"", ""media_type"":""video"", ""session_id"":""" + session_sk + @""", ""user_email"":""" + email + @""" }");
            }
            catch (InvalidOperationException e)
            {
                Log("500", "DesativarCamera", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    DesativarCamera(email);
                }
            }
            catch (Exception e)
            {
                Log("502", "DesativarCamera", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Método para chaver no vídeo de um participante
        /// </summary>
        /// <param name="email"></param>
        public HReturn<bool> AtivarCamera(string email)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "AtivarCamera", "Email inválido!");
            }

            Log("1", "AtiHReturn<bool>Camera", $"Ativando a câmeta do participante {email}");
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                // ALTERAR
                webSocket.Send(@"{ ""action"":""change_media_state"", ""state"":""ON"", ""media_type"":""video"", ""session_id"":""" + session_sk + @""", ""user_email"":""" + email + @""" }");
            }
            catch (InvalidOperationException e)
            {
                Log("500", "AtivarCamera", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    AtivarCamera(email);
                }
            }
            catch (Exception e)
            {
                Log("502", "AtivarCamera", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Método para ativar o preview do vídeo de um participante
        /// </summary>
        /// <param name="email"></param>
        public HReturn<bool> AtivarPreview(string email)
        {
            var result = new HReturn<bool>();
            return result;
        }
        /// <summary>
        /// Método para desativar o preview do vídeo de um participante
        /// </summary>
        /// <param name="email"></param>
        public HReturn<string> DesativarPreview(string email)
        {
            var result = new HReturn<string>();
            return result;
        }
        /// <summary>Ativa o microfone de um participante da sessão.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public HReturn<bool> AtivarMicrofone(string email)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "AtivarMicrofone", "Email inválido!");
            }

            Log("1", "AtivarMicrofone", $"Ligando o microfone do {email} da sessão {session_sk}");
            var result = new HReturn<bool>();
            try
            {
                // ALTERAR
                webSocket.Send(@"{""action"": ""add_active_audio"", ""session_id"": """ + session_sk + @""", ""user_email"": """ + email + @"""}");
            }
            catch (InvalidOperationException e)
            {
                Log("500", "AtivarMicrofone", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    AtivarMicrofone(email);
                }
            }
            catch (Exception e)
            {
                Log("502", "AtivarMicrofone", e.Message);
            }
            return result;
        }
        /// <summary>Desativa o microfone de um pariticpante da sessão.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public HReturn<bool> DesativarMicrofone(string email)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "DesativarMicrofone", "Email inválido!");
            }

            Log("1", "DesativarMicrofone", $"Ligando o vídeo do usuário {email}");
            var result = new HReturn<bool>();
            try
            {
                // ALTERAR
                webSocket.Send(@"{""action"": ""remove_active_audio"",""session_id"": """ + session_sk + @""",""user_email"": """ + email + @"""}");
            }
            catch (InvalidOperationException e)
            {
                Log("500", "DesativarMicrofone", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    DesativarMicrofone(email);
                }
            }
            catch (Exception e)
            {
                Log("502", "DesativarMicrofone", e.Message);
            }
            return result;
        }
        /// <summary>Solicita a palavra para um participante.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public HReturn<bool> SolicitarPalavra(string email)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "SolicitarPalavra", "Email inválido!");
            }

            Log("1", "SolicitarPalavra", $"Solicitando palavra do usuário {email}");

            var result = new HReturn<bool>();
            try
            {
                // ALTERAR
            }
            catch (InvalidOperationException e)
            {
                Log("500", "SolicitarPalavra", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    SolicitarPalavra(email);
                }
            }
            catch (Exception e)
            {
                Log("502", "SolicitarPalavra", e.Message);
            }
            return result;
        }
        /// <summary>Recusa a solicitação de palavra de um participante.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public HReturn<bool> RecusarPedidoPalavra(string email)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "RecusarPedidoPalavra", "Email inválido!");
            }

            Log("1", "RecusarPedidoPalavra", $"Cancelando solicitação da palavra do usuário {email}");

            var result = new HReturn<bool>();
            try
            {
                // ALTERAR
                webSocket.Send(@"{""action"": ""system_cancel_ask_to_speak"",""session_id"": """ + session_sk + @""",""user_email"": """ + email + @"""}");
            }
            catch (InvalidOperationException e)
            {
                Log("500", "RecusarPedidoPalavra", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    RecusarPedidoPalavra(email);
                }
            }
            catch (Exception e)
            {
                Log("502", "RecusarPedidoPalavra", e.Message);
            }
            return result;
        }
        /// <summary>Altera o situacao do microfone de um participante.</summary>
        /// <param name="email">User_email do participante</param>
        /// <param name="state">Estado do microfone</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public HReturn<bool> AlterarEstadoMicrofone(string email, string state)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "AlterarEstadoMicrofone", "Email inválido!");
            }

            if (string.IsNullOrEmpty(state))
            {
                Log("400", "AlterarEstadoMicrofone", "Estado do microfone inválido!");
            }
            Log("1", "AlterarEstadoMicrofone", $"Alterando o estado do microfone do {email} da sessão {session_sk} para {state}");

            var result = new HReturn<bool>();
            try
            {
                // ALTERAR
                webSocket.Send($@"{{""action"": ""change_mic_state"",""session_id"": ""{session_sk}"",""user_email"": ""{email}"",""state"":""{state}""}}");
            }
            catch (InvalidOperationException e)
            {
                Log("500", "AlterarEstadoMicrofone", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    AlterarEstadoMicrofone(email, state);
                }
            }
            catch (Exception e)
            {
                Log("502", "AlterarEstadoMicrofone", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Inícia uma votação para os participantes conectados ao Promicnet.
        /// </summary>
        /// <param name="id_votacao">ID da votação no Promic.</param>
        /// <param name="titulo">Título da votação.</param>
        /// <param name="ementa">Ementa da votação.</param>
        /// <param name="votacao">Tipo de votos possíveis para essa votação.</param>
        /// <param name="tempoVotacao">Tempo total para votar, em segundos. Caso a votação não tenha um tempo estimado, passar o valor 0.</param>
        /// <param name="id_delegado">ID do Delegado no Promic.</param>
        /// <exception cref="ArgumentException"></exception>
        public HReturn<bool> IniciarVotacao(int id_votacao, string titulo, string ementa, TipoVotacao votacao, int tempoVotacao, int id_delegado)
        {
            if (id_votacao == 0)
            {
                Log("400", "IniciarVotacao", "ID da Votação não pode ser vazio.");
            }
            if (string.IsNullOrEmpty(titulo))
            {
                titulo = String.Empty;
            }
            var result = new HReturn<bool>();
            try
            {
                // ALTERAR
                InicioVotacao obj = new InicioVotacao
                {
                    action = "initiate_vote",
                    vote_id = id_votacao,
                    title = titulo,
                    description = ementa,
                    types_allowed = votacao.GetString(),
                    session_id = session_sk,
                    vote_time = tempoVotacao,
                    user_id = id_delegado
                };
                string mensagem = JsonSerializer.Serialize(obj);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "IniciarVotacao", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    IniciarVotacao(id_votacao, titulo, ementa, votacao, tempoVotacao, id_delegado);
                }
            }
            catch (Exception e)
            {
                Log("502", "IniciarVotacao", e.Message);
            }
            return result;
        }
        /// <summary>
        /// atualiza a lista de votos para todos os participantes conectado no Promicnet
        /// </summary>
        /// <param name="votos"></param>
        public HReturn<bool> AtualizarListaVotos(System.Collections.Generic.List<VOTO> votos)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                ListaVotacao listaVotacao = new ListaVotacao(session_sk, map.ToVotoDtoList(votos));
                string message = JsonSerializer.Serialize(listaVotacao);
                webSocket.Send(message);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "AtualizarListaVotos", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    AtualizarListaVotos(votos);
                }
            }
            catch (Exception e)
            {
                Log("502", "AtualizarListaVotos", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Encerra uma votação e exibe o resultado para todos os participantes conectados ao Promicnet.
        /// </summary>
        /// <param name="id_votacao">ID da votação no Promic.</param>
        /// <param name="titulo">Título da votação.</param>
        /// <param name="ementa">Ementa da votação.</param>
        /// <param name="votacao">Tipo de votos possíveis para essa votação.</param>
        /// <param name="resultado">Resultado da votação.</param>
        /// <param name="votos">Lista dos votos </param>
        /// <param name="resultado_votacao">Resultado da votação.</param>
        public HReturn<bool> EncerrarVotacao(int id_votacao, string titulo, string ementa, TipoVotacao votacao, bool resultado, int[] resultado_votacao, List<VOTO> votos)
        {
            if (id_votacao == 0)
            {
                Log("400", "IniciarVotacao", "ID da Votação não pode ser vazio.");
            }
            if (titulo is null)
            {
                titulo = String.Empty;
            }

            var result = new HReturn<bool>();
            try
            {
                string resVotacao = string.Empty;
                if (resultado)
                {
                    resVotacao = "APPROVED";
                }
                else
                {
                    resVotacao = "REPROVED";
                }

                List<VotoDTO> votosDTO = map.ToVotoDtoList(votos);

                // ALTERAR
                ResultadoVotacao obj = new ResultadoVotacao
                {
                    action = "vote_results",
                    result = resVotacao,
                    title = titulo,
                    description = ementa,
                    vote_id = id_votacao,
                    types_allowed = votacao.GetString(),
                    session_id = session_sk,
                    vote_count = resultado_votacao,
                    votes = votosDTO
                };
                string mensagem = JsonSerializer.Serialize(obj);

                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "IniciarVotacao", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    EncerrarVotacao(id_votacao, titulo, ementa, votacao, resultado, resultado_votacao, votos);
                }
            }
            catch (Exception e)
            {
                Log("502", "IniciarVotacao", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Cancela uma votação para todos os participantes conectados ao Promicnet.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public HReturn<bool> CancelarVotacao(int id_votacao)
        {
            var result = new HReturn<bool>();
            try
            {
                // OK
                CancelaVotacao cancelaVotacao = new CancelaVotacao(id_votacao, session_sk);
                string mensagem = JsonSerializer.Serialize(cancelaVotacao);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "CancelarVotacao", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    CancelarVotacao(id_votacao);
                }
            }
            catch (Exception e)
            {
                Log("502", "CancelarVotacao", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Dá inicio a uma parte para os usuários do Promicnet.
        /// </summary>
        /// <param name="id_parte">ID da parte.</param>
        /// <param name="speaker">ID da parte.</param>
        public HReturn<bool> IniciarParte(int id_parte, bool speaker)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                InicioParte inicioParte = new InicioParte(id_parte, "", speaker, session_sk);
                string mensagem = JsonSerializer.Serialize(inicioParte);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "IniciarParte", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    IniciarParte(id_parte, speaker);
                }
            }
            catch (Exception e)
            {
                Log("502", "IniciarParte", e.Message);
            }

            return result;
        }
        /// <summary>
        /// Encerra a parte que está ativa para os usuários do Promicnet.
        /// </summary>
        public HReturn<bool> EncerraParte()
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                // ALTERAR
                FimParte fimParte = new FimParte
                {
                    action = "end_part",
                    session_id = session_sk
                };
                string mensagem = JsonSerializer.Serialize(fimParte);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "EncerraParte", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    EncerraParte();

                }
            }
            catch (Exception e)
            {
                Log("502", "EncerraParte", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Dá início a discussão de uma matéria
        /// </summary>
        /// <param name="id_materia"></param>
        public HReturn<bool> IniciarDiscussao(int id_materia)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                // ALTERAR
                InicioDiscussao inicioDiscussao = new InicioDiscussao
                {
                    action = "initiate_subject",
                    session_id = session_sk,
                    id = id_materia
                };
                string mensagem = JsonSerializer.Serialize(inicioDiscussao);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "IniciarDiscussao", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    IniciarDiscussao(id_materia);
                }
            }
            catch (Exception e)
            {
                Log("502", "IniciarDiscussao", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Encerra a discussão da matéria atual para os usuários do Promicnet.
        /// </summary>
        public HReturn<bool> EncerrarDiscussao()
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                // ALTERAR
                EncerraDiscussao encerraDiscussao = new EncerraDiscussao(session_sk);
                string mensagem = JsonSerializer.Serialize(encerraDiscussao);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "EncerrarDiscussao", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    EncerrarDiscussao();
                }
            }
            catch (Exception e)
            {
                Log("502", "EncerrarDiscussao", e.Message);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="partes"></param>
        public HReturn<bool> EnviarPauta(List<ParteDTO> partes)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                // ALTERAR
                PautaDTO pauta = new PautaDTO
                {
                    action = "update_session_agenda",
                    session_id = session_sk,
                    parts = partes
                };
                string mensagem = JsonSerializer.Serialize(pauta);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "EnviarPauta", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    EnviarPauta(partes);
                }
            }
            catch (Exception e)
            {
                Log("502", "EnviarPauta", e.Message);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pause"></param>
        public HReturn<bool> PausarVotacao(bool pause)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                // ALTERAR
                PausaVotacao pausaVotacao = new PausaVotacao
                {
                    action = "pause_vote",
                    session_id = session_sk,
                    pause = pause
                };
                string mensagem = JsonSerializer.Serialize(pausaVotacao);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "PausarVotacao", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    PausarVotacao(pause);
                }
            }
            catch (Exception e)
            {
                Log("502", "PausarVotacao", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Cadastra um novo orador para uma sessão
        /// </summary>
        /// <param name="login"></param>
        /// <exception cref="ArgumentException"></exception>
        public HReturn<bool> CadastrarOrador(string login)
        {
            HReturn<bool> result = new HReturn<bool>();
            if (string.IsNullOrEmpty(login))
            {
                Log("400", "CadastrarOrador", "O  não pode ser vazio.");
            }
            try
            {
                // ALTERAR
                CadastraOrador cadastraOrador = new CadastraOrador
                {
                    action = "speaker_subscribe",
                    session_id = session_sk,
                    login = login
                };
                string mensagem = JsonSerializer.Serialize(cadastraOrador);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "CadastrarOrador", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    CadastrarOrador(login);
                }
            }
            catch (Exception e)
            {
                Log("502", "CadastrarOrador", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Inícia o tempo de um orador
        /// </summary>
        /// <param name="user_id">ID do Delegado.</param>
        /// <param name="tempo">Tempo de fala do Delegado.</param>
        /// <exception cref="ArgumentException"></exception>
        public HReturn<bool> IniciarTempoOrador(int user_id, int tempo)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                // ALTERAR
                InicioTempoOrador inicioTempoOrador = new InicioTempoOrador
                {
                    action = "initiate_speaker_time",
                    session_id = session_sk,
                    time = tempo,
                    user_id = user_id,
                    turn_off_mic = false,
                    turn_on_mic = true
                };
                string mensagem = JsonSerializer.Serialize(inicioTempoOrador);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "IniciarTempoOrador", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    IniciarTempoOrador(user_id, tempo);
                }
            }
            catch (Exception e)
            {
                Log("502", "IniciarTempoOrador", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Encerra o tempo de um Orador
        /// </summary>
        /// <param name="user_id">ID do Delegado.</param>
        public HReturn<bool> FinalizarTempoORador(int user_id)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                // ALTERAR
                FimTempoOrador fimTempoOrador = new FimTempoOrador
                {
                    action = "end_speaker_time",
                    session_id = session_sk,
                    user_id = user_id,
                };
                string mensagem = JsonSerializer.Serialize(fimTempoOrador);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "FinalizarTempoORador", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    FinalizarTempoORador(user_id);
                }
            }
            catch (Exception e)
            {
                Log("502", "FinalizarTempoORador", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Pausa/despausa o tempo de um Orador
        /// </summary>
        /// <param name="pause">Enviar 1 para pausar o tempo, e 0 para retomar.</param>
        public HReturn<bool> PausarTempoOrador(int pause)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                // ALTERAR
                PausaTempoOrador pausaTempoOrador = new PausaTempoOrador
                {
                    action = "pause_speaker_time",
                    session_id = session_sk,
                    pause = pause == 1
                };
                string mensagem = JsonSerializer.Serialize(pausaTempoOrador);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "PausarTempoOrador", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    PausarTempoOrador(pause);
                }
            }
            catch (Exception e)
            {
                Log("502", "PausarTempoOrador", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Altera o tempo de um Orador
        /// </summary>
        /// <param name="user_id">ID do Delegado</param>
        /// <param name="tempo">Tempo do Orador</param>
        public HReturn<bool> AtualizarTempoOrador(int user_id, int tempo)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                AtualizaTempoOrador atualizaTempoOrador = new AtualizaTempoOrador(user_id, tempo, session_sk);
                string mensagem = JsonSerializer.Serialize(atualizaTempoOrador);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "AtualizarTempoOrador", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    AtualizarTempoOrador(user_id, tempo);
                }
            }
            catch (Exception e)
            {
                Log("502", "AtualizarTempoOrador", e.Message);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oradores"></param>
        /// <returns></returns>
        public HReturn<bool> EnviaListaOradores(List<Orador> oradores)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                ListaOradores listaOradores = new ListaOradores(session_sk, oradores);
                string mensagem = JsonSerializer.Serialize(listaOradores);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "EnviaListaOradores", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    EnviaListaOradores(oradores);
                }
            }
            catch (Exception e)
            {
                Log("502", "EnviaListaOradores", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Envia a lista de presença para todos os participantes conectados no Promicnet.
        /// </summary>
        /// <param name="presenca">Lista com todos os participantes, indicando se ele está ausente ou presente na reunião.</param>
        /// <returns></returns>
        public HReturn<bool> EnviaListaPresenca(List<Presence> presenca)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                ListaPresenca listaPresenca = new ListaPresenca(session_sk, presenca);
                string mensagem = JsonSerializer.Serialize(listaPresenca);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "EnviaListaPresenca", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    EnviaListaPresenca(presenca);
                }
            }
            catch (Exception e)
            {
                Log("502", "EnviaListaPresenca", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Da início ao tempo para inscrições de oradores.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public HReturn<bool> IniciaTempoInscricao(int time)
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                TempoIncricao tempoIncricao = new TempoIncricao(session_sk, time);
                string mensagem = JsonSerializer.Serialize(tempoIncricao);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "IniciaTempoInscricao", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    IniciaTempoInscricao(time);
                }
            }
            catch (Exception e)
            {
                result.OBJETO = false;
                result.SUCESSO = false;
                result.MENSAGEM = e.Message;
                Log("502", "IniciaTempoInscricao", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Da início ao tempo para inscrições de oradores.
        /// </summary>
        public HReturn<bool> FimTempoInscricao()
        {
            HReturn<bool> result = new HReturn<bool>();
            try
            {
                Message message = new Message("end_speaker_subscribe_time", session_sk);
                string mensagem = JsonSerializer.Serialize(message);
                webSocket.Send(mensagem);
            }
            catch (InvalidOperationException e)
            {
                Log("500", "FimTempoInscricao", e.Message);
                if (webSocket.ReadyState == WebSocketSharp.WebSocketState.Closed)
                {
                    Conectar();
                    FimTempoInscricao();
                }
            }
            catch (Exception e)
            {
                result.OBJETO = false;
                result.SUCESSO = false;
                result.MENSAGEM = e.Message;
                Log("502", "FimTempoInscricao", e.Message);
            }
            return result;
        }
        /// <summary>
        /// Callback de retorno ao se conectar no WebSocket.
        /// </summary>
        public void OnOpen(object sender, EventArgs e)
        {
            ConexaoAbertaEventHandler?.Invoke(sender, e);
        }
        /// <summary>
        /// Callback de retorno ao ocorrer um erro no WebScoket.
        /// </summary>
        private void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            ErrorEventHandler?.Invoke(sender, e);
        }
        /// <summary>
        /// Callback de retorno ao fechar uma conexão com o WebSocket.
        /// </summary>
        private void OnClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            ConexaoFechadaEvent conexaoFechada = new ConexaoFechadaEvent(e.Code, e.Reason, id_sala);
            ClosedEventHandler?.Invoke(sender, conexaoFechada);
        }
        /// <summary>
        /// Callback de retorno com a trativa de todas as mensagens do WebSocket.
        /// </summary>
        private void OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {

            if (e.Data.Contains("keep_alive")) { return; }

            Event evento = JsonSerializer.Deserialize<Event>(e.Data.Trim(), new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, PropertyNameCaseInsensitive = true });

            switch (evento.Action)
            {
                case "connected_successfully":
                    {
                        ConectadoEvent connectedSuccessfully = new ConectadoEvent(evento.Action, evento.Connection_id, id_sala);
                        ConectadoEventHandler?.Invoke(ConectadoEventHandler, connectedSuccessfully);
                        break;
                    }
                case "disconnect":
                    {
                        ParticipanteDesconectadoEvent disconnectedSuccessfully = new ParticipanteDesconectadoEvent(evento.Action, evento.Agora_id, evento.User_email, id_sala);
                        ParticipanteDesconectadoEventHandler?.Invoke(ParticipanteDesconectadoEventHandler, disconnectedSuccessfully);
                        break;
                    }
                case "keep_alive":
                    {
                        KeepAliveEvent keepAlive = new KeepAliveEvent(evento.Action, id_sala);
                        KeepAliveEventHandler?.Invoke(KeepAliveEventHandler, keepAlive);
                        break;
                    }
                case "select_active_video":
                    {
                        VideoAtivadoEvent selectActiveVideo = new VideoAtivadoEvent(evento.Action, evento.User_email, id_sala);
                        VideoAtivadoEventHandler?.Invoke(VideoAtivadoEventHandler, selectActiveVideo);
                        break;
                    }
                case "add_active_audio":
                    {
                        MicrofoneAtivadoEvent addActiveAudio = new MicrofoneAtivadoEvent(evento.Action, evento.User_email, id_sala);
                        MicrofoneAtivadoEventHandler?.Invoke(MicrofoneAtivadoEventHandler, addActiveAudio);
                        break;
                    }
                case "ask_to_speak":
                    {
                        PedidoPalavraEvent askToSpeak = new PedidoPalavraEvent(evento.Action, evento.Session_id, evento.User_email, id_sala);
                        PedidoPalavraEventHandler?.Invoke(PedidoPalavraEventHandler, askToSpeak);
                        break;
                    }
                case "user_joined_session":
                    {

                        if (evento.User_email != "PROMIC")
                        {
                            ParticipanteIngressouEvent userJoinedSessionEvent = new ParticipanteIngressouEvent(evento.Action, evento.Session_id, evento.User_email, evento.Agora_id, id_sala);
                            ParticipanteIngressouEventHandler?.Invoke(ParticipanteIngressouEventHandler, userJoinedSessionEvent);
                        }
                        break;
                    }
                case "cancel_ask_to_speak":
                    {
                        PermissaoFalarRecusadaEvent cancelAskToSpeak = new PermissaoFalarRecusadaEvent(evento.Action, evento.Session_id, evento.User_email, id_sala);
                        PermissaoFalarRecusadaEventHandler?.Invoke(PermissaoFalarRecusadaEventHandler, cancelAskToSpeak);
                        break;
                    }
                case "change_mic_state":
                    {
                        SitaucaoMicrofoneAlteradaEvent changeMicState = new SitaucaoMicrofoneAlteradaEvent(evento.Action, evento.State, id_sala);
                        SitaucaoMicrofoneAlteradaEventHandler?.Invoke(SitaucaoMicrofoneAlteradaEventHandler, changeMicState);
                        break;
                    }
                case "session_status_changed":
                    {
                        SituacaoReuniaoAlteradaEvent sessionStatusChanged = new SituacaoReuniaoAlteradaEvent(evento.Action, id_sala);
                        SituacaoReuniaoAlteradaEventHandler?.Invoke(SituacaoReuniaoAlteradaEventHandler, sessionStatusChanged);
                        break;
                    }
                case "peak_detect":
                    {
                        PeakDetectEvent peakDetect = new PeakDetectEvent(evento.Action, evento.Session_id, evento.User_email, evento.State, id_sala);
                        PeakDetectEventHandler?.Invoke(PeakDetectEventHandler, peakDetect);
                        break;
                    }
                case "media_changed_state":
                    {
                        if (evento.Media_type == "audio" && evento.State == "OFF")
                        {
                            MicrofoneDesativadoEvent microfoneDesativado = new MicrofoneDesativadoEvent(evento.Action, evento.User_email, id_sala);
                            MicrofoneDesativadoEventHandler?.Invoke(MicrofoneDesativadoEventHandler, microfoneDesativado);
                        }
                        else if (evento.Media_type == "audio" && evento.State == "ON")
                        {
                            MicrofoneAtivadoEvent microfoneAtivado = new MicrofoneAtivadoEvent(evento.Action, evento.User_email, id_sala);
                            MicrofoneAtivadoEventHandler?.Invoke(MicrofoneAtivadoEventHandler, microfoneAtivado);
                        }
                        else if (evento.Media_type == "video" && evento.State == "OFF")
                        {
                            VideoDesativadoEvent videoDesativado = new VideoDesativadoEvent(evento.Action, evento.User_email, id_sala);
                            VideoDesativadoEventHandler?.Invoke(VideoDesativadoEventHandler, videoDesativado);
                        }
                        else if (evento.Media_type == "video" && evento.State == "ON")
                        {
                            VideoAtivadoEvent videoAtivado = new VideoAtivadoEvent(evento.Action, evento.User_email, id_sala);
                            VideoAtivadoEventHandler?.Invoke(VideoAtivadoEventHandler, videoAtivado);
                        }
                        break;
                    }
                case "initiate_vote":
                    {
                        VotacaoIniciadaEvent votacaoEvent = new VotacaoIniciadaEvent(evento.Action, evento.Vote_id, evento.Title, evento.Description, evento.Status, evento.Types_allowed, evento.Vote_time, id_sala);
                        VotacaoIniciadaEventHandler?.Invoke(sender, votacaoEvent);
                        break;
                    }
                case "user_vote":
                    {
                        ParticipanteVotouEvent participanteVotou = new ParticipanteVotouEvent(evento.Action, evento.Vote_id, evento.Session_id, evento.Vote, evento.User_email, evento.User_id, id_sala);
                        ParticipanteVotouEventHandler?.Invoke(sender, participanteVotou);
                        break;
                    }
                case "vote_results":
                    {
                        VotacaoFinalizadaEvent votacaoFinalizada = new VotacaoFinalizadaEvent(evento.Action, evento.Result, evento.Vote_id, evento.Title, evento.Session_id, evento.Types_allowed, evento.Votes, id_sala);
                        VotacaoFinalizadaEventHandler?.Invoke(sender, votacaoFinalizada);
                        break;
                    }
                case "cancel_vote":
                    {
                        VotacaoCanceladaEvent votacaoCancelada = new VotacaoCanceladaEvent(evento.Action, evento.Session_id, evento.Vote_id, id_sala);
                        VotacaoCanceladaEventHandler?.Invoke(sender, votacaoCancelada);
                        break;
                    }
                case "speaker_subscribe":
                    {
                        OradorInscritoEvent oradorInscrito = new OradorInscritoEvent(evento.Action, evento.Session_id, evento.User_email, id_sala);
                        OradorInscritoEventHandler?.Invoke(sender, oradorInscrito);
                        break;
                    }
                case "speaker_unsubscribe":
                    {
                        OradorDesinscritoEvent oradorDesinscrito = new OradorDesinscritoEvent(evento.Action, evento.Session_id, evento.User_email, id_sala);
                        OradorDesincritoEventHandler?.Invoke(this, oradorDesinscrito);
                        break;
                    }
                case "end_speaker_time":
                    {
                        TempoOradorEncerradoEvent tempoOradorEncerrado = new TempoOradorEncerradoEvent(evento.Action, evento.Session_id, evento.User_id, id_sala);
                        TempoOradorEncerradoEventHandler?.Invoke(this, tempoOradorEncerrado);
                        break;
                    }
                default:
                    {
                        EventHandler?.Invoke(sender, evento);
                        break;
                    }

            }
            #endregion
        }
    }
}
