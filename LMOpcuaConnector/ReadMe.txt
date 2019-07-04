Il prgetto OPCUAConnector si divide nelle seguenti aree :

	+ OPCUAClient -> Classe da utlizzare per la connessione al server OPC


Quando viene eseguito il metodo Run della classe OPCUAClient, in base ai parametri viene aperta la comunicazione, ed il tutto viene gestito e salvato nell'
oggetto session della classe.

Per questo motivo al termine del metodo, nulla viene gestito da thread secondari.
