namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class EncerraDiscussao : Message
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="session_sk"></param>
        public EncerraDiscussao(string session_sk)
        {
            this.action = "end_subject";
            this.session_id = session_sk;
        }
    }
}
