using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Codinlab.SameTermsFilter.Handlers
{
    public class CurrentContentHandler : ContentHandler
    {
        private readonly IWorkContextAccessor _workContextAccessor;

        public CurrentContentHandler(IWorkContextAccessor workContextAccessor)
        {
            _workContextAccessor = workContextAccessor;

            OnGetDisplayShape<TermsPart>((context, part) =>
            {
                if (context.DisplayType == "Detail")
                {
                    var workContext = _workContextAccessor.GetContext();
                    string ContentTermsIds = String.Join(",", part.TermParts.Select(t => t.TermPart.Id).ToArray());
                    workContext.SetState("ContentTermsIds", ContentTermsIds);
                }
            });

            OnGetDisplayShape<TermPart>((context, part) =>
            {
                if (context.DisplayType == "Detail")
                {
                    var workContext = _workContextAccessor.GetContext();
                    workContext.SetState("ContentTermsIds", part.Id.ToString());
                }
            });
        }

    }
}