using Newtonsoft.Json;
using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    class FieldComparisonActivityChooser : IActivityChooser
    {
        public static class CommonParameterNames
        {
            public const string Ulo = "ulo";
            public const string Workflow = "wf";
        }

        public class Expression
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("workflowActivityKey")]
            public string WorkflowActivityKey { get; set; }
        }

        public class MySettings
        {
            [JsonProperty("fieldVals")]
            public IList<Expression> Expressions { get; set; }
        }

        string IActivityChooser.GetNextActivityKey(Data.Workflow wf, string settings)
        {
            var s = JsonConvert.DeserializeObject<MySettings>(settings);
            var parameters = new[] {
                new DynamicExpresso.Parameter(CommonParameterNames.Workflow, wf),
                //TODO: Workflow object should contain TargetULo
                //new DynamicExpresso.Parameter(CommonParameterNames.Ulo, wf.TargetUlo)
            };
            foreach (var e in s.Expressions)
            {
                var i = new DynamicExpresso.Interpreter();
                bool res = i.Eval<bool>(e.Code, parameters);
                if (res)
                {
                    return e.WorkflowActivityKey;
                }
            }
            return null;
        }
    }
}
