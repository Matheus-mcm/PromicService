using FirebirdSql.Data.FirebirdClient;
using PromicnetIntegration;
using PromicnetWebsocket;
using PromicnetWebsocket.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PromicnetIntegration.PromicWebSocket;
using static PromicnetIntegration.TypesDTO;

namespace LibPromicDB
{
    public partial class PromicDB
    {
        public List<ControlesLista> ListaControle { get; set; }


        // EVENTO ESTADO DE OPERAÇÃO DOS CONTROLES
        public event EstadoControlesDelegate OnEstadoControles;
        public delegate void EstadoControlesDelegate(int index, EstadoOperacao estadoAtual, EstadoOperacao estadoAnterior);
        protected virtual void EstadoControles(int index, EstadoOperacao estadoAtual, EstadoOperacao estadoAnterior)
        {
            OnEstadoControles?.Invoke(index, estadoAtual: estadoAtual, estadoAnterior: estadoAnterior);
        }

        // EVENTO DE CRONOMETRO
        public event CronometroTempoDelegate OnTempoCronometroSet;
        public delegate void CronometroTempoDelegate(int index, Cronometro crono, TimeSpan tempo);
        protected virtual void TempoCronometroSet(int index, Cronometro crono, TimeSpan tempo)
        {
            OnTempoCronometroSet?.Invoke(index, crono, tempo);
        }


        // EVENTO DE CRONOMETRO
        public event CronometroPausaDelegate OnPausaCronometroSet;
        public delegate void CronometroPausaDelegate(int index, Cronometro crono, int status);
        protected virtual void PausaCronometroSet(int index, Cronometro crono, int status)
        {
            OnPausaCronometroSet?.Invoke(index, crono, status);
        }

        // EVENTO DE PAUTA
        public event PautaDelegate OnPauta;
        public delegate void PautaDelegate(int index, int id_sala, PautaDTO pauta);
        protected virtual void EnviaPauta(int index, int id_sala, PautaDTO pauta)
        {
            OnPauta?.Invoke(index, id_sala, pauta);
        }

        public event ListaPresencaDelegate OnListaPresenca;
        public delegate void ListaPresencaDelegate(int index, int id_sala, List<Presence> presenca);
        protected virtual void ListaPresenca(int index, int id_sala, List<Presence> presenca)
        {
            OnListaPresenca?.Invoke(index, id_sala, presenca);
        }
        public enum Cronometro
        {
            CronoParte = 0,
            CronoOrador = 1,
            CronoAparte = 2,
            CronoPresenca = 3,
            CronoVotacao = 4,
            CronoMateria = 5
        }
        public enum EstadoOperacao
        {
            Nada = -1,
            Reuniao = 1,
            VerificacaoPresenca = 2,
            Parte = 3,
            TempoOradorParte = 4,
            Discussao = 5,
            TempoOradorDiscussao = 6,
            Votacao = 7,
            Resultado = 8,
            VotacaoRapida = 9,
            TempoOradorDeslogoNada = 10,
            TempoOradorDeslogoReuniao = 11,
            TempoOradorEmVotacao = 12,
            ColetaBiometria = 13,
            TempoAparteOrador = 14,
            TempoAparteOradorVotacao = 15,
            TempoOradorResultado = 16,
            TempoAparteOradorResultado = 17,
            TempoAparteOradorEmVotacao = 18
        }

