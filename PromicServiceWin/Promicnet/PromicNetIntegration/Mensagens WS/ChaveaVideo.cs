using PromicnetWebsocket.Messages;

namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// Classse responsável pela mensagem de chaveamento de vídeo.
    /// </summary>
    public class ChaveaVideo : Message
    {
        /// <summary>
        /// Email do participante que terá o vídeo ativado
        /// </summary>
        public string user_email { get; private set; }
        /// <summary>
        /// Construtor padrão desta classe.
        /// </summary>
        /// <param name="session_id">ID da sessão no Promicnet.</param>
        /// <param name="user_email">Email do participante.</param>
        public ChaveaVideo(string session_id, string user_email)
        {
            this.action = "select_active_video";
            this.session_id = session_id;
            this.user_email = user_email;
        }
    }
}
