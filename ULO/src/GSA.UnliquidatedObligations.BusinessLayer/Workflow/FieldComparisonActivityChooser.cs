using Newtonsoft.Json;
using System.Collections.Generic;

namespace GSA.UnliquidatedObligations.BusinessLayer.Workflow
{
    public class FieldComparisonActivityChooser : IActivityChooser
    {
        public static class CommonParameterNames
        {
            public const string Ulo = "ulo";
            public const string Workflow = "wf";
            public const string wfQuestion = "wfQuestion";
            public const string SubmitterGroupNames = "groups";
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
            [JsonProperty("expressions")]
            public IList<Expression> Expressions { get; set; }
        }

        string IActivityChooser.GetNextActivityKey(Data.Workflow wf, Data.UnliqudatedObjectsWorkflowQuestion question, string settings, IList<string> submitterGroupNames)
        {
            var s = JsonConvert.DeserializeObject<MySettings>(settings);
            //TODO: pass in questions object
            var parameters = new[] {
                new DynamicExpresso.Parameter(CommonParameterNames.Workflow, wf),
                new DynamicExpresso.Parameter(CommonParameterNames.Ulo, wf.UnliquidatedObligation),
                new DynamicExpresso.Parameter(CommonParameterNames.wfQuestion, question),
                new DynamicExpresso.Parameter(CommonParameterNames.SubmitterGroupNames, submitterGroupNames),
            };
            var i = new DynamicExpresso.Interpreter();
            foreach (var e in s.Expressions)
            {
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