        private void GetListaEstadoOperacao()
        {
            string SqlText = "SELECT ID_SALA, ID_MODELO, ID_REUNIAO, ID_PARTE, ID_DELEGADO, ID_VOTACAO, ID_RAPIDO, TIPOVOT, RESULTADO, NOMINAL, PRESENCA, CRONO_PARTE, TEMPO_PARTE, CRONO_ORADOR, TEMPO_ORADOR,"
                     + " CRONO_VOTACAO, TEMPO_VOTACAO, CRONO_PRESENCA, TEMPO_PRESENCA, PAUSA_PARTE, PAUSA_ORADOR, PAUSA_VOTACAO, PAUSA_PRESENCA, CORADORES, CVOTACAO, CVOTACAOORADORES, CAPARTE, DEBATE, ESTADO, "
                     + " CRONO_MATERIA, TEMPO_MATERIA, PAUSA_MATERIA, CRONO_APARTE, TEMPO_APARTE, PAUSA_APARTE, ID_APARTE "
                      + " FROM  VCONFIGURACAO  ORDER BY ID_SALA";

            try
            {

                int i = 0;


                using (FbConnection con = new FbConnection(connectionstring))
                {
                    con.Open();
                    using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (FbCommand FbCom = new FbCommand(SqlText, con, transaction))
                        {
                            FbCom.CommandTimeout = 5;
                            using (FbDataReader dread = FbCom.ExecuteReader())
                            {
                                if (dread.HasRows)
                                {
                                    while (dread.Read())

                                    {
                                        if (ListaControle.Count == i)
                                        {
                                            ListaControle.Add(new ControlesLista());
                                        }

                                        if (System.Convert.ToInt32(dread["ID_MODELO"]) == 0)
                                        {
                                            continue;
                                        }

                                        #region CRONOMETROS
                                        // CONTROLA CRONO PARTE
                                        if (System.Convert.ToInt32(dread["ID_PARTE"]) != 0)
                                        {
                                            if ((DateTime)dread["CRONO_PARTE"] != ListaControle[i].CRONO_PARTE || (TimeSpan)dread["TEMPO_PARTE"] != ListaControle[i].TEMPO_PARTE || System.Convert.ToInt32(dread["PAUSA_PARTE"]) != ListaControle[i].PAUSA_PARTE)
                                            {
                                                ListaControle[i].CRONO_PARTE = (DateTime)dread["CRONO_PARTE"];
                                                ListaControle[i].TEMPO_PARTE = (TimeSpan)dread["TEMPO_PARTE"];
                                                ListaControle[i].PAUSA_PARTE = System.Convert.ToInt32(dread["PAUSA_PARTE"]);

                                                /// ENVIAR ATUALIZACAO DO ESTADO DO CRONOMETRO
                                            }
                                        }
                                        else
                                        {
                                            ListaControle[i].CRONO_PARTE = (DateTime)dread["CRONO_PARTE"];
                                            ListaControle[i].TEMPO_PARTE = (TimeSpan)dread["TEMPO_PARTE"];
                                            ListaControle[i].PAUSA_PARTE = System.Convert.ToInt32(dread["PAUSA_PARTE"]);
                                        }

                                        // CONTROLA CRONO ORADOR E VOTACAOORADOR
                                        if (System.Convert.ToInt32(dread["ID_DELEGADO"]) != 0)
                                        {
                                            if ((System.Convert.ToInt32(dread["PAUSA_ORADOR"]) != ListaControle[i].PAUSA_ORADOR))
                                            {
                                                ListaControle[i].PAUSA_ORADOR = System.Convert.ToInt32(dread["PAUSA_ORADOR"]);
                                                ListaControle[i].PAUSA_ORADOR_ATUALIZA = true;
                                            }
                                            else
                                            {
                                                var tempo = (TimeSpan)dread["TEMPO_ORADOR"];
                                                var pause = System.Convert.ToInt32(dread["PAUSA_ORADOR"]);
                                                if (tempo != ListaControle[i].TEMPO_ORADOR
                                                    && pause == 1)
                                                {
                                                    ListaControle[i].CRONO_ORADOR = (DateTime)dread["CRONO_ORADOR"];
                                                    ListaControle[i].TEMPO_ORADOR = (TimeSpan)dread["TEMPO_ORADOR"];
                                                    ListaControle[i].TEMPO_ORADOR_ATUALIZA = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ListaControle[i].CRONO_ORADOR = (DateTime)dread["CRONO_ORADOR"];
                                            ListaControle[i].TEMPO_ORADOR = (TimeSpan)dread["TEMPO_ORADOR"];
                                            ListaControle[i].PAUSA_ORADOR = System.Convert.ToInt32(dread["PAUSA_ORADOR"]);
                                        }

                                        // CONTROLA CRONO VOTACAO
                                        if (System.Convert.ToInt32(dread["ID_VOTACAO"]) != 0 && System.Convert.ToInt32(dread["DEBATE"]) == 0)
                                        {
                                            if (((DateTime)dread["CRONO_VOTACAO"] != ListaControle[i].CRONO_VOTACAO
                                                || (TimeSpan)dread["TEMPO_VOTACAO"] != ListaControle[i].TEMPO_VOTACAO)
                                                && System.Convert.ToInt32(dread["PAUSA_VOTACAO"]) != 1)
                                            {
                                                ListaControle[i].CRONO_VOTACAO = (DateTime)dread["CRONO_VOTACAO"];
                                                ListaControle[i].TEMPO_VOTACAO = (TimeSpan)dread["TEMPO_VOTACAO"];
                                                ListaControle[i].TEMPO_VOTACAO_ATUALIZA = true;
                                            }

                                            if ((System.Convert.ToInt32(dread["PAUSA_VOTACAO"]) != ListaControle[i].PAUSA_VOTACAO))
                                            {
                                                ListaControle[i].PAUSA_VOTACAO = System.Convert.ToInt32(dread["PAUSA_VOTACAO"]);
                                                ListaControle[i].PAUSA_VOTACAO_ATUALIZA = true;
                                            }
                                        }
                                        else
                                        {
                                            ListaControle[i].CRONO_VOTACAO = (DateTime)dread["CRONO_VOTACAO"];
                                            ListaControle[i].TEMPO_VOTACAO = (TimeSpan)dread["TEMPO_VOTACAO"];
                                            ListaControle[i].PAUSA_VOTACAO = System.Convert.ToInt32(dread["PAUSA_VOTACAO"]);
                                        }

                                        // CONTROLA CRONO PRESENCA
                                        if (System.Convert.ToInt32(dread["PRESENCA"]) != 0)
                                        {
                                            if ((DateTime)dread["CRONO_PRESENCA"] != ListaControle[i].CRONO_PRESENCA || (TimeSpan)dread["TEMPO_PRESENCA"] != ListaControle[i].TEMPO_PRESENCA || System.Convert.ToInt32(dread["PAUSA_PRESENCA"]) != ListaControle[i].PAUSA_PRESENCA)
                                            {
                                                ListaControle[i].CRONO_PRESENCA = (DateTime)dread["CRONO_PRESENCA"];
                                                ListaControle[i].TEMPO_PRESENCA = (TimeSpan)dread["TEMPO_PRESENCA"];
                                                ListaControle[i].PAUSA_PRESENCA = System.Convert.ToInt32(dread["PAUSA_PRESENCA"]);
                                                /// ENVIAR ATUALIZACAO DO ESTADO DO CRONOMETRO
                                            }
                                        }
                                        else
                                        {
                                            ListaControle[i].CRONO_PRESENCA = (DateTime)dread["CRONO_PRESENCA"];
                                            ListaControle[i].TEMPO_PRESENCA = (TimeSpan)dread["TEMPO_PRESENCA"];
                                            ListaControle[i].PAUSA_PRESENCA = System.Convert.ToInt32(dread["PAUSA_PRESENCA"]);
                                        }

                                        // CONTROLA CRONO MATERIA
                                        if (System.Convert.ToInt32(dread["ID_VOTACAO"]) != 0)
                                        {
                                            if ((DateTime)dread["CRONO_MATERIA"] != ListaControle[i].CRONO_MATERIA || (TimeSpan)dread["TEMPO_MATERIA"] != ListaControle[i].TEMPO_MATERIA || System.Convert.ToInt32(dread["PAUSA_MATERIA"]) != ListaControle[i].PAUSA_MATERIA)
                                            {
                                                ListaControle[i].CRONO_MATERIA = (DateTime)dread["CRONO_MATERIA"];
                                                ListaControle[i].TEMPO_MATERIA = (TimeSpan)dread["TEMPO_MATERIA"];
                                                ListaControle[i].PAUSA_MATERIA = System.Convert.ToInt32(dread["PAUSA_MATERIA"]);
                                                /// ENVIAR ATUALIZACAO DO ESTADO DO CRONOMETRO
                                            }
                                        }
                                        else
                                        {
                                            ListaControle[i].CRONO_MATERIA = (DateTime)dread["CRONO_MATERIA"];
                                            ListaControle[i].TEMPO_MATERIA = (TimeSpan)dread["TEMPO_MATERIA"];
                                            ListaControle[i].PAUSA_MATERIA = System.Convert.ToInt32(dread["PAUSA_MATERIA"]);
                                            ListaControle[i].ID_VOTACAO = System.Convert.ToInt32(dread["ID_VOTACAO"]);
                                        }

                                        // CONTROLA CRONO APARTE
                                        if (System.Convert.ToInt32(dread["ID_APARTE"]) != 0)
                                        {
                                            if ((DateTime)dread["CRONO_APARTE"] != ListaControle[i].CRONO_APARTE || (TimeSpan)dread["TEMPO_APARTE"] != ListaControle[i].TEMPO_APARTE || System.Convert.ToInt32(dread["PAUSA_APARTE"]) != ListaControle[i].PAUSA_APARTE)
                                            {
                                                ListaControle[i].CRONO_APARTE = (DateTime)dread["CRONO_APARTE"];
                                                ListaControle[i].TEMPO_APARTE = (TimeSpan)dread["TEMPO_APARTE"];
                                                ListaControle[i].PAUSA_APARTE = System.Convert.ToInt32(dread["PAUSA_APARTE"]);
                                                /// ENVIAR ATUALIZACAO DO ESTADO DO CRONOMETRO
                                            }
                                        }
                                        else
                                        {
                                            ListaControle[i].CRONO_APARTE = (DateTime)dread["CRONO_APARTE"];
                                            ListaControle[i].TEMPO_APARTE = (TimeSpan)dread["TEMPO_APARTE"];
                                            ListaControle[i].PAUSA_APARTE = System.Convert.ToInt32(dread["PAUSA_APARTE"]);
                                        }
                                        #endregion

                                        // VERIFICAÇÃO ALTERAÇÃO NAS LISTAS

                                        if ((System.Convert.ToInt32(dread["CVOTACAO"]) != ListaControle[i].CVOTACAO))
                                        {
                                            ListaControle[i].CVOTACAO = System.Convert.ToInt32(dread["CVOTACAO"]);
                                        }

                                        if ((System.Convert.ToInt32(dread["CAPARTE"]) != ListaControle[i].CAPARTE))
                                        {
                                            ListaControle[i].CAPARTE = System.Convert.ToInt32(dread["CAPARTE"]);
                                        }

                                        if (System.Convert.ToInt32(dread["CORADORES"]) != ListaControle[i].CORADORES)
                                        {
                                            ListaControle[i].CORADORES = System.Convert.ToInt32(dread["CORADORES"]);
                                        }
                                        if (System.Convert.ToInt32(dread["CVOTACAOORADORES"]) != ListaControle[i].CVOTACAOORADORES)
                                        {
                                            ListaControle[i].CVOTACAOORADORES = System.Convert.ToInt32(dread["CVOTACAOORADORES"]);
                                        }

                                        // VERIFICAÇÃO DOS ESTADOS 
                                        if (System.Convert.ToInt32(dread["ID_SALA"]) != ListaControle[i].ID_SALA)
                                        {
                                            ListaControle[i].ID_SALA = System.Convert.ToInt32(dread["ID_SALA"]);
                                        }
                                        if (System.Convert.ToInt32(dread["ID_MODELO"]) != ListaControle[i].ID_MODELO)
                                        {
                                            ListaControle[i].ID_MODELO = System.Convert.ToInt32(dread["ID_MODELO"]);
                                        }
                                        if (System.Convert.ToInt32(dread["ID_REUNIAO"]) != ListaControle[i].ID_REUNIAO)
                                        {
                                            ListaControle[i].ID_REUNIAO = System.Convert.ToInt32(dread["ID_REUNIAO"]);
                                        }
                                        if (System.Convert.ToInt32(dread["ID_PARTE"]) != ListaControle[i].ID_PARTE)
                                        {
                                            ListaControle[i].ID_PARTE = System.Convert.ToInt32(dread["ID_PARTE"]);
                                        }
                                        if (System.Convert.ToInt32(dread["ID_RAPIDO"]) != ListaControle[i].ID_RAPIDO)
                                        {
                                            ListaControle[i].ID_RAPIDO = System.Convert.ToInt32(dread["ID_RAPIDO"]);
                                        }
                                        if (System.Convert.ToInt32(dread["ID_APARTE"]) != ListaControle[i].ID_APARTE)
                                        {
                                            ListaControle[i].ID_RAPIDO = System.Convert.ToInt32(dread["ID_RAPIDO"]);
                                        }
                                        if (System.Convert.ToString(dread["TIPOVOT"]) != ListaControle[i].TIPOVOT)
                                        {
                                            ListaControle[i].TIPOVOT = System.Convert.ToString(dread["TIPOVOT"]);
                                        }
                                        if (System.Convert.ToInt32(dread["ID_DELEGADO"]) != ListaControle[i].ID_DELEGADO)
                                        {
                                            ListaControle[i].ID_DELEGADO = System.Convert.ToInt32(dread["ID_DELEGADO"]);
                                        }
                                        if (System.Convert.ToInt32(dread["PRESENCA"]) != ListaControle[i].PRESENCA)
                                        {
                                            ListaControle[i].PRESENCA = System.Convert.ToInt32(dread["PRESENCA"]);
                                        }
                                        if (System.Convert.ToInt32(dread["DEBATE"]) != ListaControle[i].DEBATE)
                                        {
                                            ListaControle[i].DEBATE = System.Convert.ToInt32(dread["DEBATE"]);
                                        }
                                        if (System.Convert.ToInt32(dread["RESULTADO"]) != ListaControle[i].RESULTADO)
                                        {
                                            ListaControle[i].RESULTADO = System.Convert.ToInt32(dread["RESULTADO"]);
                                        }
                                        if (System.Convert.ToInt32(dread["ID_VOTACAO"]) != ListaControle[i].ID_VOTACAO)
                                        {
                                            ListaControle[i].ID_VOTACAO = System.Convert.ToInt32(dread["ID_VOTACAO"]);
                                        }
                                        if (System.Convert.ToString(dread["NOMINAL"]) != ListaControle[i].NOMINAL)
                                        {
                                            ListaControle[i].NOMINAL = System.Convert.ToString(dread["NOMINAL"]);
                                        }
                                        if (System.Convert.ToInt32(dread["ID_APARTE"]) != ListaControle[i].ID_APARTE)
                                        {
                                            ListaControle[i].ID_APARTE = System.Convert.ToInt32(dread["ID_APARTE"]);
                                        }
                                        if (System.Convert.ToInt32(dread["ESTADO"]) != ListaControle[i].ESTADO_ATUAL)
                                        {
                                            ListaControle[i].ESTADO_ATUAL = System.Convert.ToInt32(dread["ESTADO"]);
                                        }
                                        i++;
                                    }
                                }
                                dread.Close();
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "GetListaEstadoOperacao " + ex.Message, 3, 0, null));
            }
        }

        private bool GetPauta(int id_sala, int id_reuniao)
        {
            bool change = false;
            try
            {

                int index = FindIndex(id_sala);

                PautaDTO pauta = new PautaDTO { parts = new List<ParteDTO>() };

                using (FbConnection con = new FbConnection(connectionstring))
                {

                    con.Open();
                    using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (FbCommand FbCom = new FbCommand($"SELECT ID_PARTE, NOME FROM REUNIAOPARTES WHERE ID_REUNIAO = {id_reuniao};", con, transaction))
                        {
                            FbCom.CommandTimeout = 30;
                            using (FbDataReader dread = FbCom.ExecuteReader())
                            {
                                while (dread.Read())
                                {
                                    pauta.parts.Add(new ParteDTO
                                    {
                                        id = Convert.ToInt32(dread[0]),
                                        name = Convert.ToString(dread[1]),
                                        speaker = true,
                                        subjects = new List<MateriaDTO>()
                                    });
                                }
                            }
                        }
                    }

                    int aux = 0;
                    foreach (ParteDTO parte in pauta.parts)
                    {
                        List<int> listaMaterias = new List<int>();

                        using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            using (FbCommand FbCom = new FbCommand($"SELECT ID_VOTACAO FROM PARTESVOTACAO WHERE ID_PARTE = {parte.id};", con, transaction))
                            {
                                FbCom.CommandTimeout = 30;
                                using (FbDataReader dread = FbCom.ExecuteReader())
                                {
                                    while (dread.Read())
                                    {
                                        listaMaterias.Add(Convert.ToInt32(dread[0]));
                                    }
                                }
                            }
                        }

                        int auxMateria = 0;
                        pauta.parts[aux].subjects = new List<MateriaDTO>();

                        foreach (int materia in listaMaterias)
                        {
                            using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                            {
                                using (FbCommand FbCom = new FbCommand($"SELECT ID_VOTACAO, VOTACAO, DESCRICAO, COD_MATERIA FROM CADVOTACOES WHERE ID_VOTACAO = {materia};", con, transaction))
                                {
                                    FbCom.CommandTimeout = 30;
                                    using (FbDataReader dread = FbCom.ExecuteReader())
                                    {
                                        while (dread.Read())
                                        {
                                            pauta.parts[aux].subjects.Add(new MateriaDTO
                                            {
                                                id = Convert.ToInt32(dread[0]),
                                                name = Convert.ToString(dread[1]),
                                                description = Convert.ToString(dread[2]),
                                                acronym = Convert.ToString(dread[3]),
                                                authors = new List<AuthorDTO>()
                                            });
                                        }
                                    }
                                }
                            }

                            using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                            {
                                using (FbCommand FbCom = new FbCommand($"SELECT v.ID_DELEGADO, c.NOME, c.PARTIDO  FROM VOTACAOAUTORES v JOIN CADDELEGADOS c ON c.ID_DELEGADO = v.ID_DELEGADO WHERE ID_VOTACAO = {materia};", con, transaction))
                                {
                                    FbCom.CommandTimeout = 30;
                                    using (FbDataReader dread = FbCom.ExecuteReader())
                                    {
                                        while (dread.Read())
                                        {
                                            if (pauta.parts[aux].subjects[auxMateria] is null)
                                                pauta.parts[aux].subjects.Add(new MateriaDTO());

                                            if (pauta.parts[aux].subjects[auxMateria].authors is null)
                                                pauta.parts[aux].subjects[auxMateria].authors = new List<AuthorDTO>();

                                            pauta.parts[aux].subjects[auxMateria].authors.Add(new AuthorDTO
                                            {
                                                name = Convert.ToString(dread[1]),
                                                party = Convert.ToString(dread[2])
                                            });
                                        }
                                    }
                                }
                                auxMateria++;
                            }
                        }

                        listaMaterias.Clear();
                        aux++;
                    }
                    con.Close();
                }

                if (pauta.parts.Count != ListaControle[index].PAUTA.parts.Count)
                {
                    ListaControle[index].PAUTA.parts = pauta.parts;
                    return true;
                }
                else
                {
                    for (int i = 0; i < pauta.parts.Count; i++)
                    {
                        for (int j = 0; j < pauta.parts[i].subjects.Count; j++)
                        {
                            if (pauta.parts.Count > 0 && ListaControle[index].PAUTA.parts.Count > 0)
                            {
                                if (pauta.parts[i].subjects.Count != ListaControle[index].PAUTA.parts[i].subjects.Count)
                                {
                                    change = true;
                                }
                            }
                        }
                    }
                }
                if (change)
                {
                    ListaControle[index].PAUTA.parts = pauta.parts;
                    return true;
                }
                else
                {
                    ListaControle[index].PAUTA.parts = pauta.parts;
                    return false;
                }
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "GetPauta: " + ex.Message, 3, 0, null));
            }

