# PROMIC SERVICE UPDATES

## v 1.0.0.20:

- Serviço:
  Implementação do recebimento do evento de finalização do tempo de orador por parte do participante remoto;

- Biblioteca Promicnet:
  Implementação do evento `TempoOradorEncerradoEvent.cs`;
  Tratamento ao receber mensagem `end_speaker_time` via WebSocket;

- Biblioteca PromicDB:
    Implementação do método `TempoOradorEncerrado(int id_delegado, int id_sala)` para encerrar o tempo do orador atual;

## v 1.0.0.19:

- Serviço:
  Correção do tempo enviado na mensagem "initiate_speaker_time";

## v 1.0.0.18:

- Serviço:
  Alteração na alimentação do objeto enviado ao front-end;

- PromicDB:
  Criação da propriedade `LibPromicDB.Oradores.TEMPO`;

## v 1.0.0.17:

- Serviço:
  Corrigido o início de tempo de orador anônimo;

## v 1.0.0.16:

- Serviço:  
  Corrigido o controle de tempo de oradores;

## v 1.0.0.15:

- Serviço: 
  Corrigido a desinscrição de oradores durante uma discussão;
  Corrigido o envio da lista de oradores durante a sessão;

    
