namespace PromicnetIntegration.Types
{
    static class TipoVotacaoMethods
    {
        public static string[] GetString(this TipoVotacao tv)
        {
            switch (tv)
            {
                case TipoVotacao.SimNao:
                    return new string[] { "SIM", "NÃO" };
                case TipoVotacao.SimNaoAbs:
                    return new string[] { "SIM", "NÃO", "ABS" };
                case TipoVotacao.Checklist:
                    return new string[] { "Checklist" };
                case TipoVotacao.UmACinco:
                    return new string[] { "1", "2", "3", "4", "5" };
                case TipoVotacao.UmDoisTres:
                    return new string[] { "1", "2", "3" };
                case TipoVotacao.Nota:
                    return new string[] { "nota" };
                default: return new string[] { };
            }
        }
    }
}

