using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SV2.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;span&gt; elements with an <c>asp-description-for</c> attribute.
    /// Adds an <c>id</c> attribute and sets the content of the &lt;span&gt; with the Description property from the model data annotation DisplayAttribute.
    /// </summary>
    [HtmlTargetElement("span", Attributes = DescriptionForAttributeName)]
    public class SpanDescriptionTagHelper : TagHelper
    {
        private const string DescriptionForAttributeName = "asp-description-for";

        /// <summary>
        /// Creates a new <see cref="SpanDescriptionTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public SpanDescriptionTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        public override int Order
        {
            get { return int.MinValue; }
        }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        protected IHtmlGenerator Generator { get; }

        /// <summary>
        /// An expression to be evaluated against the current model.
        /// </summary>
        [HtmlAttributeName(DescriptionForAttributeName)]
        public ModelExpression DescriptionFor { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="DescriptionFor"/> is <c>null</c>.</remarks>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var metadata = DescriptionFor.Metadata;

            if (metadata == null)
            {
                throw new InvalidOperationException(string.Format("No provided metadata ({0})", DescriptionForAttributeName));
            }

            output.Attributes.SetAttribute("id", metadata.PropertyName + "-description");

            if (!string.IsNullOrWhiteSpace(metadata.Description))
            {
                output.Content.SetContent(metadata.Description);
                output.TagMode = TagMode.StartTagAndEndTag;
            }
        }
    }
}