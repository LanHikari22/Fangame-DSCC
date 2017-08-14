using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Dialog_Nodes
{
    class TextNode : DialogNode
    {
        public string name { get; set; }
        public string content { get; set; }
        
        public TextNode() : base()
        {
            
        }

        public TextNode(string name, string content)
        {
            this.name = name;
            this.content = content;
        }

    }
}
