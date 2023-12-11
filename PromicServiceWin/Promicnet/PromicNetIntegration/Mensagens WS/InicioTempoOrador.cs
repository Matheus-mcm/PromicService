namespace PromicnetWebsocket.Messages
{
        /// <summary>
        /// 
        /// </summary>
        public class InicioTempoOrador : Message
        {
            /// <summary>
            /// 
            /// </summary>
            public int user_id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int time { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public bool turn_on_mic { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public bool turn_off_mic { get; set; }
        }
    }

