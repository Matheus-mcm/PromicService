using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPromicDB.Entidades
{
    public class Config
    {
        /// <summary>
        /// IP do Banco de dados. O valor padrão é "local".
        /// </summary>
        public string IpDB { get; set; } = "local";
        /// <summary>
        /// Porta de comunicação do banco de dados. O valor padrão é 3050
        /// </summary>
        public int Porta { get; set; } = 3050;
        /// <summary>
        /// Ambiente que o serviço está utilizando. Sendo eles: Dev, Test e Prod.
        /// </summary>
        public string Ambiente { get; set; } = String.Empty;
        /// <summary>
        /// URL do Promicnet para cadastros. Por exemplo, cadastro de sessões e de delegados.
        /// </summary>
        public string ApiURL { get; set; } = String.Empty;
        /// <summary>
        /// URL para obter o Token de autenticação para acesso ao Promicnet.
        /// </summary>
        public string CognitoURL { get; set; } = String.Empty;
        /// <summary>
        /// URL para conexão do WebSocket.
        /// </summary>
        public string WebSocketURL { get; set; } = String.Empty;
        /// <summary>
        /// Client ID para obter o Token de autenticação.
        /// </summary>
        public string MasterID { get; set; } = String.Empty;
        /// <summary>
        /// CLient Secret para obter o Token de autenticação.
        /// </summary>
        public string MasterSK { get; set; } = String.Empty;
    }
}
