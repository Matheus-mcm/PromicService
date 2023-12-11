namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando a situação de um microfone for alterada
    /// </summary>
    public partial class SitaucaoMicrofoneAlteradaEvent :EventBase
    {
        /// <summary>
        /// Situação do microfone
        /// </summary>
        public string State { get; }
        /// <summary>
        /// Construtor padrão deste evento
        /// </summary>
        /// <param name="action">Ação que disparou este evento</param>
        /// <param name="state">Situação do microfone</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public SitaucaoMicrofoneAlteradaEvent(string action, string state, int id_sala)
        {
            this.Acao = action;
            this.State = state;
            this.Id_sala += id_sala;
        }
    }
}
