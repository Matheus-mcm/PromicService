using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using System.Globalization;
using System.Security;

namespace LibPromicDB
{
    public partial class PromicDB
    {
        public string connectionstring { get; private set; }
        private AutoResetEvent ThrStopEvent;

        public PromicDB(bool local, string IP, int porta)
        {
            if (local)
            {
                connectionstring = "User=SYSDBA;Password=masterkey;Database=localhost:DBPROMIC5;Port=" + porta + ";Dialect=3;Charset=ISO8859_1;Connection Lifetime=900;Connection Timeout=10;ServerType=0;Pooling=true;MinPoolSize=3;MaxPoolSize=50;";
            }
            else
            {
                connectionstring = "User=SYSDBA;Password=masterkey;Database=" + IP + ":DBPROMIC5;DataSource=" + IP + ";Port=" + porta + ";Dialect=3;Charset=ISO8859_1;Connection Lifetime=900;Connection Timeout=10;ServerType=0;Pooling=true;MinPoolSize=3;MaxPoolSize=50;";
            }
        }

        public void IniciarLoop()
        {
            ThrStopEvent = new AutoResetEvent(false);
            Thread thr = new Thread(ThreadLoop);
            thr.IsBackground = true;
            thr.SetApartmentState(ApartmentState.STA);
            thr.Start();
        }

        public void PararLoop()
        {
            ThrStopEvent.Set();
            Task.Delay(300);
        }

