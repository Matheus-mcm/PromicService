namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class CancelaVotacao : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public int vote_id { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vote_id"></param>
        /// <param name="session_sk"></param>
        public CancelaVotacao(int vote_id, string session_sk)
        {
            this.action = "cancel_vote";
            this.session_id = session_sk;
            this.vote_id = vote_id;
        }
    }
}
