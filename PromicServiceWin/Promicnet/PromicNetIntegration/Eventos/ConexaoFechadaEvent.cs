using PromicnetIntegration.Events;

namespace PromicnetIntegration
{
    /// <summary>
    /// 
    /// </summary>
    public class ConexaoFechadaEvent : EventBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ushort Code { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Reason { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int id_sala { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        /// <param name="id_sala"></param>
        public ConexaoFechadaEvent(ushort code, string reason, int id_sala)
        {
            Code = code;
            Reason = reason;
            this.id_sala = id_sala;
        }
    }
}