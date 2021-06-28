using System.Collections.Generic;
using System.Diagnostics;

namespace Faaast.SeoRouter
{
    [DebuggerDisplay("Controller = {Controller}, Action = {Action}, AdditionalValues = {AdditionalValues}")]
    public struct MvcAction
    {
        public string Controller { get; internal set; }
        public string Action { get; internal set; }
        public string AdditionalValues { get; internal set; }
        public string Constraints { get; internal set; }

        public IDictionary<string, object> DefaultValues { get; internal set; }

        public MvcAction(string controller, string action, string additionalValues) : this(controller, action, additionalValues, null)
        {
        }

        public MvcAction(string controller, string action, string additionalValues, string constraints)
        {
            Controller = controller.ToLower();
            Action = action.ToLower();
            AdditionalValues = additionalValues?.ToLower();
            Constraints = constraints?.ToLower();
            DefaultValues = AdditionalValues.GetQueryDictionnary();
        }
    }
}
