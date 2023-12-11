namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando solicitado um KeepAlive
    /// </summary>
    public partial class KeepAliveEvent : EventBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="id_sala"></param>
        public KeepAliveEvent(string action, int id_sala)
        {
            this.Acao = action;
            this.Id_sala = id_sala;
        }
    }
}
