namespace PromicnetWebsocket.Messages

{
    /// <summary>
    /// 
    /// </summary>
    public class InicioParte : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public int id { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string name { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool speaker { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="speaker"></param>
        /// <param name="session_sk"></param>
        public InicioParte(int id, string name, bool speaker, string session_sk)
        {
            action = "initiate_part";
            session_id = session_sk;
            this.id = id;
            this.name = name;
            this.speaker = speaker;
        }
    }
}
