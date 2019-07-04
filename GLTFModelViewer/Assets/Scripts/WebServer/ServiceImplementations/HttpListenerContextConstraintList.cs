#if ENABLE_WINMD_SUPPORT
namespace UwpHttpServer
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    internal class HttpListenerContextConstraintList
    {
        class ListEntry
        {
            public Func<HttpListenerContext, bool> Predicate { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }
        public HttpListenerContextConstraintList()
        {
            this.contextConstraints = new List<ListEntry>();
        }
        public void Add(Func<HttpListenerContext, bool> constraint,
            HttpStatusCode statusCode)
        {
            this.contextConstraints.Add(
                new ListEntry()
                {
                    Predicate = constraint, StatusCode = statusCode
                }
            );
        }
        public HttpStatusCode CheckConstraints(HttpListenerContext context)
        {
            var statusCode = HttpStatusCode.OK;

            foreach (var predicate in this.contextConstraints)
            {
                if (!predicate.Predicate(context))
                {
                    statusCode = predicate.StatusCode;
                    break;
                }
            }
            return (statusCode);
        }
        List<ListEntry> contextConstraints;
    }
}
#endif // ENABLE_WINMD_SUPPORT
