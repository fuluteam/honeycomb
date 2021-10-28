

using System.Collections.Generic;
using Fabron.Mando;

namespace HoneyComb.Commands
{
    /// <summary>
    /// Http请求命令
    /// </summary>
    /// <param name="Url">请求Url</param>
    /// <param name="HttpMethod">请求方法</param>
    /// <param name="Headers">请求头</param>
    /// <param name="PayloadJson">请求体</param>
    public record InvokeHttpRequest(
        string Url,
        string HttpMethod,
        Dictionary<string, string>? Headers = null,
        string? PayloadJson = null
    ) : ICommand<InvokeHttpRequestResult>;

    /// <summary>
    /// Http请求结果
    /// </summary>
    /// <param name="StatusCode">返回码</param>
    public record InvokeHttpRequestResult(
        int StatusCode,
        string? ResponseBody
    );
}
