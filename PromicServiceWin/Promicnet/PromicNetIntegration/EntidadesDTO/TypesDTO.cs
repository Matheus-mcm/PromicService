using System.Collections.Generic;

namespace PromicnetIntegration
{
    /// <summary>
    /// 
    /// </summary>
    public class TypesDTO
    { /// <summary>
      /// Classe de retorno dos métodos
      /// </summary>
        public partial class Result<T>
        {
            /// <summary>
            /// Dados retornados em lista
            /// </summary>
            public T Items { get; set; }
            /// <summary>
            /// Dados retornados em lista
            /// </summary>
            public T Clients { get; set; }
            /// <summary>
            /// Dados retornados
            /// </summary>
            public T Item { get; set; }
            /// <summary>
            /// Dados retornados
            /// </summary>
            public T Sessions { get; set; }
            /// <summary>
            /// Dados retornados
            /// </summary>
            public T Session_users { get; set; }
            /// <summary>
            /// Dados retornados
            /// </summary>
            public T Connected_users { get; set; }
            /// <summary>
            /// Dados retornados
            /// </summary>
            public T Agora_usage { get; set; }
        }
        /// <summary>
        /// Classe responsável por gerenciar os clientes usados na integração com a API da PromicNet.
        /// Orgao = Casa legislativa
        /// </summary>
        public partial class ClientDTO
        {
            /// <summary>
            /// Chave primária do Orgao
            /// </summary>
            public string Client_id { get; set; }
            /// <summary>
            /// Nome do Orgao
            /// </summary>
            public string Client_name { get; set; }
            /// <summary>
            /// /Lista de todos os usuários do cliente
            /// </summary>
            public System.Collections.Generic.List<string> Client_users { get; set; }
            /// <summary>
            /// Número de participantes remotos
            /// </summary>
            public int Max_session_participants { get; set; }
        }
        /// <summary>
        /// Classe responsável por gerenciar os usuários usados na integração com a API da PromicNet.
        /// Usuário = Parlamentar
        /// </summary>
        public partial class UserDTO
        {
            /// <summary>
            /// User_email do usuário
            /// </summary>
            public string Email { get; set; }
            /// <summary>
            /// ID do Orgao
            /// </summary>
            public string Client_id { get; set; }
            /// <summary>
            /// Nome completo do usuário
            /// </summary>
            public string Full_name { get; set; }
            /// <summary>
            /// ID do usuário no Promic
            /// </summary>
            public int User_id { get; set; }
        }
        /// <summary>
        /// Classe responsável por gerenciar as sessões plenárias na interação com a API da PromicNet.
        /// </summary>
        public partial class SessionDTO
        {
            /// <summary>
            /// Status da sessão
            /// </summary>
            public string Status { get; set; }
            /// <summary>
            /// Nome da sessão
            /// </summary>
            public string Session_name { get; set; }
            /// <summary>
            /// Data de início da sesão
            /// </summary>
            public string Initial_date { get; set; }
            /// <summary>
            /// Nome da sessão
            /// </summary>
            public string Session_id { get; set; }
            /// <summary>
            /// ID do órgão no Promicnet
            /// </summary>
            public string Client_id { get; set; }
        }
        /// <summary>
        /// Classe responsável por gerenciar os participantes da sessão plenária
        /// </summary>
        public partial class ParticipantDTO
        {
            /// <summary>
            /// User_email/login do participante
            /// </summary>
            public string Agora_id { get; set; }

            /// <summary>
            /// User_email/login do participante
            /// </summary>
            public string User_email { get; set; }
            /// <summary>
            /// Situação do micrfofone do participante
            /// </summary>
            public string Mic_state { get; set; }
            /// <summary>
            /// Situação da câmera do participante
            /// </summary>
            public string Camera_state { get; set; }
        }
        /// <summary>
        /// Classe responsável por gerenciar o token do Agora
        /// </summary>
        public partial class AgoraTokenDTO
        {
            /// <summary>
            /// Token do Agora
            /// </summary>
            public string Token { get; set; }
            /// <summary>
            /// Canal do Agora
            /// </summary>
            public string Channel { get; set; }
            /// <summary>
            /// ID fixo do Agora
            /// </summary>
            public string Appid { get; set; }
        }
        /// <summary>
        /// Classe responsável por gerenciar as sessões que registram algum uso no Agora.
        /// </summary>
        public partial class UsageSessionDTO
        {
            /// <summary>
            /// ID da Reunião no Promicnet
            /// </summary>
            public string Session_id { get; set; }
            /// <summary>
            /// Nome da sessão
            /// </summary>
            public string Session_name { get; set; }
            /// <summary>
            /// ID do órgão no Promicnet
            /// </summary>
            public string Client_id { get; set; }
        }
        /// <summary>
        /// Classe responsável por gerenciar as informações de consumo do Agora
        /// </summary>
        public partial class AgoraUsageDTO
        {
            /// <summary>
            /// Email do participante 
            /// </summary>
            public string User_email { get; set; }
            /// <summary>
            /// Conectado à
            /// </summary>
            public string Connected_to { get; set; }
            /// <summary>
            /// Data de início do consumo
            /// </summary>
            public string Start_date { get; set; }
            /// <summary>
            /// Data de fim do consumo
            /// </summary>
            public string End_date { get; set; }
            /// <summary>
            /// Tempo de uso
            /// </summary>
            public string Seconds { get; set; }
            /// <summary>
            /// Tipo do consumo
            /// </summary>
            public string Type { get; set; }
            /// <summary>
            /// ID do uso
            /// </summary>
            public string Usage_id { get; set; }
        }
        /// <summary>
        /// Classe responsável por gerenciar os votos.
        /// </summary>
        public partial class VotoDTO
        {
            /// <summary>
            /// Nome do participante votante.
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Partido do participante votante.
            /// </summary>
            public string party { get; set; }
            /// <summary>
            /// Voto deste participante.
            /// </summary>
            public string vote { get; set; }
            /// <summary>
            /// Indicador se o voto do participante foi registrado ou não
            /// </summary>
            public bool voted { get; set; }
        }
        /// <summary>
        /// Autor de uma matéria
        /// </summary>
        public partial class AuthorDTO
        {
            /// <summary>
            /// Nome do delegado
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// Partido
            /// </summary>
            public string party { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class MateriaDTO
        {
            /// <summary>
            /// 
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string acronym { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<AuthorDTO> authors { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string description { get; set; }
        }
        /// <summary>
        /// Parte de uma sessão.
        /// </summary>
        public class PautaDTO
        {
            /// <summary>
            /// 
            /// </summary>
            public string action { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string session_id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<ParteDTO> parts { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class ParteDTO
        {
            /// <summary>
            /// 
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public bool speaker { get; set; } = true;
            /// <summary>
            /// 
            /// </summary>
            public List<MateriaDTO> subjects { get; set; }
        }
    }
}
