namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Enumeração com os tipos possíveis de votação.
    /// </summary>
    public enum TipoVotacao
    {
        /// <summary>
        /// Tipos de votação possíveis são SIM e NÃO.
        /// </summary>
        SimNao = 3,
        /// <summary>
        /// Tipos de votação possíveis são SIM, NÃO e ABSTENÇÃO.
        /// </summary>
        SimNaoAbs = 0,
        /// <summary>
        /// Tipos de votação possíveis são UM, DOIS e TRÊS.
        /// </summary>
        UmDoisTres = 1,
        /// <summary>
        /// Tipos de votação possíveis são UM, DOIS, TRÊS, QUATRO e CINCO.
        /// </summary>
        UmACinco = 2,
        /// <summary>
        /// Tipos de votação possíveis são CHECHLIST.
        /// </summary>
        Checklist = 4,
        /// <summary>
        /// Tupos de votação possíveis são NOTA.
        /// </summary>
        Nota = 5

    }
}

