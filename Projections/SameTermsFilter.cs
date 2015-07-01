using Orchard;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Services;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Codinlab.SameTermsFilter.Projections
{
    public class SameTermsFilter : IFilterProvider 
    {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public Localizer T { get; set; }

        public SameTermsFilter(ITaxonomyService taxonomyService,
            IWorkContextAccessor workContextAccessor)
        {
            _taxonomyService = taxonomyService;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe)
        {
            describe.For("Taxonomy", T("Taxonomy"), T("Taxonomy"))
                .Element("HasSameTerms", T("Has same Terms as content"), T("Categorized content items"),
                    ApplyFilter,
                    DisplayFilter,
                    "SelectSameTermsMethod"
                );
        }

        public void ApplyFilter(FilterContext context)
        {
            var workContext = _workContextAccessor.GetContext();

            var termIds = workContext.GetState<string>("ContentTermsIds");

            if (!String.IsNullOrEmpty(termIds))
            {
                var ids = termIds.Split(new[] { ',' }).Select(Int32.Parse).ToArray();

                if (ids.Length == 0)
                {
                    return;
                }

                int op = Convert.ToInt32(context.State.Operator);
                
                var terms = ids.Select(_taxonomyService.GetTerm).ToList();
                var allChildren = new List<TermPart>();
                foreach (var term in terms)
                {
                    allChildren.AddRange(_taxonomyService.GetChildren(term));
                    allChildren.Add(term);
                }

                allChildren = allChildren.Distinct().ToList();

                var allIds = allChildren.Select(x => x.Id).ToList();

                switch (op)
                {
                    case 0:
                        // is one of
                        Action<IAliasFactory> s = alias => alias.ContentPartRecord<TermsPartRecord>().Property("Terms", "terms").Property("TermRecord", "termRecord");
                        Action<IHqlExpressionFactory> f = x => x.InG("Id", allIds);
                        context.Query.Where(s, f);
                        break;
                    case 1:
                        // is all of
                        foreach (var id in allIds)
                        {
                            var termId = id;
                            Action<IAliasFactory> selector =
                                alias => alias.ContentPartRecord<TermsPartRecord>().Property("Terms", "terms" + termId);
                            Action<IHqlExpressionFactory> filter = x => x.Eq("TermRecord.Id", termId);
                            context.Query.Where(selector, filter);
                        }
                        break;
                }
            }
            else {
                // Don't return any result if content has no term
                Action<IAliasFactory> selector =
                    alias => alias.ContentPartRecord<TermsPartRecord>().Property("Terms", "noterm");
                Action<IHqlExpressionFactory> filter = x => x.Eq("TermRecord.Id", -1);
                context.Query.Where(selector, filter);
            }
        }

        public LocalizedString DisplayFilter(FilterContext context)
        {
            return T("Categorized with same terms as content");
        }
    }
}