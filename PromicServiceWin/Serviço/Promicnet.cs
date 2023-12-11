using LibPromicDB;
using LibPromicDB.Entidades;
using PromicnetIntegration;
using PromicnetIntegration.Events;
using PromicnetIntegration.Types;
using PromicService.Entidades;
using PromicService.Entites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace PromicService
{
    public class PromicserviceNet
    {
        Api PromicNetApi;
        readonly PromicDB PromicDB;
        Timer timerKeepAlive = new Timer();
        private readonly List<User> ListaUsuarios = new List<User>();
        private readonly Config _config;
        readonly PromicService _promicService;
        private string CodOrgao;
        private PromicnetIntegration.Types.Token MasterToken;
        private string session;

        public PromicserviceNet(PromicService promicService)
        {
            PromicDB = promicService.PromicDB;
            _promicService = promicService;

            string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Riole\\config.json";
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                string conteudo = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
                _config = JsonSerializer.Deserialize<Config>(conteudo);
                _promicService.Log($"[SER] [Abiente do Promicnet: {_config.Ambiente}.]");
            }
            else
            {
                _promicService.Log($"[SER] [Arquivo de configuração não encontrado! Adicione o arquivo de configuração na pasta: \"{path}\" e reinicie o serviço ]");
            }
        }

        #region COMANDOS

        public bool PromicNetGetToken()
        {
            if (string.IsNullOrEmpty(CodOrgao))
            {
                CodOrgao = PromicDB.RetornaColuna("SELECT COD_ORGAO FROM REGISTRO WHERE ID_REGISTRO = 1;");
            }

            try
            {
                if (CodOrgao != "")
                {
                    if (PromicNetApi == null)
                    {
                        PromicNetApi = new Api(_config.ApiURL);
                    }
                    bool executa = false;

                    if (MasterToken == null)
                    {
                        executa = true;
                    }
                    else if (!MasterToken.Valid)
                    {
                        executa = true;
                    }
                    if (executa)
                    {
                        MasterToken = new Token(_config.CognitoURL, _config.MasterID, _config.MasterSK);

                        HReturn<Token> tokenWs = Task.Run(() => PromicNetApi.ObterTokenPromic(MasterToken)).Result;

                        if (tokenWs.SUCESSO)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool ConectarWebSocket(int id_reuniao, int id_sala)
        {
            int index = PromicDB.FindIndex(id_sala);

            if (PromicDB.ListaControle[index].WEBSOCKET != null)
            {
                return true;
            }

            if (CodOrgao == "")
            {
                CodOrgao = PromicDB.RetornaColuna("SELECT COD_ORGAO FROM REGISTRO WHERE ID_REGISTRO = 1");
            }
            try
            {
                if (CodOrgao != "")
                {

                    session = PromicDB.RetornaColuna("SELECT ID_PROMICNET FROM REUNIAO WHERE ID_REUNIAO =  " + id_reuniao + "");
                    if (session != "")
                    {

                        if (PromicNetGetToken())
                        {
                            PromicnetIntegration.PromicWebSocket PromicNetWebsocket = new PromicnetIntegration.PromicWebSocket(_config.WebSocketURL, MasterToken.Access_token, session, CodOrgao, id_sala, "SERVICE");

                            PromicNetWebsocket.ErrorEventHandler += logerror;
                            PromicNetWebsocket.ConectadoEventHandler += ConnectedSuccessfully;
                            PromicNetWebsocket.ParticipanteVotouEventHandler += ParticipanteVotou;
                            PromicNetWebsocket.LogEvent += logmessage;
                            PromicNetWebsocket.ClosedEventHandler += WebsocketDesconectado;
                            PromicNetWebsocket.ConexaoAbertaEventHandler += WebSocketConectado;
                            PromicNetWebsocket.OradorInscritoEventHandler += OradorInscrito;
                            PromicNetWebsocket.OradorDesincritoEventHandler += OradorDesinscrito;
                            PromicNetWebsocket.TempoOradorEncerradoEventHandler += TemppoOradorEncerrado;

                            PromicNetWebsocket.Conectar();

                            timerKeepAlive = new System.Timers.Timer();
                            timerKeepAlive.Interval = 60000;
                            timerKeepAlive.Elapsed += timerKeepAliveElapsed;
                            timerKeepAlive.Start();

                            PromicDB.ListaControle[index].WEBSOCKET = PromicNetWebsocket;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private void TemppoOradorEncerrado(object sender, TempoOradorEncerradoEvent e)
        {
            int sala = e.Id_sala;
            int delegado = e.User_id;

            PromicDB.TempoOradorEncerrado(delegado, sala);
        }

        private void OradorInscrito(object sender, OradorInscritoEvent e)
        {
            int sala = e.Id_sala;
            int delegado = 0;

            string teste = PromicDB.RetornaColuna("SELECT ID_DELEGADO FROM CADDELEGADOS WHERE LOGIN = '" + e.User_email + "';");

            if (!string.IsNullOrEmpty(teste))
            {
                delegado = int.Parse(teste);
            }

            PromicDB.InscricaoOrador(sala, delegado);
        }

        private void OradorDesinscrito(object sender, OradorDesinscritoEvent e)
        {

            int sala = e.Id_sala;
            int delegado = 0;
            string teste = PromicDB.RetornaColuna("SELECT ID_DELEGADO FROM CADDELEGADOS WHERE LOGIN = '" + e.User_email + "';");
            if (!string.IsNullOrEmpty(teste))
            {
                delegado = int.Parse(teste);
            }
            PromicDB.DesincreveOrador(sala, delegado);
        }

        private void WebSocketConectado(object sender, EventArgs e)
        {
            _promicService.Log("WebSocket Conectado.");
        }

        private void WebsocketDesconectado(object sender, ConexaoFechadaEvent e)
        {
            int index = PromicDB.FindIndex(e.id_sala);
            PromicDB.ListaControle[index].WEBSOCKET = null;
            _promicService.Log("Websocket desconetado. Motivo: " + e.Reason);
        }

        private void logmessage(object sender, LogData e)
        {
            _promicService.Log($"[{e.Function}] [{e.Message}]");
        }

        private void logerror(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            _promicService.Log(e.Message);
        }

        public void DesconectarWebSocket(int id_sala)
        {
            int index = PromicDB.FindIndex(id_sala);
            try
            {
                PromicnetIntegration.PromicWebSocket PromicNetWebsocket = PromicDB.ListaControle[index].WEBSOCKET;
                if (PromicNetWebsocket != null)
                {
                    timerKeepAlive.Stop();
                    PromicNetWebsocket.Desconectar();
                    _promicService.Log("[WEBSOCKET] [WebScoket Desconectado]");
                }
            }
            catch (Exception e)
            {
                _promicService.Log($"[WEBSOCKET] [Falha ao desconectar do WebSocket. {e.Message}]");
            }
        }

        private void timerKeepAliveElapsed(object sender, EventArgs e)
        {
            try
            {
                foreach (LibPromicDB.ControlesLista item in PromicDB.ListaControle.Where(lc => lc.WEBSOCKET != null))
                {
                    PromicnetIntegration.PromicWebSocket PromicNetWebsocket = item.WEBSOCKET;
                    PromicNetWebsocket?.ManterLogado();
                }
            }
            catch
            {
                try
                {
                    foreach (LibPromicDB.ControlesLista item in PromicDB.ListaControle.Where(lc => lc.WEBSOCKET != null))
                    {
                        PromicnetIntegration.PromicWebSocket PromicNetWebsocket = item.WEBSOCKET;
                        PromicNetWebsocket.Conectar();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        #endregion

        #region EVENTOS

        private void ParticipanteVotou(object sender, ParticipanteVotouEvent e)
        {
            int voto;
            int sala;
            int delegado;

            if (e.Voto.ToLower().Trim() == "sim")
            {
                voto = 1;
            }
            else if (e.Voto.ToLower().Trim() == "não" ||
                e.Voto.ToLower().Trim() == "nao")
            {
                voto = 2;
            }
            else
            {
                voto = 3;
            }

            sala = e.Id_sala;
            delegado = e.Id_delegado;

            _promicService.Log($"[WEBSOCKET] [Voto {e.Participante} recebido.]");
            PromicDB.RecebeVoto(sala, delegado, voto);
        }

        private async void ConnectedSuccessfully(object sender, ConectadoEvent e)
        {
            HReturn<List<Participante>> item = await PromicNetApi.ObterListaParticipantesAtivos(MasterToken, session);
            if (item.SUCESSO)
            {
                for (int i = 0; i <= item.OBJETO.Count - 1; i++)
                {
                    int index = PromicDB.FindIndex(e.Id_sala);
                    User user = new User
                    {
                        username = PromicDB.RetornaColuna("SELECT FIRST 1 (NOME) FROM CADDELEGADOS WHERE LOGIN = '" + item.OBJETO[i].LOGIN + "';"),
                        login = item.OBJETO[i].LOGIN,
                        uid = Convert.ToUInt32(item.OBJETO[i].ID_AGORA),
                        mutedvideo = !item.OBJETO[i].CAMERA,
                        mutedaudio = !item.OBJETO[i].MICROFONE,
                        getvideo = false
                    };

                    int ID_DELEGADO = 0;
                    try
                    {
                        if (user.username != "")
                            ID_DELEGADO = Int32.Parse(PromicDB.RetornaColuna("SELECT first 1 (ID_DELEGADO) FROM CADDELEGADOS WHERE LOGIN = '" + item.OBJETO[i].LOGIN + "';"));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    if (ID_DELEGADO != 0)
                    {
                        user.id_delegado = ID_DELEGADO;
                        PromicDB.AlteraTabela("UPDATE OR INSERT INTO MODELOMIC (ID_MODELO, COD_MIC, ID_DELEGADO, VINCULACAO, ID_SALA) VALUES (" + PromicDB.ListaControle[index].ID_MODELO + ", " + e.Id_sala + "9" + ID_DELEGADO.ToString().PadLeft(3, '0') + "," + ID_DELEGADO + ", 'V', " + e.Id_sala + ") matching (ID_MODELO,  COD_MIC, ID_DELEGADO);");
                    }
                    else user.id_delegado = -1;

                    ListaUsuarios.Add(user);

                    if (ListaUsuarios.Count == 1)
                    {
                        ListaUsuarios[0].getvideo = true;
                    }
                }
            }

            if (session != "")
            {
                await PromicNetApi.IngressarSessaoAgora(MasterToken, session, e.Id_agora.ToString(), e.Id_conexao);
            }
        }
        #endregion
    }
}
