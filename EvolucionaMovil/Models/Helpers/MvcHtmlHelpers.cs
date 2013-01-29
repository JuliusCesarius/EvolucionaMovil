using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq.Expressions;

namespace EvolucionaMovil.Models.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString SpanFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression)
        {
            var valueGetter = expression.Compile();
            var value = valueGetter(helper.ViewData.Model);

            var span = new TagBuilder("span");
            span.SetInnerText(value.ToString());

            return MvcHtmlString.Create(span.ToString());
        }
        public static MvcHtmlString SpanFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            var valueGetter = expression.Compile();
            var value = valueGetter(helper.ViewData.Model);

            var span = new TagBuilder("span");
            span.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            span.SetInnerText(value.ToString());

            return MvcHtmlString.Create(span.ToString());
        }
         private const string Nbsp = "&nbsp;";
        private const string SelectedAttribute = " selected='selected'";
        public static MvcHtmlString NbspIfEmpty(this HtmlHelper helper, string value)
        {
            return new MvcHtmlString(string.IsNullOrEmpty(value) ? Nbsp : value);
        }
        public static MvcHtmlString SelectedIfMatch(this HtmlHelper helper, object expected, object actual)
        {
            return new MvcHtmlString(Equals(expected, actual) ? SelectedAttribute : string.Empty);
        }

    }
}