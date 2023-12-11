namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// 
    /// </summary>
        public class InicioVotacao : Message
        {
            /// <summary>
            /// 
            /// </summary>
            public int vote_id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string[] types_allowed { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int vote_time { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int user_id { get; set; }
        
    }
}
