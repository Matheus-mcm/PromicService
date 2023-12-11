using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using static PromicnetIntegration.PromicWebSocket;

namespace LibPromicDB
{
    public partial class PromicDB
    {
        private string SqlVotacaoNominal = "";
        private string SqlVotacaoTotais = "";

        // EVENTO VOTACAO GERAL
        public event VotacaoTotaisDelegate OnListaVotacaoGeral;
        public delegate void VotacaoTotaisDelegate(int index, int id_sala, VotacaoTotais totais);
        protected virtual void EnviaListaVotacaoTotais(int index, int id_sala, VotacaoTotais totais)
        {
            OnListaVotacaoGeral?.Invoke(index, id_sala, totais);
        }

        // EVENTO LISTA VOTOS NOMINAIS
        public event VotoNominalDelegate OnListaVotoNominal;
        public delegate void VotoNominalDelegate(int index, int id_sala, List<VotoNominal> lista);
        protected virtual void EnviaListaVotoNominal(int index, int id_sala, List<VotoNominal> lista)
        {
            OnListaVotoNominal?.Invoke(index, id_sala, lista);
        }

        /// <summary>
        /// Obtém a lista com as informações detalhadas de uma votação
        /// </summary>
        /// <param name="index">Posição da sala na lista</param>
        /// <param name="id_sala">ID da sala</param>
        /// <param name="tabela">Qual o modo da tabela</param>
        /// <returns></returns>
        private bool GetListaVotacaoTotais(int index, int id_sala, Tabela tabela)
        {

            bool result = false;

            try
            {

                /// esta recriando a lista??
                if (ListaControle[index].ESTADO_ATUAL != ListaControle[index].ESTADO_ANTERIOR)
                {
                    ListaControle[index].VotacaoTotais = new VotacaoTotais();
                }


                if (tabela == Tabela.VotacaoRapida)
                {
                    SqlVotacaoTotais = "SELECT ID_VOTACAO, TITULO, EMENTA, VOTO1, VOTO2, VOTO3, VOTO4, VOTO5, TOTALVOTOS, RESPOSTA, TURNO, RESULTADO, RESULTADOSTR, SECRETO, REALS, MINERVA, QDMINERVA, (SELECT COUNT(*) FROM VRAPIDORESULTADO WHERE ID_SALA = " + id_sala.ToString() + ") AS QTD "
                                    + " FROM VRAPIDORESULTADO WHERE ID_SALA =  " + id_sala.ToString() + "";
                }
                else
                {
                    SqlVotacaoTotais = "SELECT ID_VOTACAO, TITULO, EMENTA, VOTO1, VOTO2, VOTO3, VOTO4, VOTO5, TOTALVOTOS, RESPOSTA, TURNO, RESULTADO, RESULTADOSTR, SECRETO, REALS, MINERVA, QDMINERVA, (SELECT COUNT(*) FROM VVOTACAORESULTADO WHERE ID_SALA = " + id_sala.ToString() + ") AS QTD "
                    + " FROM VVOTACAORESULTADO WHERE ID_SALA = " + id_sala.ToString() + ";";
                }


                int i = 0;


                using (FbConnection con = new FbConnection(connectionstring))
                {
                    con.Open();
                    using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (FbCommand FbCom = new FbCommand(SqlVotacaoTotais, con, transaction))
                        {
                            FbCom.CommandTimeout = 5;
                            using (FbDataReader dread = FbCom.ExecuteReader())
                            {
                                if (dread.HasRows)
                                {
                                    while (dread.Read())



                                    {
                                        // Se ID_VOTACAO for 0, então não tem nenhuma votação em andamento
                                        // A primeira consulta foi feita com i = 0, que retorna dados da primeira sala (provavelmente).
                                        // se a leitura Read() não retornar mais nada, encerra esta função com false
                                        if (System.Convert.ToInt32(dread["ID_VOTACAO"]) != 0)
                                        {
                                            // ListaControle tem o tamanho da quantidade de salas. 
                                            // Como para entrar aqui o ID_VOTACAO é != de 0, então guarda o ID da votação
                                            ListaControle[index].ID_VOTACAO = Convert.ToInt32(dread["ID_VOTACAO"]);

                                            // Verifica se houve alteração no tipo de 
                                            if (ListaControle[index].VotacaoTotais.RESPOSTA != System.Convert.ToInt32(dread["RESPOSTA"]))
                                            {
                                                ListaControle[index].VotacaoTotais.RESPOSTA = System.Convert.ToInt32(dread["RESPOSTA"]);
                                                result = true;
                                                /// ALTERAÇÃO DO TIPO DE RESPOSTA
                                            }

                                            if (ListaControle[index].VotacaoTotais.SECRETO != (dread["SECRETO"] == null ? "N" : dread["SECRETO"].ToString()) ||
                                                ListaControle[index].VotacaoTotais.REAL != (dread["REALS"] == null ? "S" : dread["REALS"].ToString()) ||
                                                ListaControle[index].VotacaoTotais.RESULTADO != System.Convert.ToInt32(dread["RESULTADO"]) ||
                                                ListaControle[index].VotacaoTotais.RESULTADOSTR != (dread["RESULTADOSTR"] == null ? "" : dread["RESULTADOSTR"].ToString()) ||
                                                ListaControle[index].VotacaoTotais.TITULO != (dread["TITULO"] == null ? "" : dread["TITULO"].ToString()) ||
                                                ListaControle[index].VotacaoTotais.EMENTA != (dread["EMENTA"] == null ? "" : dread["EMENTA"].ToString()) ||
                                                ListaControle[index].VotacaoTotais.TOTALVOTOS != System.Convert.ToInt32(dread["TOTALVOTOS"]))
                                            {
                                                ListaControle[index].VotacaoTotais.SECRETO = (dread["SECRETO"] == null ? "N" : dread["SECRETO"].ToString());
                                                ListaControle[index].VotacaoTotais.REAL = (dread["REALS"] == null ? "S" : dread["REALS"].ToString());
                                                ListaControle[index].VotacaoTotais.RESULTADO = System.Convert.ToInt32(dread["RESULTADO"]);
                                                ListaControle[index].VotacaoTotais.RESULTADOSTR = (dread["RESULTADOSTR"] == null ? "" : dread["RESULTADOSTR"].ToString());
                                                ListaControle[index].VotacaoTotais.TITULO = (dread["TITULO"] == null ? "" : dread["TITULO"].ToString());
                                                ListaControle[index].VotacaoTotais.EMENTA = (dread["EMENTA"] == null ? "" : dread["EMENTA"].ToString());
                                                ListaControle[index].VotacaoTotais.TOTALVOTOS = System.Convert.ToInt32(dread["TOTALVOTOS"]);
                                                result = true;
                                                // ALTERAÇÃO DA EXIBIÇÃO
                                            }


                                            if (ListaControle[index].VotacaoTotais.ID_VOTACAO != System.Convert.ToInt32(dread["ID_VOTACAO"]) ||
                                                ListaControle[index].VotacaoTotais.VOTO1 != System.Convert.ToInt32(dread["VOTO1"]) ||
                                                ListaControle[index].VotacaoTotais.VOTO2 != System.Convert.ToInt32(dread["VOTO2"]) ||
                                                ListaControle[index].VotacaoTotais.VOTO3 != System.Convert.ToInt32(dread["VOTO3"]) ||
                                                ListaControle[index].VotacaoTotais.VOTO4 != System.Convert.ToInt32(dread["VOTO4"]) ||
                                                ListaControle[index].VotacaoTotais.VOTO5 != System.Convert.ToInt32(dread["VOTO5"]) ||
                                                ListaControle[index].VotacaoTotais.MINERVA != (dread["MINERVA"] == null ? "N" : dread["MINERVA"].ToString()) ||
                                                ListaControle[index].VotacaoTotais.TURNO != System.Convert.ToInt32(dread["TURNO"]))
                                            {
                                                ListaControle[index].VotacaoTotais.ID_VOTACAO = System.Convert.ToInt32(dread["ID_VOTACAO"]);
                                                ListaControle[index].VotacaoTotais.VOTO1 = System.Convert.ToInt32(dread["VOTO1"]);
                                                ListaControle[index].VotacaoTotais.VOTO2 = System.Convert.ToInt32(dread["VOTO2"]);
                                                ListaControle[index].VotacaoTotais.VOTO3 = System.Convert.ToInt32(dread["VOTO3"]);
                                                ListaControle[index].VotacaoTotais.VOTO4 = System.Convert.ToInt32(dread["VOTO4"]);
                                                ListaControle[index].VotacaoTotais.VOTO5 = System.Convert.ToInt32(dread["VOTO5"]);
                                                ListaControle[index].VotacaoTotais.MINERVA = (dread["MINERVA"] == null ? "N" : dread["MINERVA"].ToString());
                                                ListaControle[index].VotacaoTotais.TURNO = System.Convert.ToInt32(dread["TURNO"]);

                                                result = true;
                                                // ALTERAÇÃO DOS VOTOS
                                            }
                                        }
                                        i += 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "GetListaVotacaoTotais, INDEX: " + index + ", ID_SALA: " + id_sala + " TABELA: " + tabela.ToString() + " - " + ex.Message, 3, 0, null));
            }


            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="id_sala"></param>
        /// <param name="tabela"></param>
        /// <returns></returns>
        private bool GetListaVotosNominais(int index, int id_sala, Tabela tabela)
        {

            bool result = false;

            try
            {
                if (ListaControle[index].ListaVotoNominal is null)
                {
                    ListaControle[index].ListaVotoNominal = new List<VotoNominal>();
                }

                if (ListaControle[index].VotacaoTotais.RESPOSTA == 0 || ListaControle[index].VotacaoTotais.RESPOSTA == 3)
                {
                    if (tabela == Tabela.VotacaoRapida)
                    {
                        SqlVotacaoNominal = " SELECT ID_DELEGADO, NOME, PARTIDO, VOTO, LOGADO, DEVEVOTAR,(SELECT COUNT(*) FROM VRAPIDONOMINAL WHERE ID_SALA = " + id_sala.ToString() + ") AS QTD"
                                            + " FROM VRAPIDONOMINAL WHERE ID_SALA = " + id_sala.ToString() + " ORDER BY NOME ";
                    }
                    else
                    {
                        //if (ListaControle[index].VotacaoTotais.RESPOSTA == 4 || ListaControle[index].VotacaoTotais.RESPOSTA == 5)
                        //{
                        //    SqlVotacaoNominal = "  SELECT ID_DELEGADO, NOME, PARTIDO, VOTO, RESPOSTA, (SELECT COUNT(*) FROM VVOTACAONOMINALCHKNUM WHERE ID_SALA = @idsala) AS QTD"
                        //                       + " FROM VVOTACAONOMINALCHKNUM WHERE ID_SALA = @idsala ORDER BY NOME";
                        //}
                        //else
                        //{
                        SqlVotacaoNominal = " SELECT ID_DELEGADO, NOME, PARTIDO, VOTO, LOGADO, DEVEVOTAR, (Select COUNT(*) FROM VVOTACAONOMINAL WHERE ID_SALA = " + id_sala.ToString() + ") As QTD"
                                        + " FROM VVOTACAONOMINAL WHERE ID_SALA = " + id_sala.ToString() + " ORDER BY NOME;";
                        //}
                    }


                    int DeveVotar = 0;
                    int i = 0;

                    using (FbConnection con = new FbConnection(connectionstring))
                    {
                        con.Open();
                        using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            using (FbCommand FbCom = new FbCommand(SqlVotacaoNominal, con, transaction))
                            {
                                FbCom.CommandTimeout = 5;
                                using (FbDataReader dread = FbCom.ExecuteReader())
                                {
                                    if (dread.HasRows)
                                    {
                                        //halinhas b
                                        while (dread.Read())
                                        {

                                            if (ListaControle[index].ListaVotoNominal.Count > System.Convert.ToInt32(dread["QTD"]))
                                            {
                                                ListaControle[index].ListaVotoNominal.Clear();
                                            }

                                            if (ListaControle[index].ListaVotoNominal.Count == i)
                                            {
                                                ListaControle[index].ListaVotoNominal.Add(new VotoNominal());
                                            }

                                            if (ListaControle[index].ListaVotoNominal.Count >= (i + 1))
                                            {
                                                if (System.Convert.ToInt32(dread["ID_DELEGADO"]) != ListaControle[index].ListaVotoNominal[i].ID_DELEGADO ||
                                                    (dread["NOME"] == null ? "" : dread["NOME"].ToString()) != ListaControle[index].ListaVotoNominal[i].NOME ||
                                                    (dread["PARTIDO"] == null ? "" : dread["PARTIDO"].ToString()) != ListaControle[index].ListaVotoNominal[i].PARTIDO ||
                                                    (dread["VOTO"] == null ? "" : dread["VOTO"].ToString()) != ListaControle[index].ListaVotoNominal[i].VOTO)
                                                {
                                                    ListaControle[index].ListaVotoNominal[i].ID_DELEGADO = System.Convert.ToInt32(dread["ID_DELEGADO"]);
                                                    ListaControle[index].ListaVotoNominal[i].NOME = (dread["NOME"] == null ? "" : dread["NOME"].ToString());
                                                    ListaControle[index].ListaVotoNominal[i].PARTIDO = (dread["PARTIDO"] == null ? "" : dread["PARTIDO"].ToString());
                                                    ListaControle[index].ListaVotoNominal[i].VOTO = (dread["VOTO"] == null ? "" : dread["VOTO"].ToString());
                                                    ListaControle[index].ListaVotoNominal[i].VOTOX = "---";
                                                    ListaControle[index].VotacaoTotais.VOTOSNECES = System.Convert.ToInt32(dread["QTD"]);

                                                    result = true;
                                                }

                                                if ((dread["DEVEVOTAR"] == null ? "N" : dread["DEVEVOTAR"].ToString()) == "S")
                                                {
                                                    ListaControle[index].ListaVotoNominal[i].DEVEVOTAR = "S";
                                                    DeveVotar++;
                                                }
                                                else
                                                {
                                                    ListaControle[index].ListaVotoNominal[i].DEVEVOTAR = "N";
                                                }
                                            }
                                            i += 1;
                                        }

                                    }
                                    ListaControle[index].VotacaoTotais.VOTOSNECES = DeveVotar;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "GetListaVotosNominais, INDEX: " + index + ", ID_SALA: " + id_sala + ", TABELA: " + tabela.ToString() + " - " + ex.Message, 3, 0, null));
            }
            return result;
        }
    }
}
