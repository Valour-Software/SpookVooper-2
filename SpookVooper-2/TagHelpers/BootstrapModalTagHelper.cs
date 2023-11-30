using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace SV2.TagHelpers;

public class BootstrapModalTagHelper : TagHelper 
{
    [HtmlAttributeName("asp-modal-title")]
    public string ModalTitle { get; set; }

    [HtmlAttributeName("asp-modal-id")]
    public string ModalId { get; set; }

    [HtmlAttributeName("asp-modal-to-open-on-close-id")]
    public string? ModalToOpenOnCloseId { get; set; }

    [HtmlAttributeName("asp-modal-extraclasses")]
    public string? ExtraClasses { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output) {
        output.PreContent.SetHtmlContent($@"<div class=""modal fade {ExtraClasses}"" id=""{ModalId}"" tabindex=""-1"" aria-labelledby=""exampleModalLabel"" aria-hidden=""true"">
    <div class=""modal-dialog"">
        <div class=""modal-content"" data-bs-theme=""dark"">
            <div class=""modal-header"">
                <h1 class=""modal-title fs-5"" id=""exampleModalLabel"">{ModalTitle}</h1>
                <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal"" aria-label=""Close""></button>
            </div>
            <div class=""modal-body"">");
        if (ModalToOpenOnCloseId is null) {
            output.PostContent.SetHtmlContent($@"</div>
            <div class=""modal-footer"">
                <button type=""button"" class=""btn btn-secondary"" data-bs-dismiss=""modal"">Okay</button>
            </div>
        </div>
    </div>
</div>");
        }
        else {
            output.PostContent.SetHtmlContent($@"</div>
            <div class=""modal-footer"">
                <button type=""button"" class=""btn btn-secondary"" data-bs-toggle=""modal"" data-bs-target=""#{ModalToOpenOnCloseId}"">Go Back</button>
            </div>
        </div>
    </div>
</div>");
        }
    }
}