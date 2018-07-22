using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.AspNetCore.Diagnostics;

namespace Basics.MiddleWares
{
    public class RequestCultureMiddleWare
    {
        RequestDelegate Next;
        public RequestCultureMiddleWare(RequestDelegate next)
        {
            Next = next;
        }
        // bring scoped instances here not ctor because middlewares have apllication liftime injection and ctor just called once
        public Task InvokeAsync(HttpContext context)
        {
            string culture = context.Request.Query["culture"];
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo cInfo = new CultureInfo(culture);
                CultureInfo.CurrentCulture = cInfo;
                CultureInfo.CurrentUICulture = cInfo;
            }
            //we can disable some features in middleware or any where
            var feature = context.Features.Get<IStatusCodePagesFeature>();
            if (feature != null)
            {
                feature.Enabled = false;
            }
            return Next.Invoke(context);
        }
    }
}
