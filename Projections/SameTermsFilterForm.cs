using Orchard.DisplayManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Codinlab.SameTermsFilter.Projections
{
    public interface IFormProvider : IEventHandler
    {
        void Describe(dynamic context);
    }

    public class SameTermsFilterForm : IFormProvider
    {
        private readonly ITaxonomyService _taxonomyService;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public SameTermsFilterForm(
            IShapeFactory shapeFactory,
            ITaxonomyService taxonomyService)
        {
            _taxonomyService = taxonomyService;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic context)
        {
            Func<IShapeFactory, object> form =
                shape =>
                {

                    var f = Shape.Form(
                        Id: "SelectSameTermsMethod",
                        _Exclusion: Shape.FieldSet(
                            _OperatorOneOf: Shape.Radio(
                                Id: "operator-is-one-of", Name: "Operator",
                                Title: T("Is one of"), Value: "0", Checked: true
                                ),
                            _OperatorIsAllOf: Shape.Radio(
                                Id: "operator-is-all-of", Name: "Operator",
                                Title: T("Is all of"), Value: "1"
                                )
                            )
                        );

                    return f;
                };

            context.Form("SelectSameTermsMethod", form);
        }
    }
}