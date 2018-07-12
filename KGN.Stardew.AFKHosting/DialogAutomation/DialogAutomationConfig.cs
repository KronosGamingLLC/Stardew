using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.AFKHosting.DialogAutomation
{
    public class DialogAutomationConfig
    {
        public Festival[] Festivals { get; set; }

        public DialogAutomationConfig()
        {
            Festivals = new Festival[] { new Festival() };
        }
    }

    public class Festival
    {
        public string FestivalId { get; set; }
        public Dialog Dialog { get; set; }

        public Festival()
        {
            FestivalId = "Luaua";
            Dialog = new Dialog();
        }
    }

    public class DialogAction
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public DialogActionType DialogActionType { get; set; }
        public int? AnswerIndex { get; set; }
    }

    public class Dialog
    {
        public string NPCName { get; set; }
        public DialogAction[] actions { get; set; }

        public Dialog()
        {
            NPCName = "Lewis";
            actions = new DialogAction[] { new DialogAction() };
        }
    }
}