            return false;
        }

        private bool GetListaPresenca(int id_sala)
        {
            int index = FindIndex(id_sala);
            bool retorno = false;

            List<Presence> listPresenca = new List<Presence>();

            using (FbConnection con = new FbConnection(connectionstring))
            {
                string sql = $"SELECT PRESENTES, NOME, PARTIDO, CASE WHEN COD_MIC IS NULL THEN 0 ELSE COD_MIC END AS COD_MIC FROM VPRESENCA WHERE ID_SALA = {id_sala};";

                con.Open();
                using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    using (FbCommand FbCom = new FbCommand(sql, con, transaction))
                    {
                        FbCom.CommandTimeout = 30;
                        using (FbDataReader dread = FbCom.ExecuteReader())
                        {
                            Presence aux;
                            while (dread.Read())
                            {
                                aux = new Presence();

                                if (dread[0] is null)
                                {
                                    aux.status = StatusPresenca.Ausente;
                                }
                                else
                                {
                                    int codMic = Convert.ToInt32(dread[3]);
                                    if (dread[0].ToString() is "SIM" && codMic < 256)
                                    {
                                        aux.status = StatusPresenca.Local;
                                    }
                                    else if (codMic > 256)
                                    {
                                        aux.status = StatusPresenca.Remoto;
                                    }
                                    else
                                    {
                                        aux.status = StatusPresenca.Ausente;
                                    }
                                }

                                if (!(dread[1] is null) && !(dread[2] is null))
                                {
                                    aux.name = dread[1].ToString();
                                    aux.party = dread[2].ToString();
                                }

                                listPresenca.Add(aux);
                            }
                        }
                    }
                    transaction.Commit();
                }
                con.Close();

                if (listPresenca.Count != ListaControle[index].ListaPresenca.Count)
                {
                    ListaControle[index].ListaPresenca = listPresenca;
                    retorno = true;
                }

                int i = 0;
                foreach (Presence item in ListaControle[index].ListaPresenca)
                {
                    if (item.name != listPresenca[i].name)
                    {
                        item.name = listPresenca[i].name;
                        retorno = true;
                    }
                    if (item.status != listPresenca[i].status)
                    {
                        item.status = listPresenca[i].status;
                        retorno = true;
                    }
                    if (item.party != listPresenca[i].party)
                    {
                        item.party = listPresenca[i].party;
                        retorno = true;
                    }
                    i++;
                }

                return retorno;
            }
        }
    }
}