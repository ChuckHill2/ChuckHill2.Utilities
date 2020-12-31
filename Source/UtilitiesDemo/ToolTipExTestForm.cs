using System;
using System.Windows.Forms;
using ChuckHill2.Utilities;

namespace UtilitiesDemo
{
    public partial class ToolTipExTestForm : Form
    {
        private static readonly string[] Words = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.".Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        private ToolTipEx tt = null;

        public ToolTipExTestForm()
        {
            InitializeComponent();
            m_rtfRichTextBox.Rtf = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Calibri;}}{\colortbl ;\red255\green0\blue0;\red0\green176\blue80;\red0\green77\blue187;}\viewkind4\uc1 \pard\sa200\sl276\slmult1\cf1\b\f0\fs22\lang9 R\cf2 T\cf3 F\cf0\b0  Test\par}";
            SetAllTips(this);

            tt = new ToolTipEx(this);
            //tt.UseAnimation = false;
            //tt.UseFading = false;
            //tt.IsBalloon = true;
        }

        Random rand = new Random(0);
        private void SetAllTips(Control cc)
        {
            foreach(Control c in cc.Controls)
            {
                if (string.IsNullOrEmpty(c?.Name)) continue;
                if (c.HasChildren) SetAllTips(c);
                SetTip(c, rand.Next(0, Words.Length), rand.Next(0, Words.Length));
            }
        }
        private void SetTip(Control c, int indexStart, int indexEnd) => c.AccessibleDescription = $"{c.Name}: {Sentence(indexStart, indexEnd)}";
        private static string Sentence(int indexStart, int indexEnd)
        {
            if (indexEnd < 0) indexEnd = 0;
            if (indexEnd > Words.Length - 1) indexEnd = Words.Length - 1;

            if (indexStart < 0) indexStart = 0;
            if (indexStart > Words.Length - 1) indexStart = Words.Length - 1;

            if (indexStart > indexEnd)
            {
                int i = indexStart;
                indexStart = indexEnd;
                indexEnd = i;
            }

            return string.Join(" ", Words, indexStart, indexEnd - indexStart + 1);
        }

        private class LayoutTest
        {
            public ToolTipIcon Icon;
            public string Title;
            public string Message;
            public LayoutTest(ToolTipIcon i,string t,string m) { Icon = i; Title = t; Message = m; }
        }
        private LayoutTest[] _tests = new []
        {
           /* 0*/ new LayoutTest(ToolTipIcon.None,"",""),  //non-functional
           /* 1*/ new LayoutTest(ToolTipIcon.Info,"",""),  //non-functional

           /* 2*/ new LayoutTest(ToolTipIcon.None,"","Simple tooltip, no title, no icon."),
           /* 3*/ new LayoutTest(ToolTipIcon.None,"Tooltip Title w/no msg",""),     //non-functional
           /* 4*/ new LayoutTest(ToolTipIcon.None,"Long Tooltip Title","msg"),
           /* 5*/ new LayoutTest(ToolTipIcon.None,"Tooltip Title","Really long tooltip msg: " + Sentence(0, 18)),
           /* 6*/ new LayoutTest(ToolTipIcon.None,"Really long Tooltip title: " + Sentence(0, 18),"Regular tooltip message."),  //no title
           /* 7*/ new LayoutTest(ToolTipIcon.None,"Regular Title","Really really long tooltip msg: " + Sentence(0, 100)),

           /* 8*/ new LayoutTest(ToolTipIcon.Info,"","Simple tooltip, no title, no icon."),
           /* 9*/ new LayoutTest(ToolTipIcon.Warning,"Tooltip Title w/no msg",""),  //non-functional
           /*10*/ new LayoutTest(ToolTipIcon.Error,"Long Tooltip Title","msg"),
           /*11*/ new LayoutTest(ToolTipIcon.Info,"Tooltip Title","Really long tooltip msg: " + Sentence(0, 18)),
           /*12*/ new LayoutTest(ToolTipIcon.Warning,"Really long Tooltip title: " + Sentence(0, 18),"Regular tooltip message."),  //no title, no icon
           /*13*/ new LayoutTest(ToolTipIcon.Error,"Regular Title","Really really long tooltip msg: " + Sentence(0, 100)),         //no icon

           /*14*/ new LayoutTest(ToolTipIcon.Info,"","Tooltip Layout Test Complete"),
        };

        private int _index = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            //Button click performs some action. This tooltip is the status of that action.

            //Test different tooltip message permutations.
            //Diagnostics.WriteLine($"{_index}. ({(_tests[_index].Icon == 0 ? "" :_tests[_index].Icon.ToString())}) {_tests[_index].Title}\r\n{_tests[_index].Message}");
            //tt?.Show((Control)sender, _tests[_index].Message, _tests[_index].Title, _tests[_index].Icon);
            //_index = ++_index % _tests.Length;

            //Test delay, fading, and timeout.
            tt?.Show((Control)sender, $"({_index++}) I've found that if I copy C#\ncode from off of a website, and\npaste it into Notepad++.", "Title", ToolTipIcon.Info);
        }
    }
}
