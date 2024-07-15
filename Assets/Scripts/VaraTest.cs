using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SubstrateClient : MonoBehaviour
{
    private string url = "http://127.0.0.1:9944"; // URL del nodo local de Substrate

    void Start()
    {
        StartCoroutine(GetLatestBlock());
    }

    IEnumerator GetLatestBlock()
    {
        // Prepara la solicitud JSON-RPC para obtener el último hash de bloque
        string requestJson = "{\"jsonrpc\":\"2.0\",\"method\":\"chain_getBlockHash\",\"params\":[],\"id\":1}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestJson);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // Procesa la respuesta para obtener el hash del último bloque
            var blockHashResponse = JsonUtility.FromJson<RpcResponse<string>>(request.downloadHandler.text);
            string latestBlockHash = blockHashResponse.result;

            Debug.Log("Latest Block Hash: " + latestBlockHash);

            // Solicita detalles del bloque usando el hash obtenido
            yield return StartCoroutine(GetBlockDetails(latestBlockHash));
        }
    }

    IEnumerator GetBlockDetails(string blockHash)
    {
        // Prepara la solicitud JSON-RPC para obtener los detalles del bloque
        string requestJson = "{\"jsonrpc\":\"2.0\",\"method\":\"chain_getBlock\",\"params\":[\"" + blockHash + "\"],\"id\":1}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestJson);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // Procesa la respuesta para obtener detalles del bloque
            var blockResponse = JsonUtility.FromJson<RpcResponse<BlockResult>>(request.downloadHandler.text);
            Debug.Log("Block Details: " + request.downloadHandler.text);

            // Maneja los datos del bloque según tus necesidades
            Debug.Log("Block Number: " + blockResponse.result.block.header.number);
            Debug.Log("Block State Root: " + blockResponse.result.block.header.state_root);
        }
    }

    [System.Serializable]
    public class RpcResponse<T>
    {
        public string jsonrpc;
        public T result;
        public int id;
    }

    [System.Serializable]
    public class BlockResult
    {
        public Block block;

        [System.Serializable]
        public class Block
        {
            public Header header;

            [System.Serializable]
            public class Header
            {
                public string number;
                public string state_root;
            }
        }
    }
}
