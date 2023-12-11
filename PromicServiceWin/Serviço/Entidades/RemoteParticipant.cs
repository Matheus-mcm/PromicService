namespace PromicService.Entidades
{
    public class RemoteParticipant
    {
        public int id_delegado { get; private set; }
        public string id_promicnet { get; private set; }
        public int id_reuniao { get; private set; }
        public string nome { get; private set; }
        public string login { get; private set; }
        public string status { get; private set; }

        public RemoteParticipant(string id_promicnet, string login, string status)
        {
            this.id_promicnet = id_promicnet;
            this.login = login;
            this.status = status;
        }
        public RemoteParticipant(int id_delegado, string id_promicnet, int id_reuniao, string name, string login, string status)
        {
            this.id_delegado = id_delegado;
            this.id_promicnet = id_promicnet;
            this.id_reuniao = id_reuniao;
            this.nome = name;
            this.login = login;
            this.status = status;
        }
    }
}
