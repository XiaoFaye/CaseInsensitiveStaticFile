using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;

namespace CaseInsensitiveStaticFile
{
    public class CaseInsensitiveStaticFileMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Dictionary<string, string> StaticFileProvider;
        private readonly bool Debug;

        public CaseInsensitiveStaticFileMiddleware(RequestDelegate next, Dictionary<string, string> staticFileProvider, bool debug = false)
        {
            _next = next;
            StaticFileProvider = staticFileProvider;
            Debug = debug;

            foreach (var provider in StaticFileProvider)
            {
                if (!Directory.Exists(provider.Value))
                    throw new DirectoryNotFoundException($"StaticFile provider: '{provider.Value}' not exists.");
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (StaticFileProvider == null || StaticFileProvider.Count == 0 ||
                context.Request.Path.ToString() == "/" || RuntimeInformation.IsOSPlatform(OSPlatform.Windows))

                await _next(context);
            else
            {
                var askingPath = HttpUtility.UrlDecode(context.Request.Path.ToString().ToLower());
                var segs = askingPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                string localPath = string.Empty;
                string requestFile = string.Empty;
                string resultFile = string.Empty;
                string resultFolder = string.Empty;
                KeyValuePair<string, string> provider = new KeyValuePair<string, string>();

                Console.WriteLine($"Asking path: {askingPath}");
                Console.WriteLine($"Segs count: {segs.Length}");

                if (segs.Length == 1)
                {
                    if (StaticFileProvider.ContainsKey("/"))
                    {
                        requestFile = segs[0];
                        localPath = StaticFileProvider["/"];
                        resultFile = Array.Find(Directory.GetFiles(localPath), x => Path.GetFileName(x).ToLower() == requestFile);
                        if (string.IsNullOrEmpty(resultFile))
                        {
                            resultFolder = Array.Find(Directory.GetDirectories(localPath), x => Path.GetFileName(x).ToLower() == requestFile);

                            if (string.IsNullOrEmpty(resultFolder))
                                await _next(context);
                            else
                            {
                                context.Response.Redirect($"/{Path.GetFileName(resultFolder)}");
                                return;
                            }
                        }
                        else
                        {
                            context.Response.Redirect($"/{Path.GetFileName(resultFile)}");
                            return;
                        }
                    }
                    else
                        await _next(context);
                }
                else
                {
                    for (int i = 0; i < segs.Length; i++)
                    {
                        var seg = segs[i];

                        if (i == 0)
                        {
                            provider = StaticFileProvider.FirstOrDefault(x => x.Key.ToLower() == $"/{seg}");
                            if (string.IsNullOrEmpty(provider.Key))
                                break;
                            else
                                localPath = provider.Value;
                        }
                        else if (i == segs.Length - 1)
                        {
                            resultFile = Array.Find(Directory.GetFiles(localPath), x => Path.GetFileName(x).ToLower() == seg);
                            if (string.IsNullOrEmpty(resultFile))
                            {
                                resultFolder = Array.Find(Directory.GetDirectories(localPath), x => Path.GetFileName(x).ToLower() == requestFile);

                                if (string.IsNullOrEmpty(resultFolder))
                                    break;
                                else
                                {
                                    context.Response.Redirect($"{provider.Key}/{resultFolder.Replace(provider.Value, "").TrimStart(Path.DirectorySeparatorChar)}");
                                    return;
                                }
                            }
                            else
                            {
                                context.Response.Redirect($"{provider.Key}/{resultFile.Replace(provider.Value, "").TrimStart(Path.DirectorySeparatorChar)}");
                                return;
                            }
                        }
                        else
                        {
                            resultFolder = Array.Find(Directory.GetDirectories(localPath), x => Path.GetFileName(x).ToLower() == seg);
                            if (string.IsNullOrEmpty(resultFolder))
                                break;
                            else
                                localPath = resultFolder;
                        }
                    }

                    await _next(context);
                }
            }
        }
    }

    public static class StaticFileMiddlewareExtensions
    {
        public static IApplicationBuilder UseCaseInsensitiveStaticFile(this IApplicationBuilder builder, Dictionary<string, string> staticFileProvider, bool debug = false)
        {
            return builder.UseMiddleware<CaseInsensitiveStaticFileMiddleware>(staticFileProvider, debug);
        }
    }
}
