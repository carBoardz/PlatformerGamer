using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class AsyncHealper
{
    //基于回调，无轮询
    public static Task AwaitAsyncOperation(AsyncOperation operation)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));

        var tcs = new TaskCompletionSource<bool>();
        Action<AsyncOperation> callback = null;
        callback = (op) =>
        {
            tcs.SetResult(true);
            operation.completed -= callback;
        };
        operation.completed += callback;
        return tcs.Task;
    }

    // 兼容UnityWebRequest
    public static Task AwaitWebRequest(UnityWebRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var tcs = new TaskCompletionSource<bool>();

        AsyncOperation asyncOp = request.SendWebRequest();

        Action<AsyncOperation> callback = null;
        callback = (op) =>
        {
            tcs.SetResult(true);
            asyncOp.completed -= callback;

            if (request.result != UnityWebRequest.Result.Success)
            {
                tcs.SetException(new Exception($"WebRequest失败: {request.error}"));
            }
        };

        asyncOp.completed += callback;
        return tcs.Task;
    }
}
