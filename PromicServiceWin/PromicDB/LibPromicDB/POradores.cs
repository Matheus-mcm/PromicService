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
        public event ListaOradoresDelegate OnListaOradores;
        public delegate void ListaOradoresDelegate(int index, int id_sala, List<Oradores> lista);
        protected virtual void EnviaListaOradores(int index, int id_sala, List<Oradores> lista)
        {
            OnListaOradores?.Invoke(index, id_sala, lista);
        }

        /// <summary>
        /// Obtém a lista com as informações detalhadas de uma votação
        /// </summary>
        /// <param name="index">Posição da sala na lista</param>
        /// <param name="id_sala">ID da sala</param>
        /// <param name="tabela">Qual o modo da tabela</param>
        /// <returns></returns>
        private bool GetListaOradores(int id_sala)
        {
            int index = FindIndex(id_sala);

            try
            {
                List<Oradores> aux = new List<Oradores>();

                string cmd = "";

                if (ListaControle[index].ESTADO_ATUAL == 5 ||
                    ListaControle[index].ESTADO_ATUAL == 6)
                {
                    cmd = $"SELECT ID_ORADOR, ID_DELEGADO, NOME, DURACAO FROM VORADORESVOTACAO;";
                }
                else
                {
                    cmd = $"SELECT ID_ORADOR, ID_DELEGADO, NOME, DURACAO FROM VORADORESPARTE;";
                }

                using (FbConnection con = new FbConnection(connectionstring))
                {
                    con.Open();
                    using (FbTransaction transaction = con.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        using (FbCommand FbCom = new FbCommand(cmd, con, transaction))
                        {
                            FbCom.CommandTimeout = 30;
                            using (FbDataReader dread = FbCom.ExecuteReader())
                            {
                                while (dread.Read())
                                {
                                    aux.Add(new Oradores
                                    {
                                        ID_DELEGADO = Convert.ToInt32(dread["ID_DELEGADO"]),
                                        ID_ORADOR = Convert.ToInt32(dread["ID_ORADOR"]),
                                        NOME = dread["NOME"].ToString(),
                                        PARTIDO = RetornaColuna($"SELECT PARTIDO FROM CADDELEGADOS WHERE ID_DELEGADO = {Convert.ToInt32(dread["ID_DELEGADO"])};"),
                                        TEMPO = Convert.ToInt32(((TimeSpan)dread["DURACAO"]).TotalSeconds)
                                    });
                                }
                            }
                        }
                        transaction.Commit();
                    }
                }


                if (ListaControle[index].ListaOradores.Count != aux.Count)
                {
                    ListaControle[index].ListaOradores = aux;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
