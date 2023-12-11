
namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class AtualizaTempoOrador : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public int user_id { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int time { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="time"></param>
        /// <param name="session_sk"></param>
        public AtualizaTempoOrador(int user_id, int time, string session_sk)
        {
            this.action = "set_speaker_time";
            this.session_id = session_sk;
            this.time = time;
            this.user_id = user_id;
        }
    }
}
