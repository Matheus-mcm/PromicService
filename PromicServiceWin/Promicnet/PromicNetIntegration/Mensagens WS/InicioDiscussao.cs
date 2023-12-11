using System.Collections.Generic;
using static PromicnetIntegration.TypesDTO;

namespace PromicnetWebsocket.Messages
{
        /// <summary>
        /// 
        /// </summary>
        public class InicioDiscussao : Message
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
    
}