        private void ThreadLoop()
        {
            ListaControle = new List<ControlesLista>();

            while (!ThrStopEvent.WaitOne(100))
            {

                GetListaEstadoOperacao();

                for (int i = 0; i < ListaControle.Count; i++)
                {
                    bool sendListaPresenca = false;
                    bool sendListaProsposicao = false;
                    bool sendListaOradores = false;
                    bool sendListaApartes = false;
                    bool sendListaVotacaoTotais = false;
                    bool sendListaVotosNominais = false;
                    bool sendListaVotacaoResultadoChk = false;
                    bool sendListaVotosNominaisChk = false;
                    bool sendPauta = false;

                    int id_sala = ListaControle[i].ID_SALA;
                    int id_reuniao = ListaControle[i].ID_REUNIAO;

                    switch (ListaControle[i].ESTADO_ATUAL)
                    {
                        case 0:
                            {
                                // REUNIÃO NÃO INICIADA
                                break;
                            }
                        case 1:
                            {
                                // INCIADA A Reuniao 
                                sendPauta = GetPauta(id_sala, id_reuniao);
                                sendListaPresenca = GetListaPresenca(id_sala);
                                break;
                            }
                        case 2:
                            {
                                // INICIADA A VERIFICAÇÃO DE PRESENCA 
                                sendPauta = GetPauta(id_sala, id_reuniao);
                                sendListaPresenca = GetListaPresenca(id_sala);
                                break;
                            }
                        case 3:
                            {
                                // INICIADA A PARTE 
                                sendPauta = GetPauta(id_sala, id_reuniao);
                                sendListaPresenca = GetListaPresenca(id_sala);
                                sendListaOradores = GetListaOradores(id_sala);
                                sendListaProsposicao = GetListaProposicoes(id_sala);
                                break;
                            }
                        case 4:
                            {
                                // INICIADO ORADOR DA PARTE 
                                sendListaPresenca = GetListaPresenca(id_sala);
                                sendListaOradores = GetListaOradores(id_sala);
                                sendListaProsposicao = GetListaProposicoes(id_sala);
                                sendListaApartes = GetListaApartes(id_sala);
                                break;
                            }
                        case 5:
                            {
                                // INICIADA A DISCUSSÃO 
                                sendListaPresenca = GetListaPresenca(id_sala);
                                sendListaOradores = GetListaOradores(id_sala);
                                sendListaProsposicao = GetListaProposicoes(id_sala);
                                break;
                            }
                        case 6:
                            {
                                // INICIADO ORADOR DE UMA DISCUSSÃO
                                sendListaPresenca = GetListaPresenca(id_sala);
                                sendListaOradores = GetListaOradores(id_sala);
                                sendListaProsposicao = GetListaProposicoes(id_sala);
                                sendListaApartes = GetListaApartes(id_sala);
                                break;
                            }
                        case 7:
                            {
                                // INICIADA UMA VOTACAO
                                sendListaVotacaoTotais = GetListaVotacaoTotais(i, id_sala, Tabela.VotacaoNormal);
                                sendListaVotosNominais = GetListaVotosNominais(i, id_sala, Tabela.VotacaoNormal);
                                break;
                            }
                        case 8:
                            {
                                // INICIADO O RESULTADO DA VOTACAO
                                if (ListaControle[i].ID_RAPIDO == 0)
                                {
                                    sendListaVotacaoTotais = GetListaVotacaoTotais(i, id_sala, Tabela.VotacaoNormal);
                                    sendListaVotosNominais = GetListaVotosNominais(i, id_sala, Tabela.VotacaoNormal);
                                }
                                else
                                {
                                    sendListaVotacaoTotais = GetListaVotacaoTotais(i, id_sala, Tabela.VotacaoRapida);
                                    sendListaVotosNominais = GetListaVotosNominais(i, id_sala, Tabela.VotacaoRapida);
                                }
                                break;
                            }
                        case 9:
                            {
                                // INICIADA A VOTACAO RAPIDA
                                sendListaVotacaoTotais = GetListaVotacaoTotais(i, id_sala, Tabela.VotacaoRapida);
                                sendListaVotosNominais = GetListaVotosNominais(i, id_sala, Tabela.VotacaoRapida);
                                break;
                            }
                        case 10:
                            {
                                // INICIADO O CRONOMETRO DE ORADOR NAO LOGADO FORA DE UMA Reuniao
                                break;
                            }
                        case 11:
                            {
                                // INICIADO O CRONOMETRO DE ORADOR NAO LOGADO EM UMA Reuniao
                                break;
                            }
                        case 12:
                            {
                                // INICIADO ORADOR QUANDO EM VOTAÇÃO
                                sendListaOradores = GetListaOradores(id_sala);
                                sendListaApartes = GetListaApartes(id_sala);
                                sendListaVotacaoTotais = GetListaVotacaoTotais(i, id_sala, Tabela.VotacaoNormal);
                                sendListaVotosNominais = GetListaVotosNominais(i, id_sala, Tabela.VotacaoNormal);
                                break;
                            }
                        case 13:
                            {
                                // INICIADA A COLETA DE BIOMETRIA
                                break;
                            }
                        case 14:
                            {
                                // INICIADO APARTE DE UM ORADOR DA PARTE
                                sendListaPresenca = GetListaPresenca(id_sala);
                                sendListaOradores = GetListaOradores(id_sala);
                                sendListaProsposicao = GetListaProposicoes(id_sala);
                                sendListaApartes = GetListaApartes(id_sala);
                                break;
                            }
                        case 15:
                            {
                                // INICIADO APARTE DE UM ORADOR DE DISCUSSÃO
                                sendListaPresenca = GetListaPresenca(id_sala);
                                sendListaOradores = GetListaOradores(id_sala);
                                sendListaProsposicao = GetListaProposicoes(id_sala);
                                sendListaApartes = GetListaApartes(id_sala);
                                break;
                            }
                        case 16:
                            {
                                // INICIADO ORADOR EM UM RESULTADO DE VOTAÇÃO
                                sendListaPresenca = GetListaPresenca(id_sala);
                                sendListaOradores = GetListaOradores(id_sala);
                                sendListaProsposicao = GetListaProposicoes(id_sala);
                                sendListaApartes = GetListaApartes(id_sala);
                                break;
                            }
                        case 17:
                            {
                                // INICIADO APARTE DE UM ORADOR DE PROPOSIÇÃO EM UM RESULTADO
                                sendListaPresenca = GetListaPresenca(id_sala);
                                sendListaOradores = GetListaOradores(id_sala);
                                sendListaProsposicao = GetListaProposicoes(id_sala);
                                sendListaApartes = GetListaApartes(id_sala);
                                break;
                            }
                        case 18:
                            {
                                // INICIADO APARTE DE ORADOR DE PROPOSIÇÃO QUANDO EM VOTAÇÃO
                                sendListaPresenca = GetListaPresenca(id_sala);
                                sendListaOradores = GetListaOradores(id_sala);
                                sendListaProsposicao = GetListaProposicoes(id_sala);
                                sendListaApartes = GetListaApartes(id_sala);

                                sendListaVotacaoTotais = GetListaVotacaoTotais(i, id_sala, Tabela.VotacaoNormal);
                                sendListaVotosNominais = GetListaVotosNominais(i, id_sala, Tabela.VotacaoNormal);
                                break;
                            }
                    }

                    if (ListaControle[i].ESTADO_ATUAL != ListaControle[i].ESTADO_ANTERIOR || ListaControle[i].ID_PARTE != ListaControle[i].ID_PARTE_ANTERIOR)
                    {
                        EstadoControles(i, (EstadoOperacao)ListaControle[i].ESTADO_ATUAL, (EstadoOperacao)ListaControle[i].ESTADO_ANTERIOR);
                        ListaControle[i].ESTADO_ANTERIOR = ListaControle[i].ESTADO_ATUAL;
                        ListaControle[i].ID_PARTE_ANTERIOR = ListaControle[i].ID_PARTE;
                    }

                    if (sendListaPresenca)
                    {
                        ListaPresenca(i, id_sala, ListaControle[i].ListaPresenca);
                    }

                    if (sendPauta)
                    {
                        EnviaPauta(i, id_sala, ListaControle[i].PAUTA);
                    }

                    if (sendListaProsposicao)
                    {

                    }

                    if (sendListaOradores)
                    {
                        EnviaListaOradores(i, id_sala, ListaControle[i].ListaOradores);
                    }

                    if (sendListaApartes)
                    {

                    }

                    if (sendListaVotacaoTotais)
                    {
                        EnviaListaVotacaoTotais(i, id_sala, ListaControle[i].VotacaoTotais);
                    }

                    if (sendListaVotosNominais)
                    {
                        EnviaListaVotoNominal(i, id_sala, ListaControle[i].ListaVotoNominal);
                    }

                    if (sendListaVotacaoResultadoChk)
                    {

                    }

                    if (sendListaVotosNominaisChk)
                    {

                    }

                    /// EVENTOS CRONOMETRO PARTE
                    if (ListaControle[i].TEMPO_PARTE_ATUALIZA)
                    {
                        TimeSpan Tempo = ListaControle[i].TEMPO_PARTE;
                        DateTime HoraInicio = ListaControle[i].CRONO_PARTE;
                        DateTime HoraAtual = DateTime.Parse(RetornaColuna("select current_timestamp from rdb$database;"));
                        double Segundos = (HoraAtual - HoraInicio).TotalSeconds;
                        if (Segundos >= 0)
                        {
                            TimeSpan TempoRestante = Tempo - TimeSpan.FromSeconds(Segundos);
                            TempoCronometroSet(i, Cronometro.CronoParte, TempoRestante);
                            ListaControle[i].TEMPO_PARTE_ATUALIZA = false;
                        }
                    }
                    if (ListaControle[i].PAUSA_PARTE_ATUALIZA)
                    {
                        PausaCronometroSet(i, Cronometro.CronoParte, ListaControle[i].PAUSA_PARTE);
                        ListaControle[i].PAUSA_PARTE_ATUALIZA = false;
                    }


                    /// EVENTOS CRONOMETRO ORADOR
                    if (ListaControle[i].TEMPO_ORADOR_ATUALIZA)
                    {
                        TimeSpan Tempo = ListaControle[i].TEMPO_ORADOR;
                        DateTime HoraInicio = ListaControle[i].CRONO_ORADOR;
                        DateTime HoraAtual = DateTime.Parse(RetornaColuna("select current_timestamp from rdb$database;"));
                        double Segundos = (HoraAtual - HoraInicio).TotalSeconds;
                        if (Segundos >= 0)
                        {
                            TimeSpan TempoRestante = Tempo - TimeSpan.FromSeconds(Segundos);
                            TempoCronometroSet(i, Cronometro.CronoOrador, TempoRestante);
                            ListaControle[i].TEMPO_ORADOR_ATUALIZA = false;
                        }
                    }
                    if (ListaControle[i].PAUSA_ORADOR_ATUALIZA)
                    {
                        PausaCronometroSet(i, Cronometro.CronoOrador, ListaControle[i].PAUSA_ORADOR);
                        ListaControle[i].PAUSA_ORADOR_ATUALIZA = false;
                    }


                    /// EVENTOS CRONOMETRO VOTACAO
                    if (ListaControle[i].TEMPO_VOTACAO_ATUALIZA)
                    {
                        TimeSpan Tempo = ListaControle[i].TEMPO_VOTACAO;
                        DateTime HoraInicio = ListaControle[i].CRONO_VOTACAO;
                        DateTime HoraAtual = DateTime.Parse(RetornaColuna("select current_timestamp from rdb$database;"));
                        double Segundos = (HoraAtual - HoraInicio).TotalSeconds;
                        if (Segundos >= 0)
                        {
                            TimeSpan TempoRestante = Tempo - TimeSpan.FromSeconds(Segundos);
                            TempoCronometroSet(i, Cronometro.CronoVotacao, TempoRestante);
                            ListaControle[i].TEMPO_VOTACAO_ATUALIZA = false;
                        }
                    }

                    if (ListaControle[i].PAUSA_VOTACAO_ATUALIZA)
                    {
                        PausaCronometroSet(i, Cronometro.CronoVotacao, ListaControle[i].PAUSA_VOTACAO);
                        ListaControle[i].PAUSA_VOTACAO_ATUALIZA = false;
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "<Pending>")]
        private bool GetListaProposicoes(int id_sala)
        {
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "<Pending>")]
        private bool GetListaApartes(int id_sala)
        {
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "<Pending>")]
        private bool GetListaVotacaoResultadoChk(int id_sala)
        {
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "<Pending>")]
        private bool GetListaVotosNominaisChk(int id_sala)
        {
            return false;
        }
    }
}
