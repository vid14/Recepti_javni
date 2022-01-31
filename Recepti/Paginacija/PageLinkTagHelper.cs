using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Recepti.Paginacija
{
    // Paginacija tablice
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PageLinkTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory urlHelperFactory;

        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public PaginacijaInfo PageModel { get; set; }
        public string PageAction { get; set; }
        public bool PageClassesEnabled { get; set; }
        public string PageClass { get; set; }
        public string PageClassNormal { get; set; }
        public string PageClassSelected { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
            TagBuilder result = new TagBuilder("div");
            
            // Prva stranica tablice
            TagBuilder prviTag = new TagBuilder("a");
            string prviUrl=PageModel.urlParameter.Replace(":", "1");
            prviTag.Attributes["href"] = prviUrl;
            prviTag.InnerHtml.Append("Prva");
            if(PageClassesEnabled)
            {
                prviTag.AddCssClass(PageClass);
            }
            result.InnerHtml.AppendHtml(prviTag);

            // Stranice tablice (gumbici)
            for (int i = 1; i <= PageModel.sveStranice; i++)
            {
                TagBuilder tag = new TagBuilder("a");
                string url = PageModel.urlParameter.Replace(":", i.ToString());
                tag.Attributes["href"] = url;
                if (PageClassesEnabled)
                {
                    tag.AddCssClass(PageClass);
                    tag.AddCssClass(i == PageModel.TrenutnaStranica ? PageClassSelected : PageClassNormal);
                    
                    // Ogranicavanje broja gumbica koji se prikazuju na ekranu kada je pretraga sa puno vise rezultata (5 lijevo i desno)
                    if(i< PageModel.TrenutnaStranica - 5 || i> PageModel.TrenutnaStranica + 5)
                    {
                        tag.AddCssClass("d-none");                   
                    }                    
                }
                tag.InnerHtml.Append(i.ToString());
                result.InnerHtml.AppendHtml(tag);
            }

            // Zadnja stranica tablice
            TagBuilder zadnjiTag = new TagBuilder("a");
            string zadnjiUrl = PageModel.urlParameter.Replace(":", PageModel.sveStranice.ToString());
            zadnjiTag.Attributes["href"] = zadnjiUrl;
            zadnjiTag.InnerHtml.Append("Zadnja");
            if (PageClassesEnabled)
            {
                zadnjiTag.AddCssClass(PageClass);
            }
            result.InnerHtml.AppendHtml(zadnjiTag);

            output.Content.AppendHtml(result.InnerHtml);
        }
    }
}
