using LibPromicDB;
using LibPromicDB.Entidades;
using Microsoft.Extensions.Logging;
using PromicnetIntegration;
using PromicnetIntegration.Types;
using PromicnetWebsocket;
using PromicnetWebsocket.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static LibPromicDB.PromicDB;
using static PromicnetIntegration.PromicWebSocket;
using static PromicnetIntegration.TypesDTO;

namespace PromicService
{
    public class PromicService : ServiceBase
    {
        public PromicDB PromicDB { get; set; }
        PromicserviceNet promicserviceNet;
        private readonly Socket udp_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        readonly System.Diagnostics.EventLog winLog;
        private string ip = "local";
        private int porta = 3050;

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        public PromicService()
        {
            try
            {
                if (!System.Diagnostics.EventLog.SourceExists("PromicService"))
                {
                    System.Diagnostics.EventLog.CreateEventSource(
                        "PromicService", "Application");
                }

                winLog = new System.Diagnostics.EventLog
                {
                    Source = "PromicService",
                    Log = "Application"
                };

                udp_socket.Connect(IPAddress.Parse("127.0.0.1"), 47159);
            }
            catch (Exception exception)
            {
                Log($"[Falha do conectar no logger de mensagem.] [Motivo: {exception.Message}]");
            }
        }

        public void Log(string mensagem)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now}] " + mensagem);
                byte[] data = Encoding.UTF8.GetBytes(mensagem);
                udp_socket.BeginSend(data, 0, data.Length, SocketFlags.None, null, null);

                winLog.WriteEntry(mensagem, System.Diagnostics.EventLogEntryType.Information);

            }
            catch (Exception exception)
            {
                Console.WriteLine($"[{DateTime.Now}] " + mensagem);
                byte[] data = Encoding.UTF8.GetBytes(exception.Message);
                if (!udp_socket.Connected)
                {
                    udp_socket.Connect(IPAddress.Parse("127.0.0.1"), 47159);
                }

                winLog.WriteEntry(mensagem, System.Diagnostics.EventLogEntryType.Error);

                udp_socket.BeginSend(data, 0, data.Length, SocketFlags.None, null, null);

                string caminho = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\erro.txt";
                if (File.Exists(caminho))
                {
                    StreamWriter sw = new StreamWriter(caminho);
                    sw.Write(exception.Message);
                    sw.Close();
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ServiceStatus serviceStatus = new ServiceStatus
                {
                    dwCurrentState = ServiceState.SERVICE_START_PENDING,
                    dwWaitHint = 100000
                };
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                Start();

                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception exception)
            {
                Log($"[SER] [Falha ao iniciar serviço.] [Motivo: {exception.Message}]");

                string caminho = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\erro.txt";
                if (File.Exists(caminho))
                {
                    StreamWriter sw = new StreamWriter(caminho);
                    sw.Write(exception.Message);
                    sw.Close();
                }
            }
        }

        protected override void OnStop()
        {
            try
            {

                Log("[SER] [Parando o serviço.]");
                // Update the service state to Stop Pending.
                ServiceStatus serviceStatus = new ServiceStatus
                {
                    dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                    dwWaitHint = 100000
                };
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                foreach (ControlesLista item in PromicDB.ListaControle)
                {
                    promicserviceNet.DesconectarWebSocket(item.ID_SALA);
                }
                // Update the service state to Stopped.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception exception)
            {
                Log($"[SER] [Erro ao fechar serviço.] [Motivo: {exception.Message}]");
                string caminho = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\erro.txt";
                if (File.Exists(caminho))
                {
                    StreamWriter sw = new StreamWriter(caminho);
                    sw.Write(exception.Message);
                    sw.Close();
                }
            }
        }

        protected override void OnContinue()
        {
            Log("[SER] [In OnContinue.]");
        }

        public void Start()
        {
            try
            {
                string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Riole\\config.json";
                if (File.Exists(path))
                {
                    StreamReader sr = new StreamReader(path);
                    string conteudo = sr.ReadToEnd();
                    sr.Close();
                    sr.Dispose();
                    Config _config = JsonSerializer.Deserialize<Config>(conteudo);
                    Log($"[SER] [Banco de dados: {_config.IpDB}:{_config.Porta}.]");
                    ip = _config.IpDB;
                    porta = _config.Porta;
                }
                else
                {
                    Log($"[SER] [Arquivo de configuração não encontrado! Adicione o arquivo de configuração na pasta: \"{path}\" e reinicie o serviço ]");
                }

                PromicDB = new PromicDB(ip.Equals("local"), ip, porta);
                PromicDB.OnLog += OnLogDB;
                PromicDB.OnEstadoControles += OnEstadoControlesDB;
                PromicDB.OnListaVotacaoGeral += OnListaVotacaoGeralDB;
                PromicDB.OnListaVotoNominal += OnListaVotosNominaisDB;

                PromicDB.OnPausaCronometroSet += OnPausaCronometroSet;
                PromicDB.OnTempoCronometroSet += OnTempoCronometroSet;
                PromicDB.OnPauta += EnviaPauta;
                PromicDB.OnListaPresenca += EnviaListaPresenca;
                PromicDB.OnListaOradores += EnviaListaOradores;

                PromicDB.IniciarLoop();

                promicserviceNet = new PromicserviceNet(this);

                promicserviceNet.PromicNetGetToken();

                Log($"[SER] [Serviço iniciado com sucesso.] [Versão: {Assembly.GetExecutingAssembly().GetName().Version}]");
            }
            catch (Exception e)
            {
                string caminho = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\erro.txt";
                if (File.Exists(caminho))
                {
                    StreamWriter sw = new StreamWriter(caminho);
                    sw.Write(e.Message);
                    sw.Close();
                }
            }
        }

        private void EnviaListaOradores(int index, int id_sala, List<Oradores> lista)
        {
            int id_reuniao = PromicDB.ListaControle[index].ID_REUNIAO;

            if (PromicDB.ListaControle[index].WEBSOCKET == null)
            {
                promicserviceNet.ConectarWebSocket(id_reuniao, id_sala);
            }

            PromicWebSocket PromicNetWebsocket = PromicDB.ListaControle[index].WEBSOCKET;

            List<Orador> orador_ = new List<Orador>();

            foreach (Oradores i in lista)
            {
                orador_.Add(new Orador
                {
                    user_id = i.ID_DELEGADO,
                    name = i.NOME,
                    party = i.PARTIDO,
                    time = i.TEMPO
                });
            }
            PromicNetWebsocket.EnviaListaOradores(orador_);
        }

        private void EnviaListaPresenca(int index, int id_sala, List<Presence> presenca)
        {
            if (PromicDB.ListaControle[index].WEBSOCKET is null)
            {
                int id_reuniao = PromicDB.ListaControle[index].ID_REUNIAO;
                promicserviceNet.ConectarWebSocket(id_reuniao, id_sala);
            }
            PromicWebSocket PromicNetWebsocket = PromicDB.ListaControle[index].WEBSOCKET;

            PromicNetWebsocket.EnviaListaPresenca(presenca);
        }

        private void EnviaPauta(int index, int id_sala, PautaDTO pauta)
        {
            try
            {
                if (PromicDB.ListaControle[index].WEBSOCKET is null)
                {
                    int id_reuniao = PromicDB.ListaControle[index].ID_REUNIAO;
                    promicserviceNet.ConectarWebSocket(id_reuniao, id_sala);
                }
                PromicWebSocket promicNetWebsocket = PromicDB.ListaControle[index].WEBSOCKET;
                if (promicNetWebsocket is null)
                {
                    Log("[Interface do WebSocket está nulo.]");
                }
                else
                {
                    Log("[Enviando pauta]");
                    promicNetWebsocket.EnviarPauta(pauta.parts);
                }
            }
            catch (Exception e)
            {
                Log(e.Message);
            }
        }

        public void OnLogDB(HReturn retorno)
        {
            Log("DB] [Falha " + retorno.Code + " - " + retorno.Message + "}");
        }

        public void OnPerdeuConexaoDB(object sender)
        {
            Log("[DB] [Desconectou]");

        }

        public void OnEstadoControlesDB(int index, EstadoOperacao estadoAtual, EstadoOperacao estadoAnterior)
        {
            int reuniao = PromicDB.ListaControle[index].ID_REUNIAO;
            int sala = PromicDB.ListaControle[index].ID_SALA;

            // ADICIONAR VALIDAÇÃO DA CONEXÃO COM O WEB SOCKET EM TODOS OS EVENTOS
            Log($"[DB] [OnEstadoControles] [ Sala:{sala}] [Estado Atual: {estadoAtual}] [Estado Anterior: {estadoAnterior}]");

            if (PromicDB.ListaControle[index].WEBSOCKET is null
                && estadoAtual != EstadoOperacao.Nada)
            {
                promicserviceNet.ConectarWebSocket(reuniao, sala);
            }

            PromicWebSocket PromicNetWebsocket = PromicDB.ListaControle[index].WEBSOCKET;

            switch (estadoAtual)
            {
                case EstadoOperacao.Nada:
                    {
                        if (PromicDB.ListaControle[index].WEBSOCKET is null)
                        {
                            return;
                        }
                        promicserviceNet.DesconectarWebSocket(sala);
                        break;
                    }
                case EstadoOperacao.Reuniao:
                    {
                        if (estadoAnterior == EstadoOperacao.Parte)
                        {
                            Log(" encerrando parte ");
                            PromicNetWebsocket.EncerraParte();
                        }
                        break;
                    }
                case EstadoOperacao.VerificacaoPresenca:
                    {
                        Log(" Verificando presença");
                        PromicNetWebsocket.EnviaListaPresenca(PromicDB.ListaControle[index].ListaPresenca);

                        break;
                    }
                case EstadoOperacao.Parte:
                    {
                        if (estadoAnterior == EstadoOperacao.Nada)
                        {
                            List<ParteDTO> partes = PromicDB.ListaControle[index].PAUTA.parts;

                            Log("Enviando pauta");

                            PromicNetWebsocket.EnviarPauta(partes);
                        }
                        if (estadoAnterior == EstadoOperacao.TempoOradorParte)
                        {
                            PromicNetWebsocket.FinalizarTempoORador(PromicDB.ListaControle[index].ID_DELEGADO);
                        }

                        int id_parte = PromicDB.ListaControle[index].ID_PARTE;

                        Log(" iniciando parte ");

                        bool speakers = PromicDB.ListaControle[index].TIPOPARTE == "S";
                        PromicNetWebsocket.IniciarParte(id_parte, speakers);

                        if (estadoAnterior == EstadoOperacao.Discussao)
                        {
                            Log(" encerrando discussão ");
                            PromicNetWebsocket.EncerrarDiscussao();
                        }
                        else if (estadoAnterior == EstadoOperacao.VotacaoRapida
                            || estadoAnterior == EstadoOperacao.Votacao
                            || estadoAnterior == EstadoOperacao.TempoAparteOradorEmVotacao
                            || estadoAnterior == EstadoOperacao.TempoOradorEmVotacao)
                        {
                            Log(" cancelando votação ");
                            PromicNetWebsocket.CancelarVotacao(PromicDB.ListaControle[index].ID_VOTACAO);
                        }
                        break;
                    }
                case EstadoOperacao.TempoOradorParte:
                    {
                        if (PromicDB.ListaControle[index].TIPOPARTE == "A")
                        {
                            Log(" tempo para inscrição orador ");
                            PromicNetWebsocket.IniciaTempoInscricao(Convert.ToInt32(PromicDB.ListaControle[index].TEMPO_ORADOR));
                        }
                        else
                        {
                            int user_id = PromicDB.BuscaDelegadoPorIdOrador(PromicDB.ListaControle[index].ID_SALA, PromicDB.ListaControle[index].ID_DELEGADO, EstadoOperacao.TempoOradorParte);

                            int tempo = 300;

                            if (PromicDB.ListaControle[index].ListaOradores.Count > 0)
                            {
                                tempo = PromicDB.ListaControle[index].ListaOradores.First(o => o.ID_DELEGADO == user_id).TEMPO;
                            }
                            else
                            {

                                TimeSpan tempoSpan = PromicDB.RetornaTempo(@"select tipoparte.tempoorador from configuracao
                                   inner join reuniaopartes on (configuracao.id_parte = reuniaopartes.id_parte)
                                   inner join tipoparte on (reuniaopartes.id_tipoparte = tipoparte.id_tipoparte)
                                   where (id_config = " + sala + ");");

                                if (tempoSpan != default(TimeSpan))
                                    tempo = Convert.ToInt32(tempoSpan.TotalSeconds);
                            }

                            Log(" iniciando tempo orador ");
                            PromicNetWebsocket.IniciarTempoOrador(user_id, tempo);
                        }
                        break;
                    }
                case EstadoOperacao.Discussao:
                    {
                        if (estadoAnterior == EstadoOperacao.Parte)
                        {
                            int id_discussao = PromicDB.ListaControle[index].ID_VOTACAO;
                            Log(" iniciando discussão");
                            PromicNetWebsocket.IniciarDiscussao(id_discussao);
                        }
                        else if (estadoAnterior == EstadoOperacao.TempoOradorDiscussao)
                        {
                            Log(" Fim tempo orador");
                            PromicNetWebsocket.FinalizarTempoORador(PromicDB.ListaControle[index].ID_DELEGADO);
                        }
                        // votação cancelada
                        else if (estadoAnterior == EstadoOperacao.Votacao
                            || estadoAnterior == EstadoOperacao.VotacaoRapida
                            || estadoAnterior == EstadoOperacao.TempoAparteOradorEmVotacao)
                        {
                            int id_votacao = PromicDB.ListaControle[index].ID_VOTACAO;
                            Log(" cancelando votação ");
                            PromicNetWebsocket.CancelarVotacao(id_votacao);
                        }
                        else if (estadoAnterior == EstadoOperacao.TempoOradorEmVotacao)
                        {
                            PromicNetWebsocket.FinalizarTempoORador(PromicDB.ListaControle[index].ID_DELEGADO);
                        }
                        break;
                    }
                case EstadoOperacao.TempoOradorDiscussao:
                    {
                        if (PromicDB.ListaControle[index].TIPOPARTE == "A")
                        {
                            Log(" tempo para inscrição orador ");
                            PromicNetWebsocket.IniciaTempoInscricao(Convert.ToInt32(PromicDB.ListaControle[index].TEMPO_ORADOR));
                        }
                        else
                        {
                            int user_id = PromicDB.BuscaDelegadoPorIdOrador(PromicDB.ListaControle[index].ID_SALA, PromicDB.ListaControle[index].ID_DELEGADO, EstadoOperacao.TempoOradorDiscussao);
                            int tempo = 300;
                            TimeSpan tempoSpan = PromicDB.RetornaTempo(@"select tipoparte.tempoorador from configuracao
                                   inner join reuniaopartes on (configuracao.id_parte = reuniaopartes.id_parte)
                                   inner join tipoparte on (reuniaopartes.id_tipoparte = tipoparte.id_tipoparte)
                                   where (id_config = " + sala + ");");

                            if (tempoSpan != default(TimeSpan))
                                tempo = Convert.ToInt32(tempoSpan.TotalSeconds);

                            Log(" iniciando tempo orador ");
                            PromicNetWebsocket.IniciarTempoOrador(user_id, tempo);
                        }
                        break;
                    }
                case EstadoOperacao.Votacao:
                    {
                        Log("Iniciando votação");
                        int tipo_resposta = PromicDB.ListaControle[index].VotacaoTotais.RESPOSTA;
                        int tempo = Convert.ToInt32(PromicDB.ListaControle[index].TEMPO_VOTACAO.TotalSeconds);
                        int id_votacao = PromicDB.ListaControle[index].ID_VOTACAO;
                        string titulo = PromicDB.ListaControle[index].VotacaoTotais.TITULO;
                        string ementa = PromicDB.ListaControle[index].VotacaoTotais.EMENTA;

                        if (PromicDB.ListaControle[index].ListaVotoNominal.Count > 0)
                        {

                            foreach (VotoNominal item in PromicDB.ListaControle[index].ListaVotoNominal)
                            {
                                if (item.DEVEVOTAR == "S")
                                {
                                    Log("iniciando votação para: " + item.NOME + $" - ID {item.ID_DELEGADO}");
                                    Thread.Sleep(100);
                                    PromicNetWebsocket.IniciarVotacao(id_votacao, titulo, ementa, (TipoVotacao)tipo_resposta, tempo, item.ID_DELEGADO);
                                }
                                else
                                {
                                    Log($"[VOTAÇÃO] [{item.ID_DELEGADO} - {item.NOME} não pode votar.]");
                                }
                            }
                        }
                        else
                        {
                            Log("Nenhum participante para iniciar votação;");
                        }
                        break;
                    }
                case EstadoOperacao.Resultado:
                    {
                        int id_votacao = PromicDB.ListaControle[index].VotacaoTotais.ID_VOTACAO;
                        int tipo_resposta = PromicDB.ListaControle[index].VotacaoTotais.RESPOSTA;
                        bool resultado = PromicDB.ListaControle[index].VotacaoTotais.RESULTADO == 1;

                        List<VOTO> votos = new List<VOTO>();
                        List<VotoNominal> votos_base = PromicDB.ListaControle[index].ListaVotoNominal;
                        if (PromicDB.ListaControle[index].VotacaoTotais.SECRETO == "S")
                        {
                            foreach (VotoNominal voto in votos_base)
                            {
                                votos.Add(new VOTO { NOME_PARTICIPANTE = voto.NOME, PARTIDO = voto.PARTIDO, VOTOU = !string.IsNullOrEmpty(voto.VOTO), VOTO_PARTICIPANTE = "" });
                            }
                        }
                        else
                        {
                            foreach (VotoNominal voto in votos_base)
                            {
                                voto.VOTO = voto.VOTO == "NAO" ? "NÃO" : voto.VOTO;
                                votos.Add(new VOTO { NOME_PARTICIPANTE = voto.NOME, PARTIDO = voto.PARTIDO, VOTOU = !string.IsNullOrEmpty(voto.VOTO), VOTO_PARTICIPANTE = voto.VOTO });
                            }
                        }

                        int[] resultadoVotacao;
                        if ((TipoVotacao)tipo_resposta == TipoVotacao.SimNao)
                        {
                            resultadoVotacao = new int[]{
                                PromicDB.ListaControle[index].VotacaoTotais.VOTO1,
                                PromicDB.ListaControle[index].VotacaoTotais.VOTO2,
                                PromicDB.ListaControle[index].VotacaoTotais.TOTALVOTOS
                            };
                        }
                        else
                        {
                            resultadoVotacao = new int[]{
                                PromicDB.ListaControle[index].VotacaoTotais.VOTO1,
                                PromicDB.ListaControle[index].VotacaoTotais.VOTO2,
                                PromicDB.ListaControle[index].VotacaoTotais.VOTO3,
                                PromicDB.ListaControle[index].VotacaoTotais.TOTALVOTOS
                            };
                        }

                        string titulo = PromicDB.ListaControle[index].VotacaoTotais.TITULO;
                        string ementa = PromicDB.ListaControle[index].VotacaoTotais.EMENTA;

                        PromicNetWebsocket.EncerrarVotacao(id_votacao, titulo, ementa, (TipoVotacao)tipo_resposta, resultado, resultadoVotacao, votos);
                        PromicNetWebsocket.EncerrarDiscussao();
                        break;
                    }
                case EstadoOperacao.VotacaoRapida:
                    {
                        int VotosNecessarios = PromicDB.ListaControle[index].VotacaoTotais.VOTOSNECES;
                        int TotalVotos = PromicDB.ListaControle[index].VotacaoTotais.TOTALVOTOS;

                        Log("PromicNet: Envia Lista Votacao Totais: " + TotalVotos.ToString() + "/" + VotosNecessarios.ToString());
                        Log("DB: Atualiza Lista Votos Nominais: " + PromicDB.ListaControle[index].ListaVotoNominal.Count.ToString());

                        int tempo = Convert.ToInt32(PromicDB.ListaControle[index].TEMPO_VOTACAO.TotalSeconds);
                        int tipo_resposta = PromicDB.ListaControle[index].VotacaoTotais.RESPOSTA;
                        int id_votacao = PromicDB.ListaControle[index].ID_RAPIDO;
                        string titulo = PromicDB.ListaControle[index].VotacaoTotais.TITULO;
                        string ementa = PromicDB.ListaControle[index].VotacaoTotais.EMENTA;

                        foreach (VotoNominal item in PromicDB.ListaControle[index].ListaVotoNominal)
                        {
                            if (item.DEVEVOTAR == "S")
                            {
                                Log("Votação RAPIDA iniciada para: " + item.NOME + $" - {item.ID_DELEGADO}");
                                Thread.Sleep(100);
                                PromicNetWebsocket.IniciarVotacao(id_votacao, titulo, ementa, (TipoVotacao)tipo_resposta, tempo, item.ID_DELEGADO);
                            }
                            else
                            {
                                Log($"[VOTAÇÃO RÁPIDA] {item.ID_DELEGADO} - {item.NOME} não pode votar.");
                            }
                        }
                        break;
                    }
                case EstadoOperacao.TempoOradorDeslogoNada:
                    {
                        break;
                    }
                case EstadoOperacao.TempoOradorDeslogoReuniao:
                    {
                        break;
                    }
                case EstadoOperacao.TempoOradorEmVotacao:
                    {
                        break;
                    }
                case EstadoOperacao.ColetaBiometria:
                    {
                        break;
                    }
                case EstadoOperacao.TempoAparteOrador:
                    {
                        break;
                    }
                case EstadoOperacao.TempoAparteOradorVotacao:
                    {
                        break;
                    }
                case EstadoOperacao.TempoOradorResultado:
                    {
                        break;
                    }
                case EstadoOperacao.TempoAparteOradorResultado:
                    {
                        break;
                    }
                case EstadoOperacao.TempoAparteOradorEmVotacao:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public void OnListaVotacaoGeralDB(int index, int id_sala, VotacaoTotais totais)
        {
            if (PromicDB.ListaControle[index].ESTADO_ATUAL != 8 &&
                totais.SECRETO != "S")
            {
                PromicWebSocket PromicNetWebsocket = PromicDB.ListaControle[index].WEBSOCKET;
                List<VotoNominal> listaVotoNominal = PromicDB.ListaControle[index].ListaVotoNominal;

                Log("DB: Atualiza Lista Votos Geral: " + listaVotoNominal.Count.ToString());

                List<VOTO> votos = new List<VOTO>();

                foreach (VotoNominal i in listaVotoNominal)
                {
                    string voto_participante = "";
                    if (totais.REAL != "N")
                    {
                        voto_participante = i.VOTO;
                    }

                    votos.Add(new VOTO
                    {
                        NOME_PARTICIPANTE = i.NOME,
                        PARTIDO = i.PARTIDO,
                        VOTOU = !string.IsNullOrEmpty(i.VOTO),
                        VOTO_PARTICIPANTE = voto_participante
                    });
                }
                PromicNetWebsocket.AtualizarListaVotos(votos);
            }
        }

        public void OnListaVotosNominaisDB(int index, int id_sala, List<VotoNominal> lista)
        {
            if (PromicDB.ListaControle[index].ESTADO_ATUAL != 8)
            {
                int reuniao = PromicDB.ListaControle[index].ID_REUNIAO;
                int sala = PromicDB.ListaControle[index].ID_SALA;

                if (PromicDB.ListaControle[index].WEBSOCKET is null)
                {
                    promicserviceNet.ConectarWebSocket(reuniao, sala);
                }

                PromicWebSocket PromicNetWebsocket = PromicDB.ListaControle[index].WEBSOCKET;

                Log("DB: Atualiza Lista Votos Nominais: " + lista.Count.ToString());

                List<VotoNominal> listaVotoNominal = PromicDB.ListaControle[index].ListaVotoNominal;

                List<VOTO> votos = new List<VOTO>();
                foreach (VotoNominal i in listaVotoNominal)
                {
                    string voto;

                    if (PromicDB.ListaControle[index].VotacaoTotais.SECRETO == "S"
                        || PromicDB.ListaControle[index].VotacaoTotais.REAL == "N")
                    {
                        voto = "";
                    }
                    else
                    {
                        voto = i.VOTO;
                    }
                    votos.Add(new VOTO { NOME_PARTICIPANTE = i.NOME, PARTIDO = i.PARTIDO, VOTOU = !string.IsNullOrEmpty(i.VOTO), VOTO_PARTICIPANTE = voto });
                }
                PromicNetWebsocket.AtualizarListaVotos(votos);
            }
        }

        public void OnPausaCronometroSet(int index, Cronometro crono, int Status)
        {
            int id_reuniao = PromicDB.ListaControle[index].ID_REUNIAO;
            int id_sala = PromicDB.ListaControle[index].ID_SALA;

            if (PromicDB.ListaControle[index].WEBSOCKET == null)
            {
                promicserviceNet.ConectarWebSocket(id_reuniao, id_sala);
            }

            PromicWebSocket PromicNetWebsocket = PromicDB.ListaControle[index].WEBSOCKET;
            Log("[DB] [Cronômetro Status Pausa: " + Status.ToString() + "] [Cronômetro: " + crono.ToString() + "]");

            switch (crono)
            {
                case Cronometro.CronoParte:
                    break;
                case Cronometro.CronoOrador:
                    {
                        Log($"[Cronometro] [Pausando tempo de orador]");
                        PromicNetWebsocket.PausarTempoOrador(Status);
                        break;
                    }
                case Cronometro.CronoAparte:
                    break;
                case Cronometro.CronoPresenca:
                    break;
                case Cronometro.CronoVotacao:
                    {
                        Log($"[Cronometro] [Pausando cronometro da votação]");
                        PromicNetWebsocket.PausarVotacao(Status == 1);
                        break;
                    }
                case Cronometro.CronoMateria:
                    break;
                default:
                    break;
            }
        }


        public void OnTempoCronometroSet(int index, Cronometro crono, TimeSpan TempoRestante)
        {
            int id_reuniao = PromicDB.ListaControle[index].ID_REUNIAO;
            int id_sala = PromicDB.ListaControle[index].ID_SALA;

            if (PromicDB.ListaControle[index].WEBSOCKET == null)
            {
                promicserviceNet.ConectarWebSocket(id_reuniao, id_sala);
            }

            PromicWebSocket PromicNetWebsocket = PromicDB.ListaControle[index].WEBSOCKET;

            Log("[DB] [Cronômetro Tempo: " + TempoRestante.ToString() + "seg] [Cronômetro: " + crono.ToString() + "]");

            switch (crono)
            {
                case Cronometro.CronoParte:
                    {
                        break;
                    }
                case Cronometro.CronoOrador:
                    {
                        PromicNetWebsocket.AtualizarTempoOrador(PromicDB.ListaControle[index].ID_DELEGADO, Convert.ToInt32(TempoRestante.TotalSeconds));
                        break;
                    }
                case Cronometro.CronoAparte:
                    {
                        break;
                    }
                case Cronometro.CronoPresenca:
                    {
                        break;
                    }
                case Cronometro.CronoVotacao:
                    {
                        break;
                    }
                case Cronometro.CronoMateria:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}
