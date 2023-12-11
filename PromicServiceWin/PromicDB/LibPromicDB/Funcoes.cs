using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibPromicDB
{
    public partial class PromicDB
    {
        // EVENTO LOG
        public event EnviaLogDelegate OnLog;
        public delegate void EnviaLogDelegate(HReturn retorno);
        protected virtual void EnviaLog(HReturn retorno)
        {
            OnLog?.Invoke(retorno);
        }

        public int FindIndex(int id_sala)
        {
            for (int i = 0; i <= ListaControle.Count - 1; i++)
            {
                if (ListaControle[i].ID_SALA == id_sala)
                {
                    return i;
                }
            }
            return -1;
        }

        public void RecebeVoto(int id_sala, int id_delegado, int voto)
        {
            int index = FindIndex(id_sala);
            string nome = ListaControle[index].ListaVotoNominal.First(x => x.ID_DELEGADO == id_delegado).NOME;
            string partido = ListaControle[index].ListaVotoNominal.First(x => x.ID_DELEGADO == id_delegado).PARTIDO;
            int ID_RAPIDO = ListaControle[index].ID_RAPIDO;
            int ID_VOTACAO = ListaControle[index].ID_VOTACAO;
            int TURNO = 1;
            string SECRETO = ListaControle[index].VotacaoTotais.SECRETO;

            if (ID_VOTACAO != 0) TURNO = ListaControle[index].VotacaoTotais.TURNO;

            if (ID_RAPIDO > 0)
            {
                if (SECRETO == "N")
                {
                    AlteraTabela("UPDATE OR INSERT INTO RAPIDONOMINAL(ID_RAPIDO, ID_VOTACAO, TURNO, ID_DELEGADO, NOME, PARTIDO, VOTO) " +
                        "VALUES(" + ID_RAPIDO.ToString() + ", " + ID_VOTACAO.ToString() + ", " + TURNO.ToString() + ", " + id_delegado.ToString() + ", '" + nome + "', '" + partido + "'," + voto.ToString() + ") " +
                        "matching(ID_RAPIDO, ID_DELEGADO);");
                }
                AlteraTabela("UPDATE OR INSERT INTO RAPIDORESULTADO(ID_RAPIDO, ID_VOTACAO, TURNO, VOTO" + voto.ToString() + ") " +
                    "VALUES(" + ID_RAPIDO.ToString() + ", " + ID_VOTACAO.ToString() + ", " + TURNO.ToString() + ", (SELECT VOTO" + voto.ToString() + " + 1 FROM RAPIDORESULTADO WHERE ID_RAPIDO = " + ID_RAPIDO.ToString() + ")) " +
                    "matching(ID_RAPIDO)");
            }
            else
            {
                string TIPOVOT = ListaControle[index].TIPOVOT;
                string item = RetornaColuna("SELECT ITEM FROM CONFIGURACAO WHERE ID_CONFIG = " + id_sala + "");
                int ITEM = 0;
                ITEM = string.IsNullOrEmpty(item) ? 0 : Convert.ToInt32(item);

                if (SECRETO == "N")
                {
                    string mensagem = "UPDATE OR INSERT INTO VOTACAONOMINAL (ID_VOTACAO, ID_DELEGADO, NOME, PARTIDO, VOTO, TURNO, TIPOVOT, ITEM) " +
                     $"VALUES({ID_VOTACAO}, {id_delegado}, '{nome}', '{partido}', {voto}, {TURNO}, '{TIPOVOT}', {item}) " +
                     "matching (ID_VOTACAO, ID_DELEGADO, NOME, PARTIDO, VOTO, TURNO, TIPOVOT, ITEM)";
                    AlteraTabela(mensagem);
                }

                AlteraTabela("UPDATE OR INSERT INTO VOTACAORESULTADO (ID_VOTACAO, VOTO" + voto.ToString() + ", TURNO, TIPOVOT, ITEM) " +
                    "VALUES (" + ID_VOTACAO + ", (SELECT VOTO" + voto.ToString() + " + 1 FROM VOTACAORESULTADO WHERE ID_VOTACAO = " + ID_VOTACAO.ToString() + " AND TURNO = " + TURNO.ToString() + " " +
                    "AND TIPOVOT = '" + TIPOVOT + "' AND ITEM = " + ITEM + "), " + TURNO.ToString() + ", '" + TIPOVOT + "', " + ITEM + ") " +
                    "matching (ID_VOTACAO, TURNO, TIPOVOT, ITEM)");
            }
        }
        public void TempoOradorEncerrado(int id_delegado, int id_sala)
        {
            string cmd = $"UPDATE CONFIGURACAO SET ID_DELEGADO = 0 WHERE ID_CONFIG = {id_sala};";
            try
            {
                using (FbConnection con = new FbConnection(connectionstring))
                {
                    con.Open();
                    using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (FbCommand Fbcom = new FbCommand(cmd, con, transaction))
                        {
                            try
                            {
                                Fbcom.ExecuteNonQuery();
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                EnviaLog(new HReturn(false, "Falha finalizar tempo orador (" + cmd + ") " + ex.Message, 0, 0, null));
                                transaction.Rollback();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "Falha finalizar tempo orador (" + cmd + ") " + ex.Message, 0, 0, null));
            }
        }
        public int BuscaDelegadoPorIdOrador(int id_sala, int id_orador, EstadoOperacao estadoAtual)
        {
            int value = 0;

            string cmd;
            if (estadoAtual == EstadoOperacao.TempoOradorParte)
            {
                cmd = $"SELECT ID_DELEGADO FROM PARTESORADORES WHERE ID_ORADOR = {id_orador};";
            }
            else
            {
                cmd = $"SELECT ID_DELEGADO FROM VOTACAOORADORES WHERE ID_ORADOR = {id_orador};";
            }
            try
            {
                using (FbConnection con = new FbConnection(connectionstring))
                {
                    con.Open();
                    using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (FbCommand Fbcom = new FbCommand(cmd, con, transaction))
                        {
                            using (FbDataReader dread = Fbcom.ExecuteReader())
                            {
                                try
                                {
                                    while (dread.Read())
                                    {
                                        value = System.Convert.ToInt32(dread["ID_DELEGADO"]);
                                    }
                                    transaction.Commit();
                                }
                                catch (Exception ex)
                                {
                                    EnviaLog(new HReturn(false, "Falha buscar orador (" + cmd + ") " + ex.Message, 0, 0, null));
                                    transaction.Rollback();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "Falha  buscar orador (" + cmd + ") " + ex.Message, 0, 0, null));
            }
            return value;
        }
        public bool InscricaoOrador(int id_sala, int id_delegado)
        {
            try
            {
                int index = FindIndex(id_sala);
                int idparte = ListaControle[index].ID_PARTE;
                int idvotacao = ListaControle[index].ID_VOTACAO;
                int mic = Convert.ToInt32(RetornaColuna("SELECT COD_MIC FROM MODELOMIC WHERE ID_DELEGADO = " + id_delegado + " AND ID_SALA = " + id_sala));
                bool result = false;

                if (idparte != 0 || idvotacao != 0)
                {
                    string tempo = "00:05:00";
                    TimeSpan tempoSpan = RetornaTempo(@"select tipoparte.tempoorador from configuracao
                                   inner join reuniaopartes on (configuracao.id_parte = reuniaopartes.id_parte)
                                   inner join tipoparte on (reuniaopartes.id_tipoparte = tipoparte.id_tipoparte)
                                   where (id_config = " + id_sala + ");");

                    if (tempoSpan != default(TimeSpan))
                        tempo = tempoSpan.ToString();

                    result = ExecutaProcedure("EXECUTE PROCEDURE SPCINSEREORADOR (" + mic + ", '" + tempo + "', 'ORADOR', " + id_sala + ");");

                    return result;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "CadastroOrador " + ex.Message, 2, 0, null));
                return false;
            }
        }
        public bool DesincreveOrador(int id_sala, int id_delegado)
        {
            try
            {
                int index = FindIndex(id_sala);
                int id_parte = ListaControle[index].ID_PARTE;
                int id_votacao = ListaControle[index].ID_VOTACAO;

                if (id_parte != 0 || id_votacao != 0)
                {
                    using (FbConnection con = new FbConnection(connectionstring))
                    {
                        con.Open();
                        using (FbTransaction transaction = con.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                        {
                            using (FbCommand Fbcom = new FbCommand($"DELETE FROM PARTESORADORES  WHERE ID_DELEGADO = {id_delegado} AND ID_PARTE = {id_parte};", con, transaction))
                            {
                                Fbcom.CommandType = CommandType.Text;
                                Fbcom.ExecuteNonQuery();
                            }
                            using (FbCommand Fbcom = new FbCommand($"DELETE FROM votacaooradores  WHERE ID_DELEGADO = {id_delegado} AND ID_VOTACAO = {id_votacao};", con, transaction))
                            {
                                Fbcom.CommandType = CommandType.Text;
                                Fbcom.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        con.Close();
                        return true;
                    }
                }
                else return false;
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "CadastroOrador EXECUTE PROCEDURE " + ex.Message, 1, 0, null));
                return false;
            }
        }
        public string RetornaColuna(string CMDTabela)
        {
            string value = "";
            try
            {
                using (FbConnection con = new FbConnection(connectionstring))
                {
                    con.Open();
                    using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (FbCommand Fbcom = new FbCommand(CMDTabela, con, transaction))
                        {
                            Fbcom.CommandType = CommandType.Text;
                            try
                            {
                                object retorno = Fbcom.ExecuteScalar();
                                if (retorno != null && retorno != DBNull.Value)
                                {
                                    value = Convert.ToString(retorno);
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                EnviaLog(new HReturn(false, "Falha Commit RetornaColuna (" + CMDTabela + ") " + ex.Message, 0, 0, null));
                                transaction.Rollback();
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "Falha Geral RetornaColuna (" + CMDTabela + ") " + ex.Message, 0, 0, null));
            }
            return value;
        }

        public bool AlteraTabela(string CMDTabela)
        {
            bool value = false;

            try
            {
                using (FbConnection con = new FbConnection(connectionstring))
                {
                    con.Open();
                    using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (FbCommand Fbcom = new FbCommand(CMDTabela, con, transaction))
                        {
                            try
                            {
                                Fbcom.ExecuteNonQuery();
                                transaction.Commit();
                                value = true;
                            }
                            catch (Exception ex)
                            {
                                EnviaLog(new HReturn(false, "Falha Commit AlteraTabela (" + CMDTabela + ") " + ex.Message, 0, 0, null));
                                transaction.Rollback();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "Falha Geral AlteraTabela (" + CMDTabela + ") " + ex.Message, 0, 0, null));
            }
            return value;
        }

        public bool ExecutaProcedure(string CMDTabela)
        {
            bool result = false;

            try
            {
                using (FbConnection con = new FbConnection(connectionstring))
                {
                    con.Open();
                    using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (FbCommand Fbcom = new FbCommand(CMDTabela, con, transaction))
                        {
                            Fbcom.CommandType = CommandType.StoredProcedure;
                            try
                            {
                                Fbcom.ExecuteNonQuery();
                                transaction.Commit();
                                result = true;
                            }
                            catch (Exception ex)
                            {
                                EnviaLog(new HReturn(false, "Falha Commit ExecutaProcedure (" + CMDTabela + ") " + ex.Message, 0, 0, null));
                                transaction.Rollback();
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "Falha Geral ExecutaProcedure (" + CMDTabela + ") " + ex.Message, 0, 0, null));
            }
            return result;
        }

        public TimeSpan RetornaTempo(string CMDTabela)
        {
            TimeSpan value = default(TimeSpan);

            try
            {
                using (FbConnection con = new FbConnection(connectionstring))
                {
                    con.Open();
                    using (FbTransaction transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (FbCommand Fbcom = new FbCommand(CMDTabela, con, transaction))
                        {
                            Fbcom.CommandType = CommandType.Text;
                            try
                            {
                                object retorno = Fbcom.ExecuteScalar();
                                if (retorno != null && retorno != DBNull.Value)
                                {
                                    value = (TimeSpan)retorno;
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                EnviaLog(new HReturn(false, "Falha Commit RetornaTempo (" + CMDTabela + ") " + ex.Message, 0, 0, null));
                                transaction.Rollback();
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EnviaLog(new HReturn(false, "Falha Geral RetornaTempo (" + CMDTabela + ") " + ex.Message, 0, 0, null));
            }
            return value;
        }


    }
}
