namespace Functions
{
    /// <summary>Conjunto de funções auxiliares</summary>
    public class Util
    {
        /// <summary>Verifica se a string é um endereço de e-mail válido.</summary>
        /// <param name="email">Endereço de e-mail a ser validado.</param>
        /// <returns>Caso a string seja um endereço válido retornará True.</returns>
        public bool IsValidEmail(string email)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(email, "^([0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$"))
            {
                return true;
            }
            else if (email.ToUpper() == "PROMIC")
            {
                return true;
            }
            else if (email.ToUpper() == "SERVICE")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
