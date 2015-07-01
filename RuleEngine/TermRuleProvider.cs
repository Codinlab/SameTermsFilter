using Orchard;
using Orchard.Mvc;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Widgets.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Codinlab.SameTermsFilter.RuleEngine
{
    public class TermRuleProvider : IRuleProvider
    {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ITaxonomyService _taxonomyService;

        public TermRuleProvider(
            IWorkContextAccessor workContextAccessor,
            ITaxonomyService taxonomyService
        )
        {
            _workContextAccessor = workContextAccessor;
            _taxonomyService = taxonomyService;
        }

        public void Process(RuleContext ruleContext)
        {
            if (!String.Equals(ruleContext.FunctionName, "contentterm", StringComparison.OrdinalIgnoreCase)
                || ruleContext.Arguments[0] == null || String.IsNullOrWhiteSpace(ruleContext.Arguments[0].ToString())
                || ruleContext.Arguments[1] == null || String.IsNullOrWhiteSpace(ruleContext.Arguments[1].ToString())
            )
            {
                return;
            }
            ruleContext.Result = false;

            var termIds = _workContextAccessor.GetContext().GetState<string>("ContentTermsIds");

            if (String.IsNullOrWhiteSpace(termIds))
            {
                return;
            }

            var ids = termIds.Split(new[] { ',' }).Select(Int32.Parse).ToArray();

            if (ids.Length == 0)
            {
                return;
            }

            string taxonomyName = ruleContext.Arguments[0].ToString();
            var taxonomyPart = _taxonomyService.GetTaxonomyByName(taxonomyName);

            if(taxonomyPart == null)
            {
                return;
            }

            string termName = ruleContext.Arguments[1].ToString();
            var termPart = _taxonomyService.GetTermByName(taxonomyPart.Id, termName);

            var terms = ids.Select(_taxonomyService.GetTerm).ToList();
            var allChildren = new List<TermPart>();
            foreach (var term in terms)
            {
                allChildren.AddRange(_taxonomyService.GetChildren(term));
                allChildren.Add(term);
            }

            allChildren = allChildren.Distinct().ToList();

            var allIds = allChildren.Select(x => x.Id).ToList();

            if (allIds.Contains(termPart.Id))
            {
                ruleContext.Result = true;
                return;
            }

        }

    }
}