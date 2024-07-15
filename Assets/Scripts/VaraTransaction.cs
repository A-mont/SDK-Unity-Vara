using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class SubstrateTransactionClient : MonoBehaviour
{
    private string url = "http://127.0.0.1:9933"; // URL del nodo local de Substrate

    void Start()
    {
        StartCoroutine(SendTransaction());
    }

    IEnumerator SendTransaction()
    {
        // Claves públicas de Alice y Bob (por propósitos de prueba)
        string alicePublicKey = "5GrwvaEF5zXb26Fz9rcQpDWSxZVdMwXXbYHKyox7KXYs5xH5";
        string bobPublicKey = "5FHneW46xGXgs5mUiveU4sbTyGBzmstR6uTrzh4qKPdY4hqm";
        string amount = "1000000000000"; // Cantidad a enviar (en plancks)

        // Prepara la solicitud JSON-RPC para obtener el nonce de Alice
        string nonceRequestJson = "{\"jsonrpc\":\"2.0\",\"method\":\"system_accountNextIndex\",\"params\":[\"" + alicePublicKey + "\"],\"id\":1}";
        byte[] nonceRequestBody = Encoding.UTF8.GetBytes(nonceRequestJson);

        UnityWebRequest nonceRequest = new UnityWebRequest(url, "POST");
        nonceRequest.uploadHandler = new UploadHandlerRaw(nonceRequestBody);
        nonceRequest.downloadHandler = new DownloadHandlerBuffer();
        nonceRequest.SetRequestHeader("Content-Type", "application/json");

        yield return nonceRequest.SendWebRequest();

        if (nonceRequest.result == UnityWebRequest.Result.ConnectionError || nonceRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(nonceRequest.error);
        }
        else
        {
            // Procesa la respuesta para obtener el nonce
            var nonceResponse = JsonUtility.FromJson<RpcResponse<string>>(nonceRequest.downloadHandler.text);
            string nonce = nonceResponse.result;

            Debug.Log("Nonce: " + nonce);

            // Firma y envía la transacción
            yield return StartCoroutine(SendSignedTransaction(alicePublicKey, bobPublicKey, amount, nonce));
        }
    }

    IEnumerator SendSignedTransaction(string sender, string recipient, string amount, string nonce)
    {
        // Aquí debes firmar la transacción con la clave privada de Alice
        // Este ejemplo asume que la transacción ya está firmada
        // En un entorno real, usa una librería adecuada para firmar la transacción

        string signedTransaction = "0x..."; // Reemplaza con la transacción firmada

        // Prepara la solicitud JSON-RPC para enviar la transacción
        string requestJson = "{\"jsonrpc\":\"2.0\",\"method\":\"author_submitExtrinsic\",\"params\":[\"" + signedTransaction + "\"],\"id\":1}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);

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
            // Procesa la respuesta
            var response = JsonUtility.FromJson<RpcResponse<string>>(request.downloadHandler.text);
            Debug.Log("Transaction Hash: " + response.result);
        }
    }

    [System.Serializable]
    public class RpcResponse<T>
    {
        public string jsonrpc;
        public T result;
        public int id;
    }
}
