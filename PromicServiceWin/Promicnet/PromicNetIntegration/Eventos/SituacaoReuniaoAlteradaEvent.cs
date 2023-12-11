namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando a situação de uma reunião for alterada
    /// </summary>
    public partial class SituacaoReuniaoAlteradaEvent : EventBase
    {
        /// <summary>
        /// Contrutor padrão deste evento
        /// </summary>
        /// <param name="action">Ação que disparou o evento.</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public SituacaoReuniaoAlteradaEvent(string action, int id_sala)
        {
            this.Acao = action;
            this.Id_sala = id_sala;
        }
    }
}
