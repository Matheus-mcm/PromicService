using PromicnetWebsocket.Messages;
using System.CodeDom;

namespace PromicnetIntegration
{
    /// <summary>
    /// 
    /// </summary>
    public class TempoIncricao : Message
    {
        /// <summary>
        /// Tempo máximo para oradores se inscreverem, em segundos.
        /// </summary>
        public int time { get; set; }
        /// <summary>
        /// Construtor padrão da classe de inicio de tempo de inscrição.
        /// </summary>
        /// <param name="session_sk">SK da sessão.</param>
        /// <param name="time">Tempo máximo para oradores se inscreverem, em segundos.</param>
        public TempoIncricao(string session_sk, int time)
        {
            action = "initiate_speaker_subscribe_time";
            session_id = session_sk;
            this.time = time;
        }
    }
}